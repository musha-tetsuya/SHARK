using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// シングルバトルシーン
/// </summary>
public class SingleBattleScene : BattleSceneBase
{
    /// <summary>
    /// 背景イメージ
    /// </summary>
    [SerializeField]
    private Image bgImage = null;
    /// <summary>
    /// 砲台
    /// </summary>
    [SerializeField]
    private Turret turret = null;
    /// <summary>
    /// HPゲージUI
    /// </summary>
    [SerializeField]
    private UIHpGauge uiHpGauge = null;
    /// <summary>
    /// 魚ゲージUI
    /// </summary>
    [SerializeField]
    private UIFishGauge uiFishGauge = null;
    /// <summary>
    /// FVアタックゲージ
    /// </summary>
    [SerializeField]
    private UIFvAttackGauge uiFvAttackGauge = null;
    /// <summary>
    /// バトルアイテムアイコン管理
    /// </summary>
    [SerializeField]
    private UIBattleItemIconManager battleItemIconManager = null;
    /// <summary>
    /// アニメ演出生成先
    /// </summary>
    [SerializeField]
    private RectTransform animationEffectArea = null;
    /// <summary>
    /// ステート管理
    /// </summary>
    [SerializeField]
    private MyStateManager stateManager = null;

    /// <summary>
    /// ワールドデータ
    /// </summary>
    private Master.SingleWorldData worldData = null;
    /// <summary>
    /// ステージデータ
    /// </summary>
    private Master.SingleStageData stageData = null;
    /// <summary>
    /// ステージ魚ステータスデータ辞書
    /// </summary>
    private Dictionary<uint, Master.SingleStageFishData> stageFishDataList = null;
    /// <summary>
    /// バトルWAVE管理
    /// </summary>
    private FishWaveDataController waveDataController = null;
    /// <summary>
    /// バトルWAVEデータロードハンドル
    /// </summary>
    private AssetLoadHandle waveDataHandle = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// BGMトラック
    /// </summary>
    private BgmTrack bgmTrack = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        //不要になったリソースの破棄
        this.Unload();
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;

        //ステート準備
        this.stateManager.Init();
    }

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public override void OnSceneLoaded(SceneDataPackBase dataPack)
    {
        if (dataPack is ToBattleSceneDataPack)
        {
            var data = dataPack as ToBattleSceneDataPack;
            this.stageData = Masters.SingleStageDB.FindById(data.stageId);
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
#if UNITY_EDITOR && DEBUG
        if (this.stageData == null)
        {
            this.stageData = Masters.SingleStageDB.GetList()[0];
        }
#endif

        this.worldData = Masters.SingleWorldDB.FindById(this.stageData.worldId);
        this.stageFishDataList = Masters.SingleStageFishDB
            .GetList()
            .Where(x => x.stageId == this.stageData.id)
            .ToDictionary(x => x.fishId, x => x);

        //魚用カメラの描画範囲設定
        BattleGlobal.instance.SetFishCameraViewport();

        //バトル用ユーザーデータ作成
        BattleGlobal.instance.userData = new SingleBattleUserData(this.stageData, UserData.Get().GetSelectedTurretData());
        BattleGlobal.instance.userData.turret = this.turret;

        //砲台データ設定
        this.turret.Init(BattleGlobal.instance.userData);

        //HPゲージ, FVゲージ初期値設定
        this.OnHpGaugeUpdate();

        //必要リソースの読み込み
        this.Load();
    }

    /// <summary>
    /// 必要リソースの読み込み
    /// </summary>
    private void Load()
    {
        //WAVEデータ読み込み
        this.waveDataHandle = AssetManager.Load<FishWaveData>(SharkDefine.GetFishWaveDataPath(this.stageData.key), (asset) =>
        {
            //WAVE準備
            this.waveDataController = new FishWaveDataController(new Fish.ID(), asset, 0f);
            this.waveDataController.onFinished = this.OnAllWaveFinished;

            //背景リソース読み込み
            this.loader.Add<Sprite>(SharkDefine.GetBattleBgSpritePath(this.worldData.key));

            //WAVE必要リソース読み込み
            this.loader.AddRange(this.waveDataController.loader);

            //バトルアイテムアイコンリソース読み込み
            this.battleItemIconManager.Set(
                userItemDatas: new UserItemData[]{
                    new UserItemData{ itemType = ItemType.BattleItem, itemId = this.stageData.itemId1, stockCount = this.stageData.amount1 },
                    new UserItemData{ itemType = ItemType.BattleItem, itemId = this.stageData.itemId2, stockCount = this.stageData.amount2 },
                },
                onClick: this.OnClickItemIcon
            );
            this.battleItemIconManager.LoadIfNeed();

            //BGM読み込み
            this.loader.Add<BgmClipData>(SharkDefine.GetBgmClipPath(this.worldData.bgmName));

            //SE読み込み
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.CAPTURE_SINGLE));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.FVATTACK_OK));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.FVATTACK_START));

            //砲台リソース読み込み
            this.loader.AddRange(this.turret.loader);

            //ロード
            this.loader.Load();

            //ロード完了待ち
            StartCoroutine(new WaitWhile(AssetManager.IsLoading).AddCallback(() =>
            {
                //ローディング表示消す
                SharedUI.Instance.HideSceneChangeAnimation();

                //BGM再生
                this.bgmTrack = SoundManager.Instance.PlayBgm(this.worldData.bgmName);

                //背景
                this.bgImage.sprite = this.loader[SharkDefine.GetBattleBgSpritePath(this.worldData.key)].handle.asset as Sprite;

                //バトルWAVEセットアップ
                this.waveDataController.Setup();

                //アイテムアイコンセットアップ
                this.battleItemIconManager.Setup();

                //砲台セットアップ
                this.turret.Setup();

                //FVアタックゲージセットアップ
                this.uiFvAttackGauge.Setup();

                //魚ゲージセットアップ
                this.uiFishGauge.Setup(this.waveDataController.master);

                //準備完了したらバトル開始
                this.stateManager.ChangeState<BattleStartState>();
            }));
        });
    }

    /// <summary>
    /// 不要になったリソースの破棄
    /// </summary>
    private void Unload()
    {
        AssetManager.Unload(this.waveDataHandle);
        this.loader.Unload();
        this.loader.Clear();
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        float deltaTime = Time.deltaTime;

        while (deltaTime > 0f)
        {
            //1回で経過させる時間は最大でも0.0166秒
            float _deltaTime = Mathf.Min(deltaTime, SharkDefine.DELTATIME);

            //ステート更新
            (this.stateManager.currentState as MainState)?.Update(_deltaTime);

            //魚更新
            BattleGlobal.instance.UpdateFish(_deltaTime);

            //次のループのため経過させた時間を引く
            deltaTime -= _deltaTime;
        }
    }

    /// <summary>
    /// 被ダメ時
    /// </summary>
    private void OnDamaged(Fish fish)
    {
        (this.stateManager.currentState as MyState)?.OnDamaged(fish);
    }

    /// <summary>
    /// アイテムアイコンクリック時
    /// </summary>
    private void OnClickItemIcon(UIBattleItemIcon icon)
    {
        (this.stateManager.currentState as MyState)?.OnClickItemIcon(icon);
    }

    /// <summary>
    /// FVアタックボタン押下時
    /// </summary>
    public void OnClickFvAttackButton()
    {
        (this.stateManager.currentState as MyState)?.OnClickFvAttackButton();
    }

    /// <summary>
    /// FVアタック時間終了時
    /// </summary>
    public void OnFinishedFvAttackTime()
    {
        this.uiFvAttackGauge.RefleshGaugeFill();
        this.uiFvAttackGauge.RefleshPercentText();
    }

    /// <summary>
    /// 全WAVE終了時
    /// </summary>
    private void OnAllWaveFinished()
    {
        (this.stateManager.currentState as MyState)?.OnAllWaveFinished();
    }

    /// <summary>
    /// 一時停止ボタン押下時
    /// </summary>
    public void OnClickPauseButton()
    {
        (this.stateManager.currentState as MyState)?.OnClickPauseButton();
    }

    /// <summary>
    /// 魚生成時
    /// </summary>
    public override void OnCreateFish(Fish fish)
    {
        if (this.stageFishDataList.ContainsKey(fish.master.id))
        {
            //ステータスマスターセット
            var fishStatusMaster = this.stageFishDataList[fish.master.id];
            fish.SetStatusMaster(fishStatusMaster);
        }
        else
        {
            Debug.LogWarningFormat("SingleStageFishData: stageId={0} fishId={1} のデータが存在しません", this.stageData.id, fish.master.id);
        }

        //被ダメージ時イベント登録
        fish.onDestroy += this.OnDamaged;
        fish.onDestroy += (f) =>
        { 
            this.uiFishGauge.now--;
            this.uiFishGauge.Reflesh();
        };

        (this.stateManager.currentState as MyState)?.OnCreateFish(fish);
    }

    /// <summary>
    /// 魚捕獲確率計算
    /// </summary>
    public override bool FishCatchingCalculation(Fish fish)
    {
        return fish.hp <= 0;
    }

    /// <summary>
    /// 魚捕獲時
    /// </summary>
    public override void OnFishCatched(Fish fish, Bullet bullet)
    {
        SoundManager.Instance.PlaySe(SeName.CAPTURE_SINGLE);

        if (!bullet.isFvAttack)
        {
            var userData = BattleGlobal.instance.userData;

            //FVポイント増加
            userData.fvPoint += this.CalcFvPointOnFishCatched(fish, bullet);

            //最大値以上にならないよう制限
            if (userData.fvPoint > this.stageData.needFvPoint)
            {
                userData.fvPoint = this.stageData.needFvPoint;
            }

            //FVアタックゲージをセット
            this.uiFvAttackGauge.RefleshView();
        }
    }

    /// <summary>
    /// 自動照準時のターゲット選定処理
    /// </summary>
    public override Fish FindTargetFish()
    {
        //X座標位置 > 残りHP量 > 魚の生成ID(生成IDが古いもの)
        return BattleGlobal.instance.fishList
            .OrderBy(fish => fish.cachedTransform.GetSiblingIndex())
            .OrderBy(fish => fish.hp)
            .OrderBy(fish => fish.cachedTransform.position.x)
            .FirstOrDefault(fish => !fish.isDead && fish.fishCollider2D.IsInScreen());
    }

    /// <summary>
    /// HPゲージ更新
    /// </summary>
    private void OnHpGaugeUpdate()
    {
        var userData = BattleGlobal.instance.userData as SingleBattleUserData;
        this.uiHpGauge.Set(userData.hp, userData.maxHp);
    }

    /// <summary>
    /// 砲台制御機能変更時
    /// </summary>
    public override void OnSetTurretController(ITurretController turretController)
    {
        //砲台制御機能が有効になった時
        if (turretController != null)
        {
            //FVアタックによる砲台制御の場合
            if (turretController is FvAttackBase)
            {
                //FVアタック時間更新に合わせて、FVゲージを変化させる
                (turretController as FvAttackBase).onUpdateTime += this.uiFvAttackGauge.OnUpdateFvAttackTime;
            }

            //FVゲージに禁止マークを表示
            this.uiFvAttackGauge.SetVisibleBanMark(true);
            this.uiFvAttackGauge.RefleshButtonInteractable();

            //砲台制御中に使用出来ないタイプのアイテムのアイコンにも禁止マークを表示
            foreach (var itemIcon in this.battleItemIconManager.icons)
            {
                if (itemIcon.IsTurretController())
                {
                    itemIcon.SetVisibleBanMark(true);
                    itemIcon.RefleshButtonInteractable();
                }
            }
        }
        //砲台制御機能が無効になった時
        else
        {
            //FVゲージの禁止マークを解除
            this.uiFvAttackGauge.SetVisibleBanMark(false);
            this.uiFvAttackGauge.RefleshButtonInteractable();

            //アイテムアイコンの禁止マークも解除
            foreach (var itemIcon in this.battleItemIconManager.icons)
            {
                itemIcon.SetVisibleBanMark(false);
                itemIcon.RefleshButtonInteractable();
            }
        }
    }

    /// <summary>
    /// ステージ選択画面へ
    /// </summary>
    private void ChangeSceneToSingleStageSelect()
    {
        SharedUI.Instance.DisableTouch();
        this.bgmTrack.Stop(0.5f, () =>
        {
            SharedUI.Instance.EnableTouch();
            SceneChanger.ChangeSceneAsync("SingleStageSelect");
        });
    }

    /// <summary>
    /// ステート管理
    /// </summary>
    [Serializable]
    private class MyStateManager : StateManager
    {
        /// <summary>
        /// シーン
        /// </summary>
        [SerializeField]
        public SingleBattleScene scene = null;
        /// <summary>
        /// バトル開始ステート
        /// </summary>
        [SerializeField]
        private BattleStartState battleStartState = null;
        /// <summary>
        /// メインステート
        /// </summary>
        [SerializeField]
        private MainState mainState = null;
        /// <summary>
        /// ボス出現演出ステート
        /// </summary>
        [SerializeField]
        private BossAppearState bossAppearState = null;
        /// <summary>
        /// バトル終了ステート
        /// </summary>
        [SerializeField]
        private BattleEndState battleEndState = null;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            this.AddState(new NoneState());
            this.AddState(this.battleStartState);
            this.AddState(this.mainState);
            this.AddState(this.bossAppearState);
            this.AddState(this.battleEndState);
        }
    }

    /// <summary>
    /// ステート基底
    /// </summary>
    private abstract class MyState : StateBase
    {
        protected SingleBattleScene scene => (this.manager as MyStateManager).scene;
        public virtual void OnDamaged(Fish fish){}
        public virtual void OnClickItemIcon(UIBattleItemIcon icon){}
        public virtual void OnClickFvAttackButton(){}
        public virtual void OnAllWaveFinished(){}
        public virtual void OnClickPauseButton(){}
        public virtual void OnCreateFish(Fish fish){}
#if DEBUG
        public virtual void OnGUI(){}
#endif
    }

    /// <summary>
    /// 空ステート
    /// </summary>
    private class NoneState : MyState
    {
    }

    /// <summary>
    /// バトル開始ステート
    /// </summary>
    [Serializable]
    private class BattleStartState : MyState
    {
        /// <summary>
        /// 開始アニメプレハブ
        /// </summary>
        [SerializeField]
        private AnimationEventReceiver startAnimPrefab = null;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            var anim = Instantiate(this.startAnimPrefab, this.scene.animationEffectArea, false);
            anim.onFinished = (tag) =>
            {
                Destroy(anim.gameObject);
                this.manager.ChangeState<MainState>();
            };
        }
    }

    /// <summary>
    /// メインステート
    /// </summary>
    [Serializable]
    private class MainState : MyState
    {
        /// <summary>
        /// FVアタックカットインプレハブ
        /// </summary>
        [SerializeField]
        private UIFvAttackCutIn fvAttackCutinPrefab = null;
        /// <summary>
        /// FVAカットイン生成先
        /// </summary>
        [SerializeField]
        private RectTransform fvaCutinArea = null;

        /// <summary>
        /// 全WAVEが終了したかどうか
        /// </summary>
        private bool isAllWaveFinished = false;

        /// <summary>
        /// Update
        /// </summary>
        public void Update(float deltaTime)
        {
            //アイテムアイコン更新
            this.scene.battleItemIconManager.Run(deltaTime);

            //砲台更新
            this.scene.turret.Run(deltaTime);

            //WAVE更新
            this.scene.waveDataController.Update(deltaTime);

            //全WAVE終了後、魚が一匹もいなくなったら終了
            if (this.isAllWaveFinished && BattleGlobal.instance.fishList.Count == 0)
            {
                this.manager.ChangeState<BattleEndState>();
            }
        }

        /// <summary>
        /// 被ダメージ時
        /// </summary>
        public override void OnDamaged(Fish fish)
        {
            var userData = BattleGlobal.instance.userData as SingleBattleUserData;
            if (userData.hp <= 0) return;
            if (fish.isDead)     return;

            //基礎ダメージ
            int baseDamage = 0;
            if (fish.statusMaster != null)
            {
                baseDamage += (int)(fish.statusMaster as Master.SingleStageFishData).power;
            }
            //ダメージ
            int damage = baseDamage;
            //スキルよるダメージ減少
            damage -= (int)userData.skill.DamageCut(baseDamage);
            //ダメージはゼロ未満にはならない
            damage = Mathf.Max(damage, 0);
            //HP減少
            userData.hp = Mathf.Max(0, userData.hp - damage);

            //HPゲージ更新
            this.scene.OnHpGaugeUpdate();

            //HPがゼロになったらバトル終了
            if (userData.hp <= 0)
            {
                this.manager.ChangeState<BattleEndState>();
            }
        }

        /// <summary>
        /// アイテムアイコン押下時
        /// </summary>
        public override void OnClickItemIcon(UIBattleItemIcon icon)
        {
            if (icon.userItemData == null)
            {
                return;
            }

            //アイテム所持数減少
            icon.userItemData.stockCount--;

            //アイテム使用
            icon.OnUse();
        }

        /// <summary>
        /// FVアタックボタン押下時
        /// </summary>
        public override void OnClickFvAttackButton()
        {
            SoundManager.Instance.PlaySe(SeName.FVATTACK_START);

            //FVポイント消費
            var userData = BattleGlobal.instance.userData;
            userData.fvPoint -= userData.currentBetData.needFvPoint;

            //FVアタックボタンのPUSHマーク表示を消して、押せなくする
            this.scene.uiFvAttackGauge.SetGaugeValue((float)userData.fvPoint / userData.currentBetData.needFvPoint);
            this.scene.uiFvAttackGauge.RefleshButtonInteractable();

            //カットイン再生
            var cutin = Instantiate(this.fvAttackCutinPrefab, this.fvaCutinArea, false);
            cutin.onFinished = () =>
            {
                Destroy(cutin.gameObject);
                this.scene.turret.FvAttackFiring();
            };
        }

        /// <summary>
        /// 全WAVE終了時
        /// </summary>
        public override void OnAllWaveFinished()
        {
            this.isAllWaveFinished = true;
        }

        /// <summary>
        /// 一時停止ボタンクリック時
        /// </summary>
        public override void OnClickPauseButton()
        {
            //Noneステートへ一時退避
            this.manager.PushState<NoneState>(null);

            //一時停止
            TimePauseManager.Pause();

            //一時停止メニューポップアップを開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            var content = dialog.SetAsYesNoMessageDialog(Masters.LocalizeTextDB.Get("ConfirmRetire"));
            content.yesNo.yes.onClick = () =>
            {
                dialog.onClose = () =>
                {
                    //一時停止解除
                    TimePauseManager.Play();
                    //リタイア通信
                    SinglePlayApi.CallClearApi(
                        SinglePlayApi.ClearResult.Retire,
                        Rank.None,
                        (response) => this.scene.ChangeSceneToSingleStageSelect()
                    );
                };

                dialog.Close();
            };
            content.yesNo.no.onClick = () =>
            {
                dialog.onClose = () =>
                {
                    //一時停止解除
                    TimePauseManager.Play();
                    //元のステートへ戻る
                    this.manager.PopState();
                };

                dialog.Close();
            };
        }

        /// <summary>
        /// 魚生成時
        /// </summary>
        public override void OnCreateFish(Fish fish)
        {
            //ボスだったらボス出現演出再生
            if (fish.master.isBoss > 0)
            {
                this.manager.PushState<BossAppearState>(null);
            }
        }

#if DEBUG
        /// <summary>
        /// GUIボタンスタイル
        /// </summary>
        private GUIStyle buttonStyle = null;

        /// <summary>
        /// OnGUI
        /// </summary>
        public override void OnGUI()
        {
            if (this.buttonStyle == null)
            {
                this.buttonStyle = UIUtility.CreateButtonStyle(300f, 30);
            }

            if (GUILayout.Button("バトル終了", this.buttonStyle))
            {
                this.manager.ChangeState<BattleEndState>();
            }
        }
#endif
    }

    /// <summary>
    /// ボス出現演出ステート
    /// </summary>
    [Serializable]
    private class BossAppearState : MyState
    {
        /// <summary>
        /// ボス出現アニメプレハブ
        /// </summary>
        [SerializeField]
        private AnimationEventReceiver bossAppearAnimPrefab = null;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.scene.StartCoroutine(new WaitForEndOfFrame().AddCallback(() =>
            {
                //一時停止
                TimePauseManager.Pause();

                //ボス出現アニメ再生
                var anim = Instantiate(this.bossAppearAnimPrefab, this.scene.animationEffectArea, false);
                anim.onFinished = (tag) =>
                {
                    Destroy(anim.gameObject);

                    //一時停止解除
                    TimePauseManager.Play();

                    //元のステートへ戻る
                    this.manager.PopState();
                };
            }));
        }
    }

    /// <summary>
    /// バトル終了ステート
    /// </summary>
    [Serializable]
    private class BattleEndState : MyState
    {
        /// <summary>
        /// バトル終了アニメプレハブ（防衛成功時）
        /// </summary>
        [SerializeField]
        private AnimationEventReceiver clearAnimPrefab = null;
        /// <summary>
        /// バトル終了アニメプレハブ（防衛失敗時）
        /// </summary>
        [SerializeField]
        private AnimationEventReceiver faildAnimPrefab = null;
        /// <summary>
        /// リザルトポップアップ内容プレハブ
        /// </summary>
        [SerializeField]
        private SingleBattleResultPopupContent resultPopupContentPrefab = null;

        /// <summary>
        /// バトル終了アニメ
        /// </summary>
        private AnimationEventReceiver endAnim = null;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            foreach (Transform child in BattleGlobal.instance.bulletArea)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in BattleGlobal.instance.fvAttackArea)
            {
                Destroy(child.gameObject);
            }

            this.scene.turret.EndLaserBeamAnimation();

            var userData = BattleGlobal.instance.userData as SingleBattleUserData;
            float hpRemain = (float)userData.hp / userData.maxHp;
            var clearRank = this.GetClearRank(hpRemain);
            var clearResult = hpRemain > 0f ? SinglePlayApi.ClearResult.Cleared : SinglePlayApi.ClearResult.Failed;

            //防衛成功
            if (clearResult == SinglePlayApi.ClearResult.Cleared)
            {
                this.endAnim = Instantiate(this.clearAnimPrefab, this.scene.animationEffectArea, false);
            }
            //防衛失敗
            else
            {
                this.endAnim = Instantiate(this.faildAnimPrefab, this.scene.animationEffectArea, false);
            }

            //アニメ中画面触れないようにしておく
            SharedUI.Instance.DisableTouch();

            //アニメ終わったら
            this.endAnim.onFinished = (tag) =>
            {
                SharedUI.Instance.EnableTouch();

                //バトル終了通信
                SinglePlayApi.CallClearApi(clearResult, clearRank, (response) =>
                {
                    if (clearResult == SinglePlayApi.ClearResult.Cleared)
                    {
                        //リザルト後、ステージセレクトへ戻る
                        SingleBattleResultPopupContent.Open(
                            this.resultPopupContentPrefab,
                            this.scene.stageData,
                            response,
                            clearRank,
                            this.scene.ChangeSceneToSingleStageSelect
                        );
                    }
                    else
                    {
                        //すぐステージセレクトへ戻る
                        this.scene.ChangeSceneToSingleStageSelect();
                    }
                });
            };
        }

        /// <summary>
        /// 残りHPからクリアランクを計算
        /// </summary>
        private Rank GetClearRank(float hpRemain)
        {
            return hpRemain >= 1.0f ? Rank.S
                 : hpRemain >= 0.5f ? Rank.A
                 :                    Rank.B;
        }
    }

#if DEBUG
    /// <summary>
    /// テキストとかのデバッグ表示
    /// </summary>
    public override void DrawGUI()
    {
        base.DrawGUI();

        (this.stateManager.currentState as MyState)?.OnGUI();
    }

    /// <summary>
    /// デバッグ：FVMAX
    /// </summary>
    protected override void OnDebugFvMax()
    {
        BattleGlobal.instance.userData.fvPoint = this.stageData.needFvPoint;
        
        //FVアタックゲージをセット
        this.uiFvAttackGauge.RefleshView();
    }
#endif

}//class SingleBattleScene

}//namespace Battle
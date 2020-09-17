using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

namespace Battle {

/// <summary>
/// マルチバトルシーン
/// </summary>
public class MultiBattleScene : BattleSceneBase
{
    /// <summary>
    /// シーンデータパック
    /// </summary>
    public class SceneDataPack : SceneDataPackBase
    {
        /// <summary>
        /// ロビー情報
        /// </summary>
        public LobbyInfo lobyInfo = null;
        /// <summary>
        /// 龍玉、龍魂情報
        /// </summary>
        public MultiPlayApi.TMultiSoulBall soulBall = null;
    }

    /// <summary>
    /// ネットワークイベント監視
    /// </summary>
    [SerializeField]
    private MultiNetworkEventObserver networkEventObserver = null;
    /// <summary>
    /// 背景イメージ
    /// </summary>
    [SerializeField]
    private Image bgImage = null;
    /// <summary>
    /// メインステート
    /// </summary>
    [SerializeField]
    private MainState mainState = null;
    /// <summary>
    /// バトルアイテムアイコン管理
    /// </summary>
    [SerializeField]
    private UIBattleItemIconManager battleItemIconManager = null;
    /// <summary>
    /// クジラパネルUI
    /// </summary>
    [SerializeField]
    private UIWhalePanel uiWhalePanel = null;

    /// <summary>
    /// ゲームプレイ中か
    /// </summary>
    private bool isPlaying = false;
    /// <summary>
    /// ログデータ
    /// </summary>
    public MultiPlayApi.LogData logData = new MultiPlayApi.LogData();
    /// <summary>
    /// 現在のコイン枚数
    /// </summary>
    public long coin => (long)UserData.Get().coin + this.logData.addCoin - this.logData.consumeCoin;
    /// <summary>
    /// ステート管理
    /// </summary>
    private StateManager stateManager = new StateManager();
    /// <summary>
    /// ロビー情報
    /// </summary>
    private LobbyInfo lobyInfo = null;
    /// <summary>
    /// 龍玉、龍魂情報
    /// </summary>
    private MultiPlayApi.TMultiSoulBall soulBall = null;
    /// <summary>
    /// ワールド情報
    /// </summary>
    private Master.MultiWorldData worldData = null;
    /// <summary>
    /// ステージ魚ステータスデータ辞書
    /// </summary>
    private Dictionary<uint, Master.MultiStageFishData> stageFishDataList = new Dictionary<uint, Master.MultiStageFishData>();
    /// <summary>
    /// 捕獲確率計算
    /// </summary>
    private Dictionary<uint, Master.FishCaptureData[]> captureDataList = new Dictionary<uint, Master.FishCaptureData[]>();
    /// <summary>
    /// バトルWAVE制御
    /// </summary>
    private MultiFishWaveGroupDataController waveDataController = null;
    /// <summary>
    /// 召喚制御
    /// </summary>
    private SummonFishRouteDataController summonController = null;
    /// <summary>
    /// BGMトラック
    /// </summary>
    private BgmTrack bgmTrack = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = new AssetListLoader();

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
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
        this.stateManager.AddState(new NoneState{ scene = this });
        this.stateManager.AddState(new ReconnectState{ scene = this });
        this.stateManager.AddState(new JoinRoomState{ scene = this });
        this.stateManager.AddState(this.mainState);
        this.stateManager.AddState(new LeaveRoomState{ scene = this });
    }

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public override void OnSceneLoaded(SceneDataPackBase dataPack)
    {
        if (dataPack is SceneDataPack)
        {
            this.lobyInfo = (dataPack as SceneDataPack).lobyInfo;
            this.soulBall = (dataPack as SceneDataPack).soulBall;
            this.worldData = Masters.MultiWorldDB.FindById(this.lobyInfo.worldId);
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
#if UNITY_EDITOR && DEBUG
        if (this.lobyInfo == null)
        {
            this.worldData = Masters.MultiWorldDB.GetList()[0];
            this.lobyInfo = new LobbyInfo();
            this.lobyInfo.worldId = this.worldData.id;
            this.lobyInfo.typedLobbyInfo = new TypedLobbyInfo
            {
                Name = this.worldData.id.ToString(),
                Type = LobbyType.Default,
            };
        }
#endif
        //魚用カメラの描画範囲設定
        BattleGlobal.instance.SetFishCameraViewport();

        //BET情報設定
        var betData = Masters.BetDB.GetList().Find(x => x.maxBet >= this.worldData.minBet);
        if (betData == null)
        {
            Debug.LogErrorFormat("minBet:{0}を満たすBetDataが存在しない", this.worldData.minBet);
            betData = Masters.BetDB.GetList()[0];
        }

        //バトル用ユーザーデータ作成
        BattleGlobal.instance.userData = new MultiBattleUserData(
            UserData.Get().GetSelectedTurretData(),
            UserData.Get().fvPoint,
            betData,
            this.soulBall
        );

        //砲台情報プロパティセット
        var turretData = UserData.Get().GetSelectedTurretData();
        var turretDto = new TurretDto {
            batteryId = turretData.batteryMasterId,
            barrelId = turretData.barrelMasterId,
            bulletId = turretData.bulletMasterId,
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
            { PlayerPropertyKey.Coin, this.coin },
            { PlayerPropertyKey.Bet, (int)betData.maxBet },
            { PlayerPropertyKey.Turret, turretDto.GetBinary() }
        });

        //必要リソースの読み込み
        this.Load();
    }

    /// <summary>
    /// 必要リソースの読み込み
    /// </summary>
    private void Load()
    {
        var tmpLoader = new AssetListLoader();

        //WAVEデータ読み込み
        var waveLoader = new AssetLoader<MultiFishWaveGroupData>(SharkDefine.GetMultiFishWaveGroupDataPath(this.worldData.key));
        tmpLoader.Add(waveLoader);

        //召喚データ読み込み
        var summonLoaders = SummonFishRouteDataController.GetAssetLoaders().ToArray();
        tmpLoader.AddRange(summonLoaders);

        //1段階目ロード開始
        tmpLoader.Load(() =>
        {
            //WAVE準備
            this.waveDataController = new MultiFishWaveGroupDataController(waveLoader.handle.asset as MultiFishWaveGroupData);
            
            //召喚準備
            this.summonController = new SummonFishRouteDataController(summonLoaders.Select(x => x.handle.asset as RandomFishRouteData).ToArray());

            //背景リソース読み込み
            this.loader.Add<Sprite>(SharkDefine.GetBattleBgSpritePath(this.worldData.key));

            //WAVE必要リソース読み込み
            this.loader.AddRange(this.waveDataController.loader);

            //召喚必要リソース読み込み
            this.loader.AddRange(this.summonController.loader);

            //バトルアイテムアイコンリソース読み込み
            this.battleItemIconManager.Set(
                userItemDatas: UserData.Get().itemData
                    .Where(x => x.itemType == ItemType.BattleItem && x.itemId > 0)
                    .OrderBy(x => x.itemId)
                    .ToArray(),
                onClick: this.OnClickItemIcon
            );
            this.battleItemIconManager.LoadIfNeed();

            //WhaleDive演出用リソース読み込み
            this.uiWhalePanel.Load();

            //BGM読み込み
            this.loader.Add<BgmClipData>(SharkDefine.GetBgmClipPath(this.worldData.bgmName));
            this.loader.Add<BgmClipData>(SharkDefine.GetBgmClipPath(BgmName.WHALEDIVE));

            //SE読み込み
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.FVATTACK_OK));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.FVATTACK_START));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.CAPTURE_MULTI));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.WIN_0));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.WIN_1));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.BUTTON_BET));

            //ロード
            this.loader.Load();

            //ロード完了待ち
            StartCoroutine(new WaitWhile(AssetManager.IsLoading).AddCallback(() =>
            {
                //ローダー合成
                this.loader.AddRange(tmpLoader);

                //背景
                this.bgImage.sprite = this.loader[SharkDefine.GetBattleBgSpritePath(this.worldData.key)].handle.asset as Sprite;

                //バトルアイテムアイコンセットアップ
                this.battleItemIconManager.Setup();

                //ルーム参加ステートへ
                this.stateManager.ChangeState<JoinRoomState>();
            }));
        });
    }

    /// <summary>
    /// 不要になったリソースの破棄
    /// </summary>
    private void Unload()
    {
        this.loader.Unload();
        this.mainState.Unload();
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        (this.stateManager.currentState as MainState)?.Update();
    }

    /// <summary>
    /// メニューボタンクリック時
    /// </summary>
    public void OnClickMenuButton()
    {
        (this.stateManager.currentState as MyState)?.OnClickMenuButton();
    }

    /// <summary>
    /// 通常弾生成しても良いかどうか
    /// </summary>
    public bool CanCreateNormalBullet(int count)
    {
        return (this.stateManager.currentState is MainState)
            && (this.stateManager.currentState as MainState).CanCreateNormalBullet(count);
    }

    /// <summary>
    /// コイン数表示更新
    /// </summary>
    public void RefleshCoinCount()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
            { PlayerPropertyKey.Coin, this.coin }
        });
    }

    /// <summary>
    /// BET増加ボタン押下時
    /// </summary>
    public void OnClickBetPlusButton()
    {
        (this.stateManager.currentState as MainState)?.OnClickBetPlusButton();
    }

    /// <summary>
    /// BET減少ボタン押下時
    /// </summary>
    public void OnClickBetMinusButton()
    {
        (this.stateManager.currentState as MainState)?.OnClickBetMinusButton();
    }

    /// <summary>
    /// アイテムアイコンクリック時
    /// </summary>
    private void OnClickItemIcon(UIBattleItemIcon icon)
    {
        (this.stateManager.currentState as MainState)?.OnClickItemIcon(icon);
    }

    /// <summary>
    /// コイン消費時
    /// </summary>
    public void OnUseCoin(uint bet)
    {
        (this.stateManager.currentState as MainState)?.OnUseCoin(bet);
    }

    /// <summary>
    /// 着弾エフェクト生成時
    /// </summary>
    public void OnCreateLandingEffect(LandingEffect landingEffect)
    {
        (this.stateManager.currentState as MainState)?.OnCreateLandingEffect(landingEffect);
    }

    /// <summary>
    /// 魚生成時
    /// </summary>
    public override void OnCreateFish(Fish fish)
    {
        //ステータスマスター
        Master.MultiStageFishData statusMaster = null;

        //辞書からステータスマスターを引っ張る。無かったら魚IDで辞書化。
        if (!this.stageFishDataList.TryGetValue(fish.master.id, out statusMaster))
        {
            statusMaster = Masters.MultiStageFishDB.GetList().Find(x => x.worldId == this.worldData.id && x.fishId == fish.master.id);
            this.stageFishDataList.Add(fish.master.id, statusMaster);
        }

        if (statusMaster != null)
        {
            //ステータスマスターセット
            fish.SetStatusMaster(statusMaster);
        }
        else
        {
            Debug.LogWarningFormat("MultiStageFishData: worldId={0}, fishId={1} のデータが存在しません", this.worldData.id, fish.master.id);
        }

        (this.stateManager.currentState as MainState)?.OnCreateFish(fish);
    }

    /// <summary>
    /// 魚被ダメ時
    /// </summary>
    public void OnFishDamaged(Fish fish, int damage)
    {
        (this.stateManager.currentState as MainState)?.OnFishDamaged(fish, damage);
    }

    /// <summary>
    /// 魚の状態変化時
    /// </summary>
    public void OnChangeFishCondition(Fish fish)
    {
        (this.stateManager.currentState as MainState)?.OnChangeFishCondition(fish);
    }

    /// <summary>
    /// 魚捕獲確率計算
    /// </summary>
    public override bool FishCatchingCalculation(Fish fish)
    {
        var statusMaster = fish.statusMaster as Master.MultiStageFishData;

        if (statusMaster == null)
        {
            return true;
        }

        //HP割合
        float hpRate = (float)fish.hp / statusMaster.hp;

        //HP0%
        if (hpRate <= 0f)
        {
            //絶対に成功
            return true;
        }

        //以下、マスターの捕獲確率に従う

        //確率データ
        Master.FishCaptureData[] captureDatas = null;

        //辞書から確率データを引っ張る。無かったらHP割合昇順で辞書化。
        if (!this.captureDataList.TryGetValue(statusMaster.captureId, out captureDatas))
        {
            captureDatas = Masters.FishCaptureDB
                .GetList()
                .Where(x => x.groupId == statusMaster.captureId)
                .OrderBy(x => x.hpRate)
                .ToArray();

            this.captureDataList.Add(statusMaster.captureId, captureDatas);
#if DEBUG
            if (captureDatas.Length == 0)
            {
                Debug.LogWarningFormat("fishId={0}, captureId={1}のデータは存在しない。", fish.master.id, statusMaster.captureId);
            }
#endif
        }

        //HP割合昇順で判定
        foreach (var captureData in captureDatas)
        {
            if (hpRate <= captureData.hpRate * Masters.PercentToDecimal)
            {
                return UnityEngine.Random.value <= captureData.probability * Masters.PercentToDecimal;
            }
        }

        return false;
    }

    /// <summary>
    /// 魚捕獲時
    /// </summary>
    public override void OnFishCatched(Fish fish, Bullet bullet)
    {
        (this.stateManager.currentState as MainState)?.OnFishCatched(fish, bullet);
    }

    /// <summary>
    /// 自動照準時のターゲット選定処理
    /// </summary>
    public override Fish FindTargetFish()
    {
        return (this.stateManager.currentState is MainState) ? (this.stateManager.currentState as MainState).FindTargetFish() : null;
    }

    /// <summary>
    /// 魚召喚
    /// </summary>
    public void OnSummon(string key)
    {
        (this.stateManager.currentState as MainState)?.OnSummon(key);
    }

    /// <summary>
    /// FVアタックボタン押下時
    /// </summary>
    public void OnClickFvAttackButton()
    {
        (this.stateManager.currentState as MainState)?.OnClickFvAttackButton();
    }

    /// <summary>
    /// 砲台制御機能変更時
    /// </summary>
    public override void OnSetTurretController(ITurretController turretController)
    {
        (this.stateManager.currentState as MainState)?.OnSetTurretController(turretController);
    }

    /// <summary>
    /// フィーバータイム開始時
    /// </summary>
    public void OnStartFeverTime()
    {
        (this.stateManager.currentState as MainState)?.OnStartFeverTime();
    }

    /// <summary>
    /// フィーバータイム終了時
    /// </summary>
    public void OnFinishedFeverTime()
    {
        (this.stateManager.currentState as MainState)?.OnFinishedFeverTime();
    }

    /// <summary>
    /// スロット演出開始時
    /// </summary>
    public void OnStartSlot()
    {
        (this.stateManager.currentState as MainState)?.OnStartSlot();
    }

    /// <summary>
    /// スロット演出終了時
    /// </summary>
    public void OnFinishedSlot()
    {
        (this.stateManager.currentState as MainState)?.OnFinishedSlot();
    }

#if DEBUG
    /// <summary>
    /// GUIボタンスタイル
    /// </summary>
    private GUIStyle getButtonStyle = null;

    /// <summary>
    /// GUI描画
    /// </summary>
    public override void DrawGUI()
    {
        if (this.getButtonStyle == null)
        {
            this.getButtonStyle = UIUtility.CreateButtonStyle(500f, 30);
        }

        base.DrawGUI();

        if (GUILayout.Button("BallGet確定:" + BattleDebug.isConfirmGetBall, this.getButtonStyle))
        {
            BattleDebug.isConfirmGetBall = !BattleDebug.isConfirmGetBall;
        }

        if (GUILayout.Button("SoulGet確定:" + BattleDebug.isConfirmGetSoul, this.getButtonStyle))
        {
            BattleDebug.isConfirmGetSoul = !BattleDebug.isConfirmGetSoul;
        }
    }

    /// <summary>
    /// デバッグ：FVMAX
    /// </summary>
    protected override void OnDebugFvMax()
    {
        (this.stateManager.currentState as MainState)?.OnDebugFvMax();
    }
#endif

    /// <summary>
    /// ステート基礎
    /// </summary>
    private abstract class MyState : MultiNetworkEventReceiverState
    {
        /// <summary>
        /// シーン
        /// </summary>
        [SerializeField]
        public MultiBattleScene scene = null;
        /// <summary>
        /// マルチネットワークイベント監視者
        /// </summary>
        public override MultiNetworkEventObserver observer => this.scene.networkEventObserver;

        /// <summary>
        /// メニューボタン押下時
        /// </summary>
        public virtual void OnClickMenuButton(){}

        /// <summary>
        /// 再接続確認ダイアログを開く
        /// </summary>
        protected void ShowDisconnectedDialog()
        {
            //空ステートに退避
            this.manager.ChangeState<NoneState>();

            //エラー通知
            var dialog = SharedUI.Instance.ShowSimpleDialog(true);
            var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("MultiBattleDisconnected"));
            content.buttonGroup.buttons[0].onClick = () =>
            {
                dialog.onClose = () => this.manager.ChangeState<LeaveRoomState>();
                dialog.Close();
            };
        }
    }

    /// <summary>
    /// 空ステート
    /// </summary>
    private class NoneState : MyState
    {
    }

    /// <summary>
    /// 再接続ステート
    /// </summary>
    private class ReconnectState : MyState
    {
        /// <summary>
        /// 接続成功後ステート名
        /// </summary>
        public Type nextStateType = null;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        /// <summary>
        /// 接続失敗
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            this.ShowDisconnectedDialog();
        }

        /// <summary>
        /// 接続成功
        /// </summary>
        public override void OnConnectedToMaster()
        {
            this.manager.ChangeState(this.nextStateType);
        }
    }

    /// <summary>
    /// ルーム参加ステート
    /// </summary>
    private class JoinRoomState : MyState
    {
        /// <summary>
        /// ルーム作成へジャンプするかどうか
        /// </summary>
        public bool isGoToCreateRoom = false;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (this.isGoToCreateRoom)
            {
                //ランダムルーム参加をスキップして即ルーム作成する
                this.OnJoinRandomFailed(0, null);
                return;
            }

            //ランダムなルームに参加
            PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, this.scene.lobyInfo.typedLobbyInfo, null);
        }

        /// <summary>
        /// 接続切断時
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            this.ShowDisconnectedDialog();
        }

        /// <summary>
        /// ランダムルームへの参加失敗時
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            //ルーム作成
            PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = 4 }, this.scene.lobyInfo.typedLobbyInfo);
        }

        /// <summary>
        /// ルーム作成失敗時
        /// </summary>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            this.OnDisconnected(DisconnectCause.None);
        }

        /// <summary>
        /// ルーム参加成功時
        /// </summary>
        public override void OnJoinedRoom()
        {
            this.isGoToCreateRoom = false;
            this.manager.ChangeState<MainState>();
        }
    }

    /// <summary>
    /// メインステート
    /// </summary>
    [Serializable]
    private class MainState : MyState
    {
        /// <summary>
        /// フィーバータイムステータス
        /// </summary>
        private enum FeverTimeStatus
        {
            None,   //フィーバータイム外
            Enabled,//フィーバータイム中
            End     //フィーバータイム終了待ち
        }

        /// <summary>
        /// 最大deltaTime(1フレームで回せる秒数)：3秒(=3,000)
        /// </summary>
        private readonly int MAX_DELTATIME_MSEC = 3000;
        /// <summary>
        /// ミリ秒表現deltaTime：0.0166秒(=16)
        /// </summary>
        private readonly int DELTATIME_MSEC = (int)(SharkDefine.DELTATIME * 1000);
        /// <summary>
        /// 自動タイムアウト判定ミリ秒：120秒(=120,000)
        /// </summary>
        private readonly int TIMEOUT_MSEC = 120 * 1000;

        /// <summary>
        /// 砲台
        /// </summary>
        [SerializeField]
        private MultiBattleTurret[] turrets = null;
        /// <summary>
        /// コインエフェクト小プレハブ
        /// </summary>
        [SerializeField]
        private CoinEffect coinEffectSmallPrefab = null;
        /// <summary>
        /// コインエフェクト中プレハブ
        /// </summary>
        [SerializeField]
        private CoinEffect coinEffectMiddlePrefab = null;
        /// <summary>
        /// コインエフェクト大プレハブ
        /// </summary>
        [SerializeField]
        private CoinEffect coinEffectBigPrefab = null;
        /// <summary>
        /// コインエフェクト生成先
        /// </summary>
        [SerializeField]
        private RectTransform coinEffectArea = null;
        /// <summary>
        /// コイン数値用フォント銀
        /// </summary>
        [SerializeField]
        private Font coinFontSilver = null;
        /// <summary>
        /// レベルアップ演出プレハブ
        /// </summary>
        [SerializeField]
        private UILevelUp levelUpPrefab = null;
        /// <summary>
        /// レベルアップ演出生成先
        /// </summary>
        [SerializeField]
        private RectTransform levelUpLayer = null;
        /// <summary>
        /// FVアタックカットインプレハブ
        /// </summary>
        [SerializeField]
        private UIFvAttackCutIn fvAttackCutInPrefab = null;
        /// <summary>
        /// FVAカットイン生成先
        /// </summary>
        [SerializeField]
        private RectTransform fvaCutinArea = null;
        /// <summary>
        /// 召喚エフェクトプレハブ
        /// </summary>
        [SerializeField]
        private ParticleSystem summonEffectPrefab = null;
        /// <summary>
        /// スライドメニューダイアログプレハブ
        /// </summary>
        [SerializeField]
        private ShortCutDialog shortCutDialogPrefab = null;
        /// <summary>
        /// バックグラウンドからの復帰中タッチブロック
        /// </summary>
        [SerializeField]
        private Image resumeBackgroundCover = null;

        /// <summary>
        /// API通信中フラグ
        /// </summary>
        private bool isApiConnecting = false;
        /// <summary>
        /// メニューを開いているかどうか
        /// </summary>
        private bool isOpenMenu = false;
        /// <summary>
        /// スロット演出中かどうか
        /// </summary>
        private bool isSlot = false;
        /// <summary>
        /// タイムスタンプ
        /// </summary>
        private int timeStamp = 0;
        /// <summary>
        /// 位置ID
        /// </summary>
        private int? myPositionId = null;
        /// <summary>
        /// 自砲台
        /// </summary>
        private MultiBattleTurret myTurret = null;
        /// <summary>
        /// 現在レベル
        /// </summary>
        private uint currentLv = 1;
        /// <summary>
        /// 現在龍玉所持数
        /// </summary>
        private uint currentBallNum = 0;
        /// <summary>
        /// 現在龍魂所持数
        /// </summary>
        private uint currentSoulNum = 0;
        /// <summary>
        /// フィーバータイムステータス
        /// </summary>
        private FeverTimeStatus feverTimeStatus = FeverTimeStatus.None;
        /// <summary>
        /// バトルユーザーデータ
        /// </summary>
        private MultiBattleUserData btlUserData = null;
        /// <summary>
        /// 龍玉ドロップ位置
        /// </summary>
        private List<(uint fishId, Vector3 dropPosition)> ballDropList = new List<(uint fishId, Vector3 dropPosition)>();
        /// <summary>
        /// 龍魂ドロップ位置
        /// </summary>
        private List<(uint fishId, Vector3 dropPosition)> soulDropList = new List<(uint fishId, Vector3 dropPosition)>();
        /// <summary>
        /// イベント処理
        /// </summary>
        private Action<EventData>[] onEventActions = new Action<EventData>[(int)MultiEventCode.Length];
        /// <summary>
        /// ゲーム開始通信成功時コールバック
        /// </summary>
        private event Action onSuccessStartApi = null;
        /// <summary>
        /// バックグラウンドからの復帰完了時コールバック
        /// </summary>
        private event Action onResumeFromBackground = null;
        /// <summary>
        /// 保留にした砲台ロード処理
        /// </summary>
        private event Action pendingLoadTurret = null;
        /// <summary>
        /// 保留にされた魚へのデータ
        /// </summary>
        private List<IFishDto> pendingFishDtos = new List<IFishDto>();
        /// <summary>
        /// 保留にされた召喚魚データ
        /// </summary>
        private List<SummonDto> pendingSummonDtos = new List<SummonDto>();
        /// <summary>
        /// 初期化完了通知待ちコルーチン
        /// </summary>
        private Coroutine waitInitEventCoroutine = null;
        /// <summary>
        /// 定期ログ送信コルーチン
        /// </summary>
        private Coroutine sendLogCoroutine = null;
        /// <summary>
        /// 現在のユーザーレベルでの最大BETデータ
        /// </summary>
        private Master.BetData maxBetData = null;
        /// <summary>
        /// 最大レベルデータ
        /// </summary>
        private Master.LevelData maxLevelData = null;
        /// <summary>
        /// 最大レベルに達するのに必要な経験値
        /// </summary>
        private uint maxExp = 0;

        /// <summary>
        /// 不要リソースの破棄
        /// </summary>
        public void Unload()
        {
            for (int i = 0; i < this.turrets.Length; i++)
            {
                this.turrets[i].loader.Unload();
                this.turrets[i].loader.Clear();
            }
        }

        /// <summary>
        /// End
        /// </summary>
        public override void End()
        {
            if (this.waitInitEventCoroutine != null)
            {
                this.scene.StopCoroutine(this.waitInitEventCoroutine);
                this.waitInitEventCoroutine = null;
            }

            if (this.sendLogCoroutine != null)
            {
                this.scene.StopCoroutine(this.sendLogCoroutine);
                this.sendLogCoroutine = null;
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
            this.btlUserData = BattleGlobal.instance.userData as MultiBattleUserData;
            this.maxBetData = this.GetMaxBetDataByCurrentLevel();
            this.maxLevelData = Masters.LevelDB.GetList().Last();
            this.maxExp = (uint)Masters.LevelDB.GetList().Sum(x => x.exp) - this.maxLevelData.exp;
            this.currentLv = UserData.Get().lv;
            this.currentBallNum = this.btlUserData.ballNum;
            this.currentSoulNum = this.btlUserData.soulNum;

            this.onEventActions[(int)MultiEventCode.Init] = this.OnEvent_Init;
            this.onEventActions[(int)MultiEventCode.CreateLandingEffect] = this.OnEvent_CreateLandingEffect;
            this.onEventActions[(int)MultiEventCode.CreateCoinEffect] = this.OnEvent_CreateCoinEffect;
            this.onEventActions[(int)MultiEventCode.FishDamaged] = this.OnEvent_FishDamaged;
            this.onEventActions[(int)MultiEventCode.ChangeFishCondition] = this.OnEvent_ChangeFishCondition;
            this.onEventActions[(int)MultiEventCode.Summon] = this.OnEvent_Summon;
            this.onEventActions[(int)MultiEventCode.ShootLaserBeam] = this.OnEvent_ShootLaserBeam;
            this.onEventActions[(int)MultiEventCode.ShootBomb] = this.OnEvent_ShootBomb;
            this.onEventActions[(int)MultiEventCode.ShootAllRange] = this.OnEvent_ShootAllRange;
            this.onEventActions[(int)MultiEventCode.CreateFVAPenetrationChargeEffect] = this.OnEvent_CreateFVAPenetraionChargeEffect;

            //自分がホストなら
            if (PhotonNetwork.IsMasterClient)
            {
                //座席決定
                this.SetPositionIds(-1, PhotonNetwork.LocalPlayer.ActorNumber);

                //ゲーム開始通信成功時コールバックセット
                this.onSuccessStartApi += () =>
                {
                    //ゲーム開始時間セット
                    this.timeStamp = BattleGlobal.GetTimeStamp();
                };
            }
            else
            {
                //5秒待ってもホストから席番号の通知が無かったら、退出して自分でルームを作る
                this.waitInitEventCoroutine = this.scene.StartCoroutine(new WaitForSeconds(5f).AddCallback(() =>
                {
                    //退出
                    PhotonNetwork.LeaveRoom();
                    //ルーム作成フラグを立てる
                    this.manager.GetState<JoinRoomState>().isGoToCreateRoom = true;
                    //再接続後、ルーム参加ステートへ
                    this.manager.GetState<ReconnectState>().nextStateType = typeof(JoinRoomState);
                    this.manager.ChangeState<ReconnectState>();
                }));
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            if (!this.scene.isPlaying)
            {
                return;
            }

            //バックグラウンドからの復帰が完了したかどうか
            bool isResumeFromBackgroung = false;

            int deltaTimeMSec = unchecked(BattleGlobal.GetTimeStamp() - this.timeStamp);

            //想定外にタイムアウトを超えていた場合
            if (deltaTimeMSec > this.TIMEOUT_MSEC)
            {
                //HOMEに戻す
                this.OnDisconnected(DisconnectCause.None);
                return;
            }
            //バックグラウンドに行っていてdeltaTimeが膨大になっていた場合
            else if (deltaTimeMSec > this.MAX_DELTATIME_MSEC)
            {
                if (!this.resumeBackgroundCover.gameObject.activeSelf)
                {
                    //早回しするためタッチブロック
                    this.resumeBackgroundCover.gameObject.SetActive(true);
                }

                //早回しは1フレームで最大3秒まで
                deltaTimeMSec = this.MAX_DELTATIME_MSEC;
            }
            //バックグラウンドからの復帰完了した
            else if (this.resumeBackgroundCover.gameObject.activeSelf)
            {
                this.resumeBackgroundCover.gameObject.SetActive(false);
                isResumeFromBackgroung = true;
            }

            while (deltaTimeMSec > 0)
            {
                //1回で経過させる時間は最大でも16ミリ秒
                int _deltaTimeMSec = Mathf.Min(deltaTimeMSec, this.DELTATIME_MSEC);

                //経過させる時間を少数表現に＝0.016秒
                float deltaTime = _deltaTimeMSec * Masters.MilliSecToSecond;

                //アイテムアイコン更新
                this.scene.battleItemIconManager.Run(deltaTime);

                //砲台更新
                this.myTurret.Run(deltaTime);

                //FVA更新
                foreach (var turret in this.turrets)
                {
                    turret.UpdateFVA(deltaTime);
                }

                //WAVE更新
                this.scene.waveDataController.Update(deltaTime);

                //保留した召喚魚の生成
                this.CreateSummonFishIfCan();

                //魚更新
                BattleGlobal.instance.UpdateFish(deltaTime);

                //次のループのため経過させた時間を引く
                deltaTimeMSec -= _deltaTimeMSec;

                //タイムスタンプに経過した時間を加算
                this.timeStamp += _deltaTimeMSec;
            }

            //バックグラウンドかの復帰完了時
            if (isResumeFromBackgroung)
            {
                //保留にしていた処理の実行
                this.onResumeFromBackground?.Invoke();
                this.onResumeFromBackground = null;

                //各プレイヤーの状態更新
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    this.OnPlayerPropertiesUpdate(player, player.CustomProperties);
                }
            }

            //必要ならレベルアップ通信
            this.CallLevelUpApiIfNeed();

            //必要なら龍玉獲得通信
            this.CallGetBallApiIfNeed();

            //必要なら龍魂獲得通信
            this.CallGetSoulApiIfNeed();

            //必要ならフィーバータイム終了通信
            this.CallFeverEndApiIfNeed();
        }

        /// <summary>
        /// バックグラウンドからの復帰中かどうか
        /// </summary>
        private bool IsBackground()
        {
            return this.resumeBackgroundCover.gameObject.activeSelf || unchecked(BattleGlobal.GetTimeStamp() - this.timeStamp) > this.MAX_DELTATIME_MSEC;
        }

        /// <summary>
        /// メニューボタン押下時
        /// </summary>
        public override void OnClickMenuButton()
        {
            //ゲームプレイ中フラグがONになるまでローディング表示されてるので、そもそも押せない

            SoundManager.Instance.PlaySe(SeName.YES);

            //メニュー開く
            var dialog = SharedUI.Instance.ShowPopup(this.shortCutDialogPrefab);
            var dialogParent = dialog.transform.parent;
            this.isOpenMenu = true;

            //HOMEに戻るボタンが押されたかどうか
            bool isBackHome = false;

            //HOMEに戻るボタン押下時
            dialog.onClickBackHomeButton = () =>
            {
                isBackHome = true;
            };

            dialog.onMissionChallenge = (MissionDialog) =>
            {
                isBackHome = true;
                this.manager.GetState<LeaveRoomState>().nextScenName = MissionDialog.nextSceneName;
                this.manager.GetState<LeaveRoomState>().dataPack = MissionDialog.dataPack;
            };

            //ダイアログが閉じた時
            dialog.onClose = () =>
            {
                //コイン数が変化したかもしれないので更新
                this.scene.RefleshCoinCount();

                //バトルアイテム所持情報に変化があったかもしれないので更新
                this.RefleshBattleItem(dialogParent, () =>
                {
                    this.isOpenMenu = false;

                    if (isBackHome)
                    {
                        //HOMEに戻るボタンが押されていたらHOMEに戻る
                        this.manager.ChangeState<LeaveRoomState>();
                    }
                    else if (!PhotonNetwork.IsConnected)
                    {
                        //Photonとの接続が切れていたらHOMEに戻る
                        this.OnDisconnected(DisconnectCause.None);
                    }
                });
            };
        }

        /// <summary>
        /// バトルアイテム情報更新
        /// </summary>
        private void RefleshBattleItem(Transform parent, Action onCompleted)
        {
            //アイテム情報セット
            this.scene.battleItemIconManager.Set(
                userItemDatas: UserData.Get().itemData
                    .Where(x => x.itemType == ItemType.BattleItem)
                    .OrderBy(x => x.itemId)
                    .ToArray(),
                onClick: this.scene.OnClickItemIcon
            );

            //ロード中はタッチブロック
            SharedUI.Instance.DisableTouch();

            //必要ならリソースロード
            this.scene.battleItemIconManager.LoadIfNeed(() =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();
                //セットアップ
                this.scene.battleItemIconManager.Setup();
                //更新完了
                onCompleted?.Invoke();
            });
        }

        /// <summary>
        /// 通常弾生成しても良いかどうか
        /// </summary>
        public bool CanCreateNormalBullet(int count)
        {
            if (this.isApiConnecting)
            {
                //通信中は不可
                return false;
            }

            if (this.isOpenMenu)
            {
                //ショートカットメニュー開いてる時は不可
                return false;
            }

            long needCoin = BattleGlobal.instance.userData.currentBetData.maxBet
                          * this.scene.worldData.betCoin
                          * (uint)count;
            if (this.scene.coin < needCoin)
            {
                //コイン枚数が足りていないので不可
                return false;
            }

            return true;
        }

        /// <summary>
        /// BET増加ボタン押下時
        /// </summary>
        public void OnClickBetPlusButton()
        {
            uint currentBetId = BattleGlobal.instance.userData.currentBetData.id;
            uint nextBetId = currentBetId + 1;

            var nextBetData = Masters.BetDB.FindById(nextBetId);
            if (nextBetData == null)
            {
                Debug.LogWarningFormat("id={0}のBetDataは存在しない", nextBetId);
                return;
            }

            if (nextBetData.maxBet > this.scene.worldData.maxBet)
            {
                //このワールドではこれ以上のBETに上げられない
                return;
            }

            var nextLvData = Masters.LevelDB.GetList().Where(x => x.betId == nextBetId).OrderBy(x => x.lv).FirstOrDefault();
            if (nextLvData == null)
            {
                Debug.LogWarningFormat("betId={0}のLevelDataは存在しない", nextBetId);
                return;
            }

            if (nextLvData.lv > UserData.Get().lv)
            {
                //現在のレベルでは次のBETは未開放
                return;
            }

            SoundManager.Instance.PlaySe(SeName.BUTTON_BET);

            //BET情報変更
            BattleGlobal.instance.userData.currentBetData = nextBetData;

            //プロパティ更新
            PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
                { PlayerPropertyKey.Bet, (int)nextBetData.maxBet }
            });

            //BET変更により必要FVポイントが変化したのでFVゲージ更新
            this.myTurret.uiFvAttackGauge.RefleshView();
        }

        /// <summary>
        /// BET減少ボタン押下時
        /// </summary>
        public void OnClickBetMinusButton()
        {
            uint currentBetId = BattleGlobal.instance.userData.currentBetData.id;
            uint prevBetId = currentBetId - 1;

            var prevBetData = Masters.BetDB.FindById(prevBetId);
            if (prevBetData == null)
            {
                Debug.LogWarningFormat("id={0}のBetDataは存在しない", prevBetId);
                return;
            }

            if (prevBetData.maxBet < this.scene.worldData.minBet)
            {
                //このワールドではこれ以下のBETに下げられない
                return;
            }

            SoundManager.Instance.PlaySe(SeName.BUTTON_BET);

            //BET情報変更
            BattleGlobal.instance.userData.currentBetData = prevBetData;

            //プロパティ更新
            PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
                { PlayerPropertyKey.Bet, (int)prevBetData.maxBet }
            });

            //BET変更により必要FVポイントが変化したのでFVゲージ更新
            this.myTurret.uiFvAttackGauge.RefleshView();
        }

        /// <summary>
        /// アイテムアイコンクリック時
        /// </summary>
        public void OnClickItemIcon(UIBattleItemIcon icon)
        {
            if (icon.userItemData == null)
            {
                return;
            }

            if (this.isApiConnecting)
            {
                //通信中なのでreturn
                return;
            }

            //通信中フラグ立てる
            this.isApiConnecting = true;

            //API実行
            MultiPlayApi.CallItemUseApi(icon.userItemData, () =>
            {
                //通信完了
                this.isApiConnecting = false;

                if (PhotonNetwork.IsConnected)
                {
                    //アイテム使用
                    icon.OnUse();
                }
                else
                {
                    //Photonとの接続が切れていたらHOMEに戻る
                    this.OnDisconnected(DisconnectCause.None);
                }
            });
        }

        /// <summary>
        /// 接続切断時
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            /*
            最初の自席決定までにここに来たら？
            -> ローディング表示が消えてないので、ローディング表示したままタイトルへ戻る

            自席決定後、自砲台ロード完了待ち中にここに来たら？
            -> ロード完了後に呼ばれるゲーム開始APIが呼ばれなければ良い。Noneステートに切り替わるので呼ばれない。

            ゲーム開始API中にここに来たら？
            -> ゲーム開始API通信中は処理せず、API完了後に処理する

            初期化通知完了待ち中にここに来たら？
            -> ステート切り替えと共に初期化通知完了待ちコルーチンは止まる

            バックグラウンドからの復帰中にここに来たら？
            -> ステートが切り替わるので、バックグラウンドからの復帰は止まる
            */

            //API通信中の場合
            if (this.isApiConnecting)
            {
                //API完了後に処理する
                return;
            }

            //メニュー開いている場合
            if (this.isOpenMenu)
            {
                //メニュー閉じた後に処理する
                return;
            }

            this.ShowDisconnectedDialog();
        }

        /// <summary>
        /// 新規プレイヤー入室時
        /// </summary>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //ホストなら
            if (PhotonNetwork.IsMasterClient)
            {
                if (!this.scene.isPlaying)
                {
                    //Debug.Log("OnPlayerEnteredRoom -> まだプレイ開始してないので保留");
                    this.onSuccessStartApi += () =>
                    {
                        //Debug.Log("保留にした処理の実行 -> OnPlayerEnteredRoom");
                        this.OnPlayerEnteredRoom(newPlayer);
                    };
                    return;
                }

                if (this.IsBackground())
                {
                    //Debug.Log("OnPlayerEnteredRoom -> バックグラウンドからの復帰中なので保留");
                    this.onResumeFromBackground += () =>
                    {
                        //Debug.Log("保留にした処理の実行 -> OnPlayerEnteredRoom");
                        this.OnPlayerEnteredRoom(newPlayer);
                    };
                    return;
                }

                var dto = new WaveResumeDto{
                    timeStamp = this.timeStamp,
                    waveBytes = this.scene.waveDataController.GetBinary(),
                    summonBytes = this.scene.summonController.GetBinary(),
                };

                //初期化情報を通知
                PhotonNetwork.RaiseEvent(
                    eventCode: (byte)MultiEventCode.Init,
                    eventContent: dto.GetBinary(),
                    raiseEventOptions: new RaiseEventOptions{ TargetActors = new int[]{ newPlayer.ActorNumber } },
                    sendOptions: SendOptions.SendReliable
                );

                //座席通知
                this.SetPositionIds(-1, newPlayer.ActorNumber);
            }
        }

        /// <summary>
        /// プレイヤー退出時
        /// </summary>
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //ホストなら
            if (PhotonNetwork.IsMasterClient)
            {
                if (!this.scene.isPlaying)
                {
                    //Debug.Log("OnPlayerLeftRoom -> まだプレイ開始してないので保留");
                    this.onSuccessStartApi += () =>
                    {
                        //Debug.Log("保留にした処理の実行 -> OnPlayerLeftRoom");
                        this.OnPlayerLeftRoom(otherPlayer);
                    };
                    return;
                }

                if (this.IsBackground())
                {
                    //Debug.Log("OnPlayerLeftRoom -> バックグラウンドからの復帰中なので保留");
                    this.onResumeFromBackground += () =>
                    {
                        //Debug.Log("保留にした処理の実行 -> OnPlayerLeftRoom");
                        this.OnPlayerLeftRoom(otherPlayer);
                    };
                    return;
                }

                //離席通知
                this.SetPositionIds(otherPlayer.ActorNumber, -1);
            }
        }

        /// <summary>
        /// 座席セット
        /// </summary>
        private void SetPositionIds(int findNo, int setNo)
        {
            //部屋から座席情報取得
            int[] actorNums = { -1, -1, -1, -1 };
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKey.PositionIds))
            {
                actorNums = (int[])PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKey.PositionIds];
            }

            for (int i = 0; i < 4; i++)
            {
                if (actorNums[i] == findNo)
                {
                    //座席決定
                    actorNums[i] = setNo;
                    break;
                }
            }

            //座席通知
            PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable{
                { RoomPropertyKey.PositionIds, actorNums }
            });
        }

        /// <summary>
        /// プレイヤー情報更新時
        /// </summary>
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
        {
            if (this.IsBackground())
            {
                //バックグラウンドに行っている間に飛んできたイベントは、バックグラウンドからの復帰完了後にまとめて処理する
                return;
            }

            object value;

            //コイン数の同期
            if (changedProps.TryGetValue(PlayerPropertyKey.Coin, out value))
            {
                var turret = this.GetSetupedTurret(targetPlayer.ActorNumber);
                if (turret != null)
                {
                    turret.coinInfoPanel.SetCoinNum((long)value);
                }
            }

            //BET数の同期
            if (changedProps.TryGetValue(PlayerPropertyKey.Bet, out value))
            {
                var turret = this.GetSetupedTurret(targetPlayer.ActorNumber);
                if (turret != null)
                {
                    turret.uiBetButton.SetBetNum((int)value);
                }
            }

            //自分の更新情報は受け取る必要無し
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return;
            }

            //弾丸リストの同期
            if (changedProps.TryGetValue(PlayerPropertyKey.BulletList, out value))
            {
                var turret = this.GetSetupedTurret(targetPlayer.ActorNumber);
                if (turret != null)
                {
                    turret.OnUpdateBulletDataList(value);
                }
            }
        }

        /// <summary>
        /// ルーム情報更新時
        /// </summary>
        public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
        {
            object value;

            //座席情報の同期
            if (propertiesThatChanged.TryGetValue(RoomPropertyKey.PositionIds, out value))
            {
                //必要なら砲台ロード
                int[] actorNums = (int[])value;
                for (int i = 0; i < 4; i++)
                {
                    this.LoadTurretIfNeed(i, actorNums[i]);
                }
            }
        }

        /// <summary>
        /// 砲台ロード
        /// </summary>
        private void LoadTurretIfNeed(int positionId, int actorNo)
        {
            if (actorNo == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //自位置決定
                this.myPositionId = positionId;

                //回転値決定
                if (positionId % 2 > 0)
                {
                    BattleGlobal.instance.viewRotation = Quaternion.Euler(0f, 0f, 180f);
                }

                //自位置が決まるまで保留にされた処理の実行
                this.pendingLoadTurret?.Invoke();
                this.pendingLoadTurret = null;
            }

            if (!this.myPositionId.HasValue)
            {
                //Debug.LogFormat("自位置が決まってないので保留 -> LoadTurretIfNeed(positionId={0}, actorNo={1})", positionId, actorNo);
                this.pendingLoadTurret += () =>
                {
                    //Debug.LogFormat("保留にされた処理の実行 -> LoadTurretIfNeed(positionId={0}, actorNo={1})", positionId, actorNo);
                    this.LoadTurretIfNeed(positionId, actorNo);
                };
                return;
            }

            //自分位置と比べて反転が必要かどうか
            bool isFlip = positionId % 2 != this.myPositionId % 2;

            //砲台
            int turretNo = positionId / 2 * 2 + (isFlip ? 1 : 0);
            var turret = this.turrets[turretNo];

            if (actorNo < 0)
            {
                if (turret.actorNo >= 0)
                {
                    //退出した砲台はアンロード
                    turret.Unload();
                }
            }
            else if (PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNo))
            {
                if (positionId == this.myPositionId)
                {
                    //自砲台決定
                    this.myTurret = turret;
                    BattleGlobal.instance.userData.turret = turret;
                }

                if (turret.loader.GetStatus() == AssetListLoader.Status.Empty)
                {
                    //砲台ロード
                    turret.Init(actorNo);
                    turret.loader.Load(() =>
                    {
                        turret.Setup();

                        //自砲台のロードが完了したのなら
                        if (turret == this.myTurret)
                        {
                            //ゲーム開始通信（ロード中にOnDisconnectedでステートが切り替わっていたらAPIは呼ばれないようにしておく）
                            (this.manager.currentState as MainState)?.CallStartApi();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// ゲーム開始通信
        /// </summary>
        private void CallStartApi()
        {
            //通信中フラグを立てる
            this.isApiConnecting = true;

            MultiPlayApi.CallStartApi(
                worldId: this.scene.worldData.id,
                roomId: PhotonNetwork.CurrentRoom.Name,
                onCompleted: () =>
                {
                    //通信完了
                    this.isApiConnecting = false;

                    //ゲームプレイ中フラグON
                    this.scene.isPlaying = true;

                    if (PhotonNetwork.IsConnected)
                    {
                        //ローディング表示消す
                        SharedUI.Instance.HideSceneChangeAnimation(() => this.CalcCurrentLevel(UserData.Get().exp));
                        //BGM再生
                        this.scene.bgmTrack = SoundManager.Instance.PlayBgm(this.scene.worldData.bgmName);
                        //WAVE開始
                        this.scene.waveDataController.Setup();
                        this.scene.summonController.SetActorNo(PhotonNetwork.LocalPlayer.ActorNumber);
                        //定期ログ送信
                        this.sendLogCoroutine = this.scene.StartCoroutine(this.SendLog());
                        //コールバック実行
                        this.onSuccessStartApi?.Invoke();
                        this.onSuccessStartApi = null;
                    }
                    else
                    {
                        //HOMEに戻る
                        this.OnDisconnected(DisconnectCause.None);
                    }
                }
            );
        }

        /// <summary>
        /// 定期ログ送信
        /// </summary>
        private IEnumerator SendLog()
        {
            while (true)
            {
                //10秒毎
                yield return new WaitForSeconds(10f);

                //API通信中なのでスルー
                if (this.isApiConnecting) continue;

                //ショートカットメニュー開いているのでスルー
                if (this.isOpenMenu) continue;

                //定期ログ送信
                MultiPlayApi.CallLogApi(this.scene.logData);
            }
        }

        /// <summary>
        /// イベント受信時
        /// </summary>
        public override void OnEvent(EventData photonEvent)
        {
            if (0 <= photonEvent.Code && photonEvent.Code < (int)MultiEventCode.Length)
            {
                this.onEventActions[photonEvent.Code].Invoke(photonEvent);
            }
        }

        /// <summary>
        /// 初期化イベント受信時
        /// </summary>
        private void OnEvent_Init(EventData photonEvent)
        {
            //イベント受信したので待機解除
            if (this.waitInitEventCoroutine != null)
            {
                this.scene.StopCoroutine(this.waitInitEventCoroutine);
                this.waitInitEventCoroutine = null;
            }

            //ゲーム開始通信成功時コールバックセット
            this.onSuccessStartApi += () =>
            {
                var dto = new WaveResumeDto();
                dto.SetBinary((byte[])photonEvent.CustomData);

                //初期化情報のセット
                this.timeStamp = dto.timeStamp;
                this.scene.waveDataController.SetBinary(dto.waveBytes);
                this.scene.summonController.SetBinary(dto.summonBytes);
            };
        }

        /// <summary>
        /// コイン消費時
        /// </summary>
        public void OnUseCoin(uint bet)
        {
            //弾1発の値段
            uint betCoin = this.scene.worldData.betCoin;
            uint useCoin = bet * betCoin;

            //コイン消費
            this.scene.logData.consumeCoin += useCoin;

            //プロパティ更新
            this.scene.RefleshCoinCount();
        }

        /// <summary>
        /// 着弾エフェクト生成時
        /// </summary>
        public void OnCreateLandingEffect(LandingEffect landingEffect)
        {
            //自分しかいないのでイベント送信の必要なし
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                return;
            }

            //着弾エフェクト情報
            var dto = new LandingEffectDto();
            dto.localPosition = landingEffect.transform.localPosition;

            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.CreateLandingEffect,
                dto.GetBinary(),
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        /// <summary>
        /// 着弾エフェクト生成イベント受信時
        /// </summary>
        private void OnEvent_CreateLandingEffect(EventData photonEvent)
        {
            if (this.IsBackground())
            {
                //Debug.Log("OnEvent_CreateLandingEffect : 古すぎるイベントなのでスルー");
                return;
            }

            this.GetSetupedTurret(photonEvent.Sender)?.CreateLandingEffect(photonEvent.CustomData);
        }

        /// <summary>
        /// コインエフェクト生成時
        /// </summary>
        private void OnCreateCoinEffect(int getCoin, Fish fish, bool isSmall)
        {
            //自分しかいないのでイベント送信の必要なし
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                return;
            }

            //コインエフェクト情報
            var dto = new CoinEffectDto{
                isSmall = isSmall,
                getCoin = getCoin,
                position = BattleGlobal.instance.viewRotation * fish.fishCollider2D.rectTransform.anchoredPosition,
            };

            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.CreateCoinEffect,
                dto.GetBinary(),
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        /// <summary>
        /// コインエフェクト生成イベント受信時
        /// </summary>
        private void OnEvent_CreateCoinEffect(EventData photonEvent)
        {
            if (!this.scene.isPlaying)
            {
                //Debug.Log("OnEvent_CreateCoinEffect : ゲーム開始までviewRotationが定まらないのでスルー");
                return;
            }

            if (this.IsBackground())
            {
                //Debug.Log("OnEvent_CreateCoinEffect : 古すぎるイベントなのでスルー");
                return;
            }

            var dto = new CoinEffectDto();
            dto.SetBinary((byte[])photonEvent.CustomData);

            CoinEffect coinEffect = null;

            if (dto.isSmall)
            {
                coinEffect = Instantiate(this.coinEffectSmallPrefab, this.coinEffectArea, false);
                coinEffect.SetPosition(BattleGlobal.instance.viewRotation * dto.position);
            }
            else
            {
                var turret = this.GetSetupedTurret(photonEvent.Sender);
                if (turret != null)
                {
                    coinEffect = Instantiate(this.coinEffectMiddlePrefab, this.coinEffectArea, false);
                    coinEffect.SetPosition(turret.cachedTransform.localPosition + turret.cachedTransform.localRotation * new Vector3(0f, 400f));
                }
            }

            if (coinEffect != null)
            {
                coinEffect.SetFont(this.coinFontSilver);
                coinEffect.SetNum(dto.getCoin);
            }
        }

        /// <summary>
        /// 魚被ダメ時
        /// </summary>
        public void OnFishDamaged(Fish fish, int damage)
        {
            //自分しかいないのでイベント送信の必要なし
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                return;
            }

            //ダメージ情報
            var dto = new FishDamagedDto();
            dto.id = fish.id;
            dto.damage = damage;

            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.FishDamaged,
                dto.GetBinary(),
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        /// <summary>
        /// 魚被ダメイベント受信時
        /// </summary>
        private void OnEvent_FishDamaged(EventData photonEvent)
        {
            var dto = new FishDamagedDto();
            dto.SetBinary((byte[])photonEvent.CustomData);

            //魚被ダメ処理
            var fish = BattleGlobal.instance.fishList.Find(x => x.id.Equals(dto.id));
            if (fish != null)
            {
                fish.SetDamagedDto(dto);
            }
            else
            {
                //Debug.LogFormat("OnEvent_FishDamaged : 該当の魚が見つからなかったので保留にする。id = {0}", dto.id.ToString());
                this.pendingFishDtos.Add(dto);
            }
        }

        /// <summary>
        /// 魚の状態変化時
        /// </summary>
        public void OnChangeFishCondition(Fish fish)
        {
            //自分しかいないのでイベント送信の必要なし
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                return;
            }

            //状態情報
            var dto = fish.GetConditionDto();

            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.ChangeFishCondition,
                dto.GetBinary(),
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        /// <summary>
        /// 魚の状態更新イベント受信時
        /// </summary>
        private void OnEvent_ChangeFishCondition(EventData photonEvent)
        {
            var dto = new FishConditionDto();
            dto.SetBinary((byte[])photonEvent.CustomData);

            var fish = BattleGlobal.instance.fishList.Find(x => x.id.Equals(dto.id));
            if (fish != null)
            {
                //魚の状態更新
                fish.SetConditionDto(dto);
            }
            else
            {
                //Debug.LogFormat("OnEvent_ChangeFishCondition : 該当の魚が見つからなかったので保留にする。id = {0}", dto.id.ToString());
                this.pendingFishDtos.Add(dto);
            }
        }

        /// <summary>
        /// 魚生成時
        /// </summary>
        public void OnCreateFish(Fish fish)
        {
            //保留にされた魚へのデータがあるならデータ反映
            for (int i = 0; i < this.pendingFishDtos.Count; i++)
            {
                var dto = this.pendingFishDtos[i];
                if (dto.id.Equals(fish.id))
                {
                    dto.Set(fish);
                    this.pendingFishDtos.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// 魚召喚
        /// </summary>
        public void OnSummon(string key)
        {
            //魚生成
            var content = this.scene.summonController.CreateFish(key);

            //召喚エフェクト生成
            this.CreateSummonEffect(content.fish);

            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.Summon,
                content.dto.GetBinary(),
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        /// <summary>
        /// 召喚エフェクト生成
        /// </summary>
        private void CreateSummonEffect(Fish fish)
        {
            var effect = Instantiate(this.summonEffectPrefab, fish.cachedTransform.parent, false);
            effect.transform.position = fish.cachedTransform.position;

            //召喚中状態になる
            fish.conditionManager.Add(new FishConditionSummon(0.7f));
        }

        /// <summary>
        /// 魚召喚イベント受信時
        /// </summary>
        private void OnEvent_Summon(EventData photonEvent)
        {
            var dto = new SummonDto();
            dto.SetBinary((byte[])photonEvent.CustomData);

            //未来に生まれる魚なので、その時まで保留にしておく。
            //※バックグラウンドに行っている間に召喚された場合に起こり得る。
            if (this.scene.isPlaying && unchecked(dto.timeStamp - this.timeStamp) > 0)
            {
                this.pendingSummonDtos.Add(dto);
                this.pendingSummonDtos.Sort((a, b) => a.timeStamp < b.timeStamp ? -1 : 1);
            }
            else
            {
                /*
                プレイ開始前に受信したイベントの場合
                -> 即生成し、プレイ開始後のUpdateで早回しされて追い付くはず。

                その他で大きく遅延することは無いはず。
                */
                var fish = this.scene.summonController.AddFish(dto);
                this.CreateSummonEffect(fish);
            }
        }

        /// <summary>
        /// 保留した召喚魚の生成
        /// </summary>
        private void CreateSummonFishIfCan()
        {
            for (int i = 0; i < this.pendingSummonDtos.Count; i++)
            {
                if (unchecked(this.pendingSummonDtos[i].timeStamp - this.timeStamp) > 0)
                {
                    //未来に生まれる魚なので、今はまだ生成しない
                    break;
                }

                //生成時刻なので魚生成
                var fish = this.scene.summonController.AddFish(this.pendingSummonDtos[i]);
                this.CreateSummonEffect(fish);

                //生成済みデータをリストから削除
                this.pendingSummonDtos.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// FVA貫通弾チャージエフェクト生成イベント受信時
        /// </summary>
        private void OnEvent_CreateFVAPenetraionChargeEffect(EventData photonEvent)
        {
            if (this.IsBackground())
            {
                //Debug.LogWarning("OnEvent_CreateFVAPenetraionChargeEffect : 古すぎるイベントなのでスルー");
                return;
            }

            var turret = this.GetSetupedTurret(photonEvent.Sender);
            if (turret != null)
            {
                turret.CreateFVAPenetrationChargeEffect(false);
            }
        }

        /// <summary>
        /// 波動砲発射イベント受信時
        /// </summary>
        private void OnEvent_ShootLaserBeam(EventData photonEvent)
        {
            if (this.IsBackground())
            {
                //Debug.LogWarning("OnEvent_ShootLaserBeam : 古すぎるイベントなのでスルー");
                return;
            }

            this.GetSetupedTurret(photonEvent.Sender)?.OnShootLaserBeam(photonEvent.CustomData);
        }

        /// <summary>
        /// ボム発射イベント受信時
        /// </summary>
        private void OnEvent_ShootBomb(EventData photonEvent)
        {
            if (this.IsBackground())
            {
                //Debug.LogWarning("OnEvent_ShootBomb : 古すぎるイベントなのでスルー");
                return;
            }

            this.GetSetupedTurret(photonEvent.Sender)?.OnShootBomb(photonEvent.CustomData);
        }

        /// <summary>
        /// 全体弾発射イベント受信時
        /// </summary>
        private void OnEvent_ShootAllRange(EventData photonEvent)
        {
            if (this.IsBackground())
            {
                //Debug.LogWarning("OnEvent_ShootAllRange : 古すぎるイベントなのでスルー");
                return;
            }

            this.GetSetupedTurret(photonEvent.Sender)?.OnShootAllRange();
        }

        /// <summary>
        /// 魚捕獲時
        /// </summary>
        public void OnFishCatched(Fish fish, Bullet bullet)
        {
            SoundManager.Instance.PlaySe(SeName.CAPTURE_MULTI);

            //魚捕獲数増加
            var fishCountData = this.scene.logData.fishData.Find(x => x.fishId == fish.master.id);
            if (fishCountData == null)
            {
                fishCountData = new MultiPlayApi.FishCountData();
                fishCountData.fishId = fish.master.id;
                this.scene.logData.fishData.Add(fishCountData);
            }
            fishCountData.amount++;

            //弾1発の値段
            uint betCoin = this.scene.worldData.betCoin;

            //魚のレート
            uint fishRate = 0;

            if (fish.statusMaster is Master.MultiStageFishData)
            {
                //魚によるコイン獲得％増加
                fishRate = (fish.statusMaster as Master.MultiStageFishData).rate;
            }

            //獲得コイン数（1/100の位で四捨五入し、小数点以下を切り捨てる）
            int getCoin = Mathf.RoundToInt(bullet.bet * betCoin * fishRate * Masters.PercentToDecimal * 10) / 10;
            //スキルによるコイン追加獲得
            getCoin += Mathf.RoundToInt(getCoin * this.btlUserData.skill.CoinGetUp() * 10) / 10;
            //最低でも1枚は獲得
            getCoin = Mathf.Max(1, getCoin);

            //コイン増加
            this.scene.logData.addCoin += getCoin;

            //プロパティ更新
            this.scene.RefleshCoinCount();

            //コインエフェクト
            CoinEffect coinEffect = null;
            bool isSmall = false;

            //100万コイン以上なら大
            if (getCoin >= 1000000)
            {
                SoundManager.Instance.PlaySe(SeName.WIN_1);
                coinEffect = Instantiate(this.coinEffectBigPrefab, this.coinEffectArea, false);
            }
            //倍率100倍以上なら中
            else if (fishRate >= 10000)
            {
                SoundManager.Instance.PlaySe(SeName.WIN_0);
                coinEffect = Instantiate(this.coinEffectMiddlePrefab, this.coinEffectArea, false);
                coinEffect.SetPosition(this.myTurret.cachedTransform.localPosition + new Vector3(0f, 400f));
            }
            //それ以外は小
            else
            {
                coinEffect = Instantiate(this.coinEffectSmallPrefab, this.coinEffectArea, false);
                coinEffect.SetPosition(fish.fishCollider2D.rectTransform.anchoredPosition);
                isSmall = true;
            }

            coinEffect.SetNum(getCoin);

            //コインエフェクト生成を通知
            this.OnCreateCoinEffect(getCoin, fish, isSmall);

            if (!bullet.isFvAttack)
            {
                //FVポイント増加
                this.btlUserData.fvPoint += this.scene.CalcFvPointOnFishCatched(fish, bullet);

                //最大値以上にならないよう制限
                if (this.btlUserData.fvPoint > this.maxBetData.needFvPoint * 3)
                {
                    this.btlUserData.fvPoint = this.maxBetData.needFvPoint * 3;
                }

                //FVゲージ値更新
                this.myTurret.uiFvAttackGauge.RefleshView();
            }

            //獲得経験値
            uint addExp = fishRate;
            //スキルによる経験値追加獲得
            addExp += (uint)Mathf.RoundToInt(addExp * this.btlUserData.skill.ExpGetUp() * 10) / 10;
            //経験値加算
            uint exp = UserData.Get().exp + addExp;

            //最大値以上にならないよう制限
            if (exp > this.maxExp)
            {
                exp = this.maxExp;
            }

            //レベルアップ計算
            this.CalcCurrentLevel(UserData.Get().exp = exp);

            //レベルアップ処理中じゃない場合
            if (this.currentLv == UserData.Get().lv && !this.isSlot)
            {
                switch (this.feverTimeStatus)
                {
                    //フィーバータイム外の場合、可能なら龍玉取得
                    case FeverTimeStatus.None:
                    this.GetBallIfCan(fish);
                    break;

                    //フィーバータイム中の場合、可能なら龍魂取得
                    case FeverTimeStatus.Enabled:
                    this.GetSoulIfCan(fish);
                    break;
                }
            }
        }

        /// <summary>
        /// 現在レベルを計算
        /// </summary>
        private void CalcCurrentLevel(uint exp)
        {
            while (this.currentLv < this.maxLevelData.lv)
            {
                //必要経験値
                uint needExp = (uint)Masters.LevelDB
                    .GetList()
                    .Where(x => x.lv <= this.currentLv)
                    .Sum(x => x.exp);

                if (exp >= needExp)
                {
                    //レベルアップ
                    this.currentLv++;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 必要ならレベルアップ通信
        /// </summary>
        private void CallLevelUpApiIfNeed()
        {
            if (this.currentLv <= UserData.Get().lv)
            {
                //レベルアップ処理は不要
                return;
            }

            if (this.isApiConnecting)
            {
                //通信中なのでreturn
                return;
            }

            //強制的に画面から指を離した状態にさせる
            BattleGlobal.instance.turretEventTrigger.OnPointerUp(null);

            //通信中フラグ立てる
            this.isApiConnecting = true;

            //API実行後、レベルアップダイアログ開く
            UILevelUp.Open(this.scene.logData, this.levelUpPrefab, this.levelUpLayer, () =>
            {
                //コイン数が変化しているかもしれないので表示更新
                this.scene.RefleshCoinCount();

                //最大BETデータ更新
                this.maxBetData = this.GetMaxBetDataByCurrentLevel();

                //バトルアイテム所持情報に変化があったかもしれないので更新
                this.RefleshBattleItem(this.levelUpLayer, () =>
                {
                    //ここまできたら通信完了にする
                    this.isApiConnecting = false;

                    //Photonとの接続が切れていたら
                    if (!PhotonNetwork.IsConnected)
                    {
                        //HOMEに戻る
                        this.OnDisconnected(DisconnectCause.None);
                    }
                });
            });
        }

        /// <summary>
        /// 可能なら龍玉取得
        /// </summary>
        private void GetBallIfCan(Fish fish)
        {
            //9個以上は取得出来ない
            if (this.currentBallNum >= 9)
                return;

            //取得処理中なので今は取得出来ない
            if (this.currentBallNum != this.btlUserData.ballNum)
                return;
#if DEBUG
            //デバッグGET確定
            if (BattleDebug.isConfirmGetBall)
            {
                this.currentBallNum++;
                this.ballDropList.Add((fish.master.id, fish.cachedTransform.position));
                return;
            }
#endif
            var status = fish.statusMaster as Master.MultiStageFishData;
            if (status != null)
            {
                //確率計算
                var ballDropRate = Masters.MultiBallDropRateDB.GetList().Find(x => x.ballRateId == status.ballRateId && x.minHoldBall <= this.currentBallNum && this.currentBallNum <= x.maxHoldBall);
                if (ballDropRate != null && UnityEngine.Random.Range(0, 10000) < ballDropRate.rate)
                {
                    this.currentBallNum++;
                    this.ballDropList.Add((fish.master.id, fish.cachedTransform.position));
                }
            }
        }

        /// <summary>
        /// 龍魂取得計算
        /// </summary>
        private void GetSoulIfCan(Fish fish)
        {
            //9個以上は取得出来ない
            if (this.currentSoulNum >= 9)
                return;

            //取得処理中なので今は取得出来ない
            if (this.currentSoulNum != this.btlUserData.soulNum)
                return;
#if DEBUG
            //デバッグGET確定
            if (BattleDebug.isConfirmGetSoul)
            {
                this.currentSoulNum++;
                this.soulDropList.Add((fish.master.id, fish.cachedTransform.position));
                return;
            }
#endif
            var status = fish.statusMaster as Master.MultiStageFishData;
            if (status != null)
            {
                //確率計算
                var soulDropRate = Masters.MultiSoulDropRateDB.GetList().Find(x => x.soulRateId == status.soulRateId);
                if (soulDropRate != null && UnityEngine.Random.Range(0, 10000) < soulDropRate.rate)
                {
                    this.currentSoulNum++;
                    this.soulDropList.Add((fish.master.id, fish.cachedTransform.position));
                }
            }
        }

        /// <summary>
        /// 必要なら龍玉取得通信
        /// </summary>
        private void CallGetBallApiIfNeed()
        {
            if (this.currentBallNum <= this.btlUserData.ballNum)
            {
                //取得処理は不要
                return;
            }

            if (this.isApiConnecting)
            {
                //通信中なのでreturn
                return;
            }

            if (this.currentBallNum == 9)
            {
                //フィーバータイム開始するので強制的に画面から指を離した状態にさせる
                BattleGlobal.instance.turretEventTrigger.OnPointerUp(null);
            }

            //通信中フラグ立てる
            this.isApiConnecting = true;

            //取得通信
            MultiPlayApi.CallGetBallApi(this.ballDropList[0].fishId, (response) =>
            {
                //龍玉数更新
                this.btlUserData.ballNum = response.tMultiSoulBall.ball;

                //龍玉取得演出（9個揃ってたらフィーバータイム開始）
                this.myTurret.uiWhalePanel.PlayGetBallAnimation(response, this.ballDropList[0].dropPosition);

                //通信完了
                this.isApiConnecting = false;
                this.ballDropList.RemoveAt(0);

                //Photonとの接続が切れていたら
                if (!PhotonNetwork.IsConnected)
                {
                    //HOMEに戻る
                    this.OnDisconnected(DisconnectCause.None);
                }
            });
        }

        /// <summary>
        /// フィーバータイム開始時
        /// </summary>
        public void OnStartFeverTime()
        {
            this.feverTimeStatus = FeverTimeStatus.Enabled;

            //フィーバータイム中BGMに切り替え
            this.scene.bgmTrack = SoundManager.Instance.PlayBgm(BgmName.WHALEDIVE);
        }

        /// <summary>
        /// フィーバータイム終了時
        /// </summary>
        public void OnFinishedFeverTime()
        {
            this.feverTimeStatus = FeverTimeStatus.End;

            //フィーバータイム中BGMだったら、通常BGMに戻す
            this.scene.bgmTrack = SoundManager.Instance.PlayBgm(this.scene.worldData.bgmName);
        }

        /// <summary>
        /// 必要ならフィーバータイム終了通信
        /// </summary>
        private void CallFeverEndApiIfNeed()
        {
            if (this.feverTimeStatus != FeverTimeStatus.End)
            {
                return;
            }

            if (this.isApiConnecting)
            {
                //通信中なのでreturn
                return;
            }

            //通信中フラグを立てる
            this.isApiConnecting = true;

            //フィーバータイム終了をサーバーに通知
            MultiPlayApi.CallFeverEndApi(() =>
            {
                //通信完了
                this.isApiConnecting = false;

                this.feverTimeStatus = FeverTimeStatus.None;
                this.currentBallNum = 0;
                this.btlUserData.ballNum = 0;

                //Photonとの接続が切れていたら
                if (!PhotonNetwork.IsConnected)
                {
                    //HOMEに戻る
                    this.OnDisconnected(DisconnectCause.None);
                }
            });
        }

        /// <summary>
        /// 必要なら龍魂取得通信
        /// </summary>
        private void CallGetSoulApiIfNeed()
        {
            if (this.currentSoulNum <= this.btlUserData.soulNum)
            {
                //取得処理は不要
                return;
            }

            if (this.isApiConnecting)
            {
                //通信中なのでreturn
                return;
            }

            if (this.currentSoulNum % 3 == 0)
            {
                //スロット演出開始するので強制的に画面から指を離した状態にさせる
                BattleGlobal.instance.turretEventTrigger.OnPointerUp(null);
            }

            //通信中フラグ立てる
            this.isApiConnecting = true;

            //取得通信
            MultiPlayApi.CallGetSoulApi(this.soulDropList[0].fishId, (response) =>
            {
                //スロット当たってたら
                if (response.jackpotHit)
                {
                    //ユーザーデータにコイン加算
                    foreach (var addItem in response.mMultiJackpotLottery)
                    {
                        UserData.Get().AddItem((ItemType)addItem.itemType, addItem.itemId, addItem.itemNum);
                    }
                }

                //龍魂数更新
                this.btlUserData.soulNum = this.currentSoulNum = response.tMultiSoulBall.soul % 9;

                //龍魂取得演出（3つ揃ったらフィーバータイム終了してスロット開始）
                this.myTurret.uiWhalePanel.PlayGetSoulAnimation(response, this.soulDropList[0].dropPosition);

                //通信完了
                this.isApiConnecting = false;
                this.soulDropList.RemoveAt(0);

                //Photonとの接続が切れていたら
                if (!PhotonNetwork.IsConnected)
                {
                    //HOMEに戻る
                    this.OnDisconnected(DisconnectCause.None);
                }
            });
        }

        /// <summary>
        /// スロット演出開始時
        /// </summary>
        public void OnStartSlot()
        {
            if (this.feverTimeStatus == FeverTimeStatus.Enabled)
            {
                //フィーバータイム中だったら終了させる
                this.feverTimeStatus = FeverTimeStatus.End;
            }

            //フィーバータイム中BGMだったら、通常BGMに戻す
            this.scene.bgmTrack = SoundManager.Instance.PlayBgm(this.scene.worldData.bgmName);

            //スロット演出中フラグを立てる
            this.isSlot = true;
        }

        /// <summary>
        /// スロット演出終了時
        /// </summary>
        public void OnFinishedSlot()
        {
            this.isSlot = false;

            //所持コイン数表示を更新
            this.scene.RefleshCoinCount();
        }

        /// <summary>
        /// 自動照準時のターゲット選定処理
        /// </summary>
        public Fish FindTargetFish()
        {
            //生きてる画面内の魚の中から一番倍率が高い魚を探す
            return BattleGlobal.instance.fishList
                .OrderByDescending(fish => (fish.statusMaster != null) ? (fish.statusMaster as Master.MultiStageFishData).rate : 0)
                .FirstOrDefault(fish => !fish.isDead && fish.fishCollider2D.IsInScreen());
        }

        /// <summary>
        /// FVアタックボタン押下時
        /// </summary>
        public void OnClickFvAttackButton()
        {
            //FVポイント消費
            var userData = BattleGlobal.instance.userData;
            userData.fvPoint -= userData.currentBetData.needFvPoint;

            //FVゲージ表示更新
            this.myTurret.uiFvAttackGauge.RefleshView();

            //FVA発動回数カウント
            this.scene.logData.fvCount++;

            //カットイン再生
            var cutin = Instantiate(this.fvAttackCutInPrefab, this.fvaCutinArea, false);
            cutin.onFinished = () =>
            {
                Destroy(cutin.gameObject);
                this.myTurret.FvAttackFiring();
            };
        }

        /// <summary>
        /// 砲台制御機能変更時
        /// </summary>
        public void OnSetTurretController(ITurretController turretController)
        {
            //砲台制御機能が有効になった時
            if (turretController != null)
            {
                //FVゲージに禁止マークを表示
                this.myTurret.uiFvAttackGauge.SetVisibleBanMark(true);
                this.myTurret.uiFvAttackGauge.RefleshButtonInteractable();

                //砲台制御中に使用出来ないタイプのアイテムのアイコンにも禁止マークを表示
                foreach (var itemIcon in this.scene.battleItemIconManager.icons)
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
                this.myTurret.uiFvAttackGauge.SetVisibleBanMark(false);
                this.myTurret.uiFvAttackGauge.RefleshButtonInteractable();

                //アイテムアイコンの禁止マークも解除
                foreach (var itemIcon in this.scene.battleItemIconManager.icons)
                {
                    itemIcon.SetVisibleBanMark(false);
                    itemIcon.RefleshButtonInteractable();
                }
            }
        }

        /// <summary>
        /// セットアップ済み砲台の取得
        /// </summary>
        private MultiBattleTurret GetSetupedTurret(int actorNo)
        {
            return this.turrets.FirstOrDefault(x => x.actorNo == actorNo && x.isSetuped);
        }

        /// <summary>
        /// 現在のレベルでの最大BETデータを取得
        /// </summary>
        private Master.BetData GetMaxBetDataByCurrentLevel()
        {
            uint level = UserData.Get().lv;
            var levelData = Masters.LevelDB.GetList().Find(x => x.lv == level);
            return Masters.BetDB.FindById(levelData.betId);
        }

#if DEBUG
        /// <summary>
        /// デバッグ：FVMAX
        /// </summary>
        public void OnDebugFvMax()
        {
            //最大値以上にならないよう制限
            this.btlUserData.fvPoint = this.maxBetData.needFvPoint * 3;

            //FVゲージ値更新
            this.myTurret.uiFvAttackGauge.RefleshView();
        }
#endif
    }

    /// <summary>
    /// ルーム退室ステート
    /// </summary>
    private class LeaveRoomState : MyState
    {
        public string nextScenName = "Home";
        public SceneDataPackBase dataPack = null;
        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                this.OnDisconnected(DisconnectCause.None);
            }
        }

        /// <summary>
        /// 接続切断時
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            if (this.scene.isPlaying)
            {
                //ゲーム終了通信後、Homeへ戻る
                MultiPlayApi.CallClearApi(this.scene.logData, () =>
                {
                    this.scene.isPlaying = false;
                    this.BackToHome();
                });
            }
            else
            {
                //Homeへ戻る
                this.BackToHome();
            }
        }

        /// <summary>
        /// HOMEへ戻る
        /// </summary>
        private void BackToHome()
        {
            if (this.scene.bgmTrack != null)
            {
                //BGM停止後HOMEへ戻る
                SharedUI.Instance.DisableTouch();
                this.scene.bgmTrack.Stop(0.5f, () =>
                {
                    SharedUI.Instance.EnableTouch();
                    //SceneChanger.ChangeSceneAsync("Home");
                    SceneChanger.ChangeSceneAsync(this.nextScenName, this.dataPack);
                });
            }
            else
            {
                //HOMEへ戻る
                //SceneChanger.ChangeSceneAsync("Home");
                SceneChanger.ChangeSceneAsync(this.nextScenName, this.dataPack);
            }
        }
    }
}

}

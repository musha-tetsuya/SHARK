using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台カスタムシーン
/// </summary>
public partial class CustomTurretScene : SceneBase
{
    /// <summary>
    /// 砲台ビューエリア
    /// </summary>
    [SerializeField]
    private GameObject turretViewArea = null;
    /// <summary>
    /// 砲台ビュー
    /// </summary>
    [SerializeField]
    private CustomTurretView turretView = null;
    /// <summary>
    /// スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView = null;
    /// <summary>
    /// 砲台ワク拡張ボタン
    /// </summary>
    [SerializeField]
    private GameObject plusButton = null;
    /// <summary>
    /// 砲台スクロールビュー要素プレハブ
    /// </summary>
    [SerializeField]
    public CustomTurretScrollViewItem turretScrollViewItemPrefab = null;
    /// <summary>
    /// パーツスクロールビュー要素プレハブ
    /// </summary>
    [SerializeField]
    public CustomPartsScrollViewItem partsScrollViewItemPrefab = null;
    /// <summary>
    /// 砲台情報
    /// </summary>
    [SerializeField]
    private CustomTurretInfoView turretInfo = null;
    [SerializeField]
    private CustomTurretGearView gearView = null;
    /// <summary>
    /// 分母テキスト
    /// </summary>
    [SerializeField]
    private Text defominatorText = null;

    /// <summary>
    /// 装着したパーツページ移動ボタン
    /// </summary>
    [SerializeField]
    private GameObject infoAreaLeftButton = null;
    /// <summary>
    /// スキル情報ページ移動ボタン
    /// </summary>
    [SerializeField]
    private GameObject infoAreaRightButton = null;
    /// <summary>
    /// スキル情報ページ
    /// </summary>
    [SerializeField]
    private GameObject infoAreaSkillArea = null;
    /// <summary>
    /// 装着したパーツページ
    /// </summary>
    [SerializeField] GameObject infoAreaEquipArea = null;

    /// <summary>
    /// パーツ変更ボタン
    /// </summary>
    [SerializeField]
    public Button partsChangeButton = null;
    /// <summary>
    /// 決定ボタン
    /// </summary>
    [SerializeField]
    public Button decideButton = null;
    /// <summary>
    /// 確定して戻るボタン
    /// </summary>
    [SerializeField]
    public Button returnWithConfirmButton = null;
    /// <summary>
    /// 確定せず戻るボタン
    /// </summary>
    [SerializeField]
    public Button returnWithoutConfirmButton = null;

    /// <summary>
    /// パーツトグルグループ
    /// </summary>
    [SerializeField]
    public ToggleGroup partsToggleGroup = null;
    /// <summary>
    /// パーツトグルグループのアクセサリートグル
    /// </summary>
    [SerializeField]
    private Toggle accessoriesToggle = null;
    /// <summary>
    /// パーツとギア情報画面変更トグル
    /// </summary>
    [SerializeField]
    public ToggleGroup PartsAndGearChangeToggleGroup = null;
    /// <summary>
    /// パーツとギア情報画面変更のパーツトグル
    /// </summary>
    [SerializeField]
    private Toggle PartsChangeToggle = null;
    /// <summary>
    /// 砲台拡張ダイアログコンテンツ
    /// </summary>
    [SerializeField]
    private CannonWorkspaceExpansionDialogContent cannonWorkspaceExpansionDialogContent = null;
    /// <summary>
    /// アセットローダー
    /// </summary>
    private AssetListLoader assetLoader = new AssetListLoader();
    /// <summary>
    /// ステート管理
    /// </summary>
    private StateManager stateManager = new StateManager();

    private SimpleDialog dialog = null;
    private uint firstUtilityId = 0;
    private uint maxCannon = 0;

    // 最小能力値
    public static uint minBulletPower { get; private set; }
    public static uint minBarrelSpeed { get; private set; }
    public static uint minBatteryFvPoint { get; private set; }

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.assetLoader.Unload();
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.stateManager.AddState(new TurretSelectState{ scene = this });
        this.stateManager.AddState(new PartsChangeState{ scene = this });
        this.stateManager.AddState(new BatteryPartsChangeState{ scene = this });
        this.stateManager.AddState(new BarrelPartsChangeState{ scene = this });
        this.stateManager.AddState(new BulletPartsChangeState{ scene = this });
        this.stateManager.AddState(new AccessoryPartsChangeState{ scene = this });

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        // 最小能取得
        minBulletPower = Masters.BulletDB.GetList().Select(x => x.power).Min();
        minBarrelSpeed = Masters.BarrelDB.GetList().Select(x => x.speed).Min();
        minBatteryFvPoint = Masters.BatteryDB.GetList().Select(x => x.fvPoint).Min();

        TurretApi.CallUserApi(this.Load);
    }

    /// <summary>
    /// ロード
    /// </summary>
    private void Load()
    {
        uint[] batteryIds = UserData.Get().batteryData.Select(x => x.itemId).Distinct().ToArray();
        uint[] barrelIds = UserData.Get().barrelData.Select(x => x.itemId).Distinct().ToArray();
        uint[] bulletIds = UserData.Get().bulletData.Select(x => x.itemId).Distinct().ToArray();
        uint[] accessoryIds = UserData.Get().accessoriesData.Select(x => x.itemId).Distinct().ToArray();
        uint[] fvAttackTypes = null;
        uint[] seriesSkillIds = null;
        uint[] gearIds = UserData.Get().gearData.Select(x => x.gearId).Distinct().ToArray();
        var fvAttackIds = new List<uint>();
        var seriesIds = new List<uint>();

        foreach (uint id in batteryIds)
        {
            //台座スプライトのロード
            var data = Masters.BatteryDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBatterySpritePath(data.key));
            this.assetLoader.Add<GameObject>(SharkDefine.GetBatteryPrefabPath(data.key));

            //FVアタックIDを詰める
            fvAttackIds.Add(data.fvAttackId);

            //シリーズIDを詰める
            seriesIds.Add(data.seriesId);
        }

        foreach (uint id in barrelIds)
        {
            //砲身スプライトのロード
            var data = Masters.BarrelDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBarrelSpritePath(data.key));
            this.assetLoader.Add<GameObject>(SharkDefine.GetBarrelPrefabPath(data.key));

            //シリーズIDを詰める
            seriesIds.Add(data.seriesId);
        }

        foreach (uint id in bulletIds)
        {
            //砲弾プレハブ、砲弾サムネイルのロード
            var data = Masters.BulletDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBulletThumbnailPath(data.key));
            this.assetLoader.Add<BulletBase>(SharkDefine.GetBulletPrefabPath(data.key));

            //シリーズIDを詰める
            seriesIds.Add(data.seriesId);
        }

        foreach (uint id in accessoryIds)
        {
            //アクセサリサムネイルのロード
            var data = Masters.AccessoriesDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetAccessoryThumbnailPath(data.key));
        }

        //FVアタックIDからFVアタックタイプに
        fvAttackTypes = fvAttackIds
            .Distinct()
            .Select(id => Masters.FvAttackDB.FindById(id).type)
            .Distinct()
            .ToArray();

        foreach (uint type in fvAttackTypes)
        {
            //FVアタックタイプアイコンのロード
            this.assetLoader.Add<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)type));
        }

        //砲台シリーズIDからシリーズスキルIDに
        seriesSkillIds = seriesIds
            .Distinct()
            .Select(id => Masters.TurretSerieseDB.FindById(id).seriesSkillId)
            .Distinct()
            .ToArray();

        foreach (uint id in seriesSkillIds)
        {
            //シリーズスキルアイコンのロード
            var data = Masters.SerieseSkillDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetSeriesSkillIconSpritePath(data.key));
        }

        //ロード開始
        this.assetLoader.Load(this.OnLoaded);
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    private void OnLoaded()
    {
        //ローディング表示消す
        SharedUI.Instance.HideSceneChangeAnimation();

        SoundManager.Instance.PlayBgm(BgmName.HOME);
        this.stateManager.ChangeState<TurretSelectState>();
    }

    /// <summary>
    /// 砲台情報更新
    /// </summary>
    public void RefleshTurretInfo()
    {
        this.turretInfo.Reflesh(this.turretView.turretData);
    }

    /// <summary>
    /// パーツ変更ボタンクリック時
    /// </summary>
    public void OnClickPartsChangeButton()
    {
        (this.stateManager.currentState as MyState).OnClickPartsChangeButton();
    }

    /// <summary>
    /// 砲台決定ボタンクリック時
    /// </summary>
    public void OnClickTurretDecideButton()
    {
        (this.stateManager.currentState as MyState).OnClickTurretDecideButton();
    }

    /// <summary>
    /// 台座ボタンクリック時
    /// </summary>
    public void OnClickBatteryButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickBatteryButton();
    }

    /// <summary>
    /// 砲身ボタンクリック時
    /// </summary>
    public void OnClickBarrelButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickBarrelButton();
    }

    /// <summary>
    /// 砲弾ボタンクリック時
    /// </summary>
    public void OnClickBulletButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickBulletButton();
    }

    /// <summary>
    /// アクセサリボタンクリック時
    /// </summary>
    public void OnClickAccessoryButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickAccessoryButton();
    }

    /// <summary>
    /// 確定して戻るボタンクリック時
    /// </summary>
    public void OnClickReturnWithConfirmButton()
    {
        (this.stateManager.currentState as MyState).OnClickReturnWithConfirmButton();
    }

    /// <summary>
    /// 確定せず戻るボタンクリック時
    /// </summary>
    public void OnClickReturnWithoutConfirmButton()
    {
        (this.stateManager.currentState as MyState).OnClickReturnWithoutConfirmButton();
    }

    /// <summary>
    /// 枠拡張ボタンクリック時
    /// </summary>
    public void OnClickPlusButton()
    {
        (this.stateManager.currentState as MyState).OnClickPlusButton();
    }

    public void OnClickChangeGearButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickChangeGearButton();
    }

    public void OnClickChangePartsButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickChangePartsButton();
    }

    /// <summary>
    /// クリックする時、スキルページに移動
    /// </summary>
    public void OnClickChangeInfoLeftButton()
    {
        this.infoAreaLeftButton.SetActive(false);
        this.infoAreaRightButton.SetActive(true);

        this.infoAreaSkillArea.SetActive(true);
        this.infoAreaEquipArea.SetActive(false);
    }

    /// <summary>
    /// クリックする時、装着したパーツページに移動
    /// </summary>
    public void OnClickChangeInfoRightButton()
    {
        this.infoAreaLeftButton.SetActive(true);
        this.infoAreaRightButton.SetActive(false);

        this.infoAreaSkillArea.SetActive(false);
        this.infoAreaEquipArea.SetActive(true);
    }

    /// <summary>
    /// ステート基底
    /// </summary>
    public abstract class MyState : StateBase
    {
        public CustomTurretScene scene = null;
        public virtual void OnClickChangeInfoLeftButton() { }
        public virtual void OnClickChangeInfoRightButton() { }
        public virtual void OnClickPartsChangeButton(){}
        public virtual void OnClickTurretDecideButton(){}
        public virtual void OnClickReturnWithConfirmButton(){}
        public virtual void OnClickReturnWithoutConfirmButton(){}
        public virtual void OnClickBatteryButton(){}
        public virtual void OnClickBarrelButton(){}
        public virtual void OnClickBulletButton(){}
        public virtual void OnClickAccessoryButton(){}
        public virtual void OnClickPlusButton(){}
        public virtual void OnClickChangeGearButton(){}
        public virtual void OnClickChangePartsButton(){}
    }

    /// <summary>
    /// 砲台選択ステート
    /// </summary>
    private class TurretSelectState : MyState
    {
        /// <summary>
        /// フォーカス中の砲台データ
        /// </summary>
        public UserTurretData focusedTurretData { get; private set; }

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.scene.plusButton.SetActive(true);

            if (this.focusedTurretData == null)
            {
                this.focusedTurretData = UserData.Get().turretData.First(x => x.useFlg > 0);
            }

            //砲台ビューの更新
            this.scene.turretView.turretData.Set(this.focusedTurretData);
            this.scene.turretView.Reflesh();

            //砲台情報の更新
            this.scene.turretInfo.Reflesh(this.scene.turretView.turretData);

            //ボタンを表示 TODO.まだAPIがないので、アルファでは使用しない
            this.scene.partsChangeButton.gameObject.SetActive(true);
            this.scene.decideButton.gameObject.SetActive(true);

            this.scene.scrollView.Initialize(
                this.scene.turretScrollViewItemPrefab.gameObject,
                UserData.Get().turretData.Length,
                this.OnUpdateScrollViewItem
            );

            int focusIndex = Array.FindIndex(UserData.Get().turretData, (x) => x.serverId == this.focusedTurretData.serverId);
            this.scene.scrollView.SetFocus(focusIndex);

            // ログイン時の、tUtilityDataのutilityId修得
            this.scene.firstUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxCannon).utilityId;

            this.scene.maxCannon = Masters.CannonExpansionDB.FindById(this.scene.firstUtilityId).maxPossession;
            this.scene.defominatorText.text = this.scene.maxCannon.ToString();

            // TODO.Sceneの名前を比較してtrue、false判断
            this.scene.turretInfo.SetTurretInfo(true, false, true);
        }
        
        /// <summary>
        /// End
        /// </summary>
        public override void End()
        {
            //ボタンを非表示
            this.scene.partsChangeButton.gameObject.SetActive(false);
            this.scene.decideButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// スクロールビュー要素表示構築
        /// </summary>
        private void OnUpdateScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<CustomTurretScrollViewItem>();
            var data = UserData.Get().turretData[elementId];
            item.Set(
                data: data,
                isEquipped: data.useFlg > 0,
                isSelected: data.serverId == this.focusedTurretData.serverId,
                onClick: this.OnClickScrollViewItem
            );
        }

        /// <summary>
        /// スクロールビュー要素クリック時
        /// </summary>
        private void OnClickScrollViewItem(CustomTurretScrollViewItem item)
        {
            this.focusedTurretData = item.turretData;
            this.scene.turretView.turretData.Set(this.focusedTurretData);
            this.scene.turretView.Reflesh();
            this.scene.scrollView.UpdateElement();
            this.scene.turretInfo.Reflesh(this.scene.turretView.turretData);
        }

        /// <summary>
        /// パーツ変更ボタンクリック時
        /// </summary>
        public override void OnClickPartsChangeButton()
        {
            this.manager.ChangeState<PartsChangeState>();
        }

        /// <summary>
        /// 砲台決定ボタンクリック時
        /// </summary>
        public override void OnClickTurretDecideButton()
        {
            TurretApi.CallUseApi(this.focusedTurretData.settingNumber, () =>
            {
                SceneChanger.ChangeSceneAsync("Home");
            });
        }

        /// <summary>
        /// 枠拡張ボタンクリック時
        /// </summary>
        public override void OnClickPlusButton()
        {
            uint nextMaxPossession = 0;
            // 顕在枠数
            var maxPossession = this.scene.maxCannon;
            // 追加 CannonExpansionDB ID
            var nextId = Masters.CannonExpansionDB.FindById(this.scene.firstUtilityId).nextId;
            
            // 最後のIDまで実行
            if(nextId != 0)
            {
                // 次の枠数
                nextMaxPossession = Masters.CannonExpansionDB.FindById(nextId).maxPossession;
            }

            // 枠拡張詩、必要ジェム
            var needGem = Masters.CannonExpansionDB.FindById(this.scene.firstUtilityId).needGem;
            // 次の砲台　SettingNumber
            var nextSettingNumber = (uint)UserData.Get().turretData.Length + 1;
            // 枠数最大値
            uint max = Masters.CannonExpansionDB.GetList().Max(x => x.maxPossession);
            
            // 最大値まで拡張可能
            if (nextSettingNumber <= max)
            {
                this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
                this.scene.dialog.closeButtonEnabled = true;
                this.scene.dialog.titleText.text = Masters.LocalizeTextDB.Get("CannonWorkspaceExpansion");
                var content = this.scene.dialog.AddContent(this.scene.cannonWorkspaceExpansionDialogContent);
                
                var expansionButton = this.scene.dialog.AddOKButton();
                expansionButton.text.text = Masters.LocalizeTextDB.Get("Expansion");
                expansionButton.onClick = this.OnClickCannonExpansionButton;

                content.Set(this.scene.dialog, expansionButton, needGem, maxPossession, nextMaxPossession);
            }
            // 拡張不可能ダイアログ
            else
            {
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                dialog.AddText(Masters.LocalizeTextDB.Get("CannonExtendedLimit"));
                var okButton = dialog.AddOKButton();
                okButton.onClick = dialog.Close;
            }
        }

        /// <summary>
        /// 枠拡張可能ダイアログ
        /// </summary>
        private void OnClickCannonExpansionButton()
        {
            // 拡張API実行
            UtilityApi.CallExpansionCannonSetApi(
               onCompleted: () =>
               {
                   // スクロールビュー更新
                    this.scene.scrollView.Initialize(
                        this.scene.turretScrollViewItemPrefab.gameObject,
                        UserData.Get().turretData.Length,
                        this.OnUpdateScrollViewItem
                    );
                       
                    // 更新されたtUtilityID取得
                    this.scene.firstUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxCannon).utilityId;

                    // データ更新
                    this.scene.maxCannon = Masters.CannonExpansionDB.FindById(this.scene.firstUtilityId).maxPossession;
                    this.scene.defominatorText.text = this.scene.maxCannon.ToString();
                    
                    this.scene.dialog.Close();
               }
           );
        }
    }

    /// <summary>
    /// パーツ変更ステート
    /// </summary>
    private class PartsChangeState : MyState
    {
        /// <summary>
        /// 子ステートのKey
        /// </summary>
        private Type childStateType = typeof(BatteryPartsChangeState);

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.scene.plusButton.SetActive(false);
            this.scene.returnWithConfirmButton.gameObject.SetActive(true);
            this.scene.returnWithoutConfirmButton.gameObject.SetActive(true);
            this.scene.partsToggleGroup.gameObject.SetActive(true);
            this.scene.PartsAndGearChangeToggleGroup.gameObject.SetActive(true);

            // TODO.Sceneの名前を比較してtrue、false判断
            this.scene.turretInfo.SetTurretInfo(false, true, true);

            //子ステート開始
            this.manager.PushState(this.childStateType, () =>
            {
                //再開する際の子ステートのKeyを記憶
                this.childStateType = this.manager.prevState.GetType();

                //砲台選択ステートへ戻る
                this.manager.ChangeState<TurretSelectState>();
            });
        }

        /// <summary>
        /// End
        /// </summary>
        public override void End()
        {
            //ボタンを非表示
            this.scene.returnWithConfirmButton.gameObject.SetActive(false);
            this.scene.returnWithoutConfirmButton.gameObject.SetActive(false);
            this.scene.partsToggleGroup.gameObject.SetActive(false);
            this.scene.PartsAndGearChangeToggleGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// パーツ切替子ステート
    /// </summary>
    private abstract class PartsChangeChildState : MyState
    {
        /// <summary>
        /// フォーカス中のパーツデータ
        /// </summary>
        public uint focusedPartsServerId { get; protected set; }
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected abstract UserPartsData[] elementDatas { get; }
        /// <summary>
        /// スクロールビュー整列値(少ない値段から)
        /// </summary>
        private UserPartsData[] sortElementDatas;
        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            //少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.itemId).ToArray();

            //フォーカス中パーツがスクロールビュー要素の何番目かを取得
            int focusIndex = Array.FindIndex(this.sortElementDatas, (x) => x.serverId == this.focusedPartsServerId);

            //スクロールビュー初期化
            this.scene.scrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.elementDatas.Length,
                this.OnUpdateScrollViewItem
            );

            //スクロールビューのフォーカス
            this.scene.scrollView.SetFocus(focusIndex);

            //パーツ情報更新
            this.RefleshPartsInfo(this.sortElementDatas[focusIndex]);
        }

        /// <summary>
        /// スクロールビュー要素表示構築
        /// </summary>
        private void OnUpdateScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<CustomPartsScrollViewItem>();
            var data = this.sortElementDatas[elementId];
            item.Set(
                data: data,
                isSelected: data.serverId == this.focusedPartsServerId,
                onClick: this.OnClickScrollViewItem
            );
        }

        /// <summary>
        /// スクロールビュー要素クリック時
        /// </summary>
        private void OnClickScrollViewItem(CustomPartsScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.PARTS_CHANGE);
            //フォーカス中パーツの変更
            this.focusedPartsServerId = item.partsData.serverId;

            //砲台ビューのパーツを切り替えて表示更新
            this.ChangeTurretViewParts(item.partsData);
            this.scene.turretView.Reflesh();

            //切り替えたパーツで砲台情報表示更新
            this.scene.turretInfo.Reflesh(this.scene.turretView.turretData);

            //フォーカス状態を反映させるためスクロールビュー要素表示更新
            this.scene.scrollView.UpdateElement();

            //パーツ情報更新
            this.RefleshPartsInfo(item.partsData);
        }

        /// <summary>
        /// パーツ情報更新
        /// </summary>
        protected virtual void RefleshPartsInfo(UserPartsData partsData)
        {

        }

        /// <summary>
        /// 確定して戻るボタンクリック時
        /// </summary>
        public override void OnClickReturnWithConfirmButton()
        {
            //通信
            TurretApi.CallSetApi(
                settingNumber: this.scene.turretView.turretData.settingNumber,
                batteryServerId: this.scene.turretView.turretData.batteryServerId,
                barrelServerId: this.scene.turretView.turretData.barrelServerId,
                bulletServerId: this.scene.turretView.turretData.bulletServerId,
                accessoryServerId: this.scene.turretView.turretData.accessoryServerId,
                onCompleted: () =>
                {
                    //フォーカス中砲台データを現在の砲台ビューで更新
                    var turretSelectState = this.manager.GetState<TurretSelectState>();
                    turretSelectState.focusedTurretData.Set(this.scene.turretView.turretData);

                    //ステート終了
                    this.manager.PopState();
                }
            );
        }

        /// <summary>
        /// 確定せず戻るボタンクリック時
        /// </summary>
        public override void OnClickReturnWithoutConfirmButton()
        {
            this.manager.PopState();
        }

        /// <summary>
        /// 台座ボタンクリック時
        /// </summary>
        public override void OnClickBatteryButton()
        {
            this.manager.ChangeState<BatteryPartsChangeState>();
        }
        
        /// <summary>
        /// 砲身ボタンクリック時
        /// </summary>
        public override void OnClickBarrelButton()
        {
            this.manager.ChangeState<BarrelPartsChangeState>();
        }

        /// <summary>
        /// 砲弾ボタンクリック時
        /// </summary>
        public override void OnClickBulletButton()
        {
            this.manager.ChangeState<BulletPartsChangeState>();
        }

        /// <summary>
        /// アクセサリボタンクリック時
        /// </summary>
        public override void OnClickAccessoryButton()
        {
            this.manager.ChangeState<AccessoryPartsChangeState>();
        }

        /// <summary>
        /// 砲台ビューのパーツ切替
        /// </summary>
        protected abstract void ChangeTurretViewParts(UserPartsData partsData);

        /// <summary>
        /// パーツボタンクリック時
        /// </summary>
        public override void OnClickChangePartsButton()
        {
            this.scene.turretInfo.SetTurretInfo(false, true, true);
            this.scene.turretViewArea.SetActive(true);
            this.scene.gearView.SetGearView(false);
            this.scene.accessoriesToggle.gameObject.SetActive(true);
        }

        /// <summary>
        /// ギアボタンクリック時
        /// </summary>
        public override void OnClickChangeGearButton()
        {
            //TODO
            this.scene.turretInfo.SetTurretInfo(false, false, false);
            this.scene.turretViewArea.SetActive(false);
            this.scene.gearView.SetGearView(true);
            this.scene.accessoriesToggle.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 台座パーツ変更ステート
    /// </summary>
    private class BatteryPartsChangeState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().batteryData;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.focusedPartsServerId = this.scene.turretView.turretData.batteryServerId;
            base.Start();
        }

        /// <summary>
        /// 台座ボタンクリック時
        /// </summary>
        public override void OnClickBatteryButton()
        {
            //何もしない
        }

        /// <summary>
        /// 枠拡張ボタンクリック時
        /// </summary>
        public override void OnClickPlusButton()
        {
            Debug.Log("台座枠拡張");
        }

        /// <summary>
        /// 砲台ビューのパーツ切替
        /// </summary>
        protected override void ChangeTurretViewParts(UserPartsData partsData)
        {
            this.scene.turretView.turretData.SetBatteryServerId(partsData.serverId);
        }

        /// <summary>
        /// パーツ情報更新
        /// </summary>
        protected override void RefleshPartsInfo(UserPartsData partsData)
        {
            this.scene.gearView.Reflesh(partsData);
            this.scene.gearView.GearSlotReflesh(partsData);
        }
    }

    /// <summary>
    /// 砲身パーツ変更ステート
    /// </summary>
    private class BarrelPartsChangeState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().barrelData;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.focusedPartsServerId = this.scene.turretView.turretData.barrelServerId;
            base.Start();
        }

        /// <summary>
        /// 砲身ボタンクリック時
        /// </summary>
        public override void OnClickBarrelButton()
        {
            //何もしない
        }

        /// <summary>
        /// 枠拡張ボタンクリック時
        /// </summary>
        public override void OnClickPlusButton()
        {
            Debug.Log("砲身枠拡張");
        }

        /// <summary>
        /// 砲台ビューのパーツ切替
        /// </summary>
        protected override void ChangeTurretViewParts(UserPartsData partsData)
        {
            this.scene.turretView.turretData.SetBarrelServerId(partsData.serverId);
        }

        /// <summary>
        /// パーツ情報更新
        /// </summary>
        protected override void RefleshPartsInfo(UserPartsData partsData)
        {
            this.scene.gearView.Reflesh(partsData);
            this.scene.gearView.GearSlotReflesh(partsData);
        }
    }

    /// <summary>
    /// 砲弾パーツ変更ステート
    /// </summary>
    private class BulletPartsChangeState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().bulletData;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.focusedPartsServerId = this.scene.turretView.turretData.bulletServerId;
            base.Start();
        }

        /// <summary>
        /// 砲弾ボタンクリック時
        /// </summary>
        public override void OnClickBulletButton()
        {
            //何もしない
        }

        /// <summary>
        /// 枠拡張ボタンクリック時
        /// </summary>
        public override void OnClickPlusButton()
        {
            Debug.Log("砲弾枠拡張");
        }

        /// <summary>
        /// 砲台ビューのパーツ切替
        /// </summary>
        protected override void ChangeTurretViewParts(UserPartsData partsData)
        {
            this.scene.turretView.turretData.SetBulletServerId(partsData.serverId);
        }

        /// <summary>
        /// パーツ情報更新
        /// </summary>
        protected override void RefleshPartsInfo(UserPartsData partsData)
        {
            this.scene.gearView.Reflesh(partsData);
            this.scene.gearView.GearSlotReflesh(partsData);
        }
    }

    /// <summary>
    /// アクセサリパーツ変更ステート
    /// </summary>
    private class AccessoryPartsChangeState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().accessoriesData;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            this.focusedPartsServerId = this.scene.turretView.turretData.accessoryServerId;
            base.Start();
        }

        /// <summary>
        /// アクセサリボタンクリック時
        /// </summary>
        public override void OnClickAccessoryButton()
        {
            //何もしない
        }

        /// <summary>
        /// 枠拡張ボタンクリック時
        /// </summary>
        public override void OnClickPlusButton()
        {
            Debug.Log("アクセサリ枠拡張");
        }

        /// <summary>
        /// アクセサリの場合、ギアトグルをクリックしても変化なし
        /// </summary>
        public override void OnClickChangeGearButton()
        {
            this.scene.PartsChangeToggle.isOn = true;
        }

        /// <summary>
        /// 砲台ビューのパーツ切替
        /// </summary>
        protected override void ChangeTurretViewParts(UserPartsData partsData)
        {
            this.scene.turretView.turretData.SetAccessoryServerId(partsData.serverId);
        }
    }
}

using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ロック・分解のチェック
/// </summary>
public class TempFlgData
{
    public uint serverId;
    public uint checkFlg;

    public TempFlgData(uint serverId, uint checkFlg)
    {
        this.serverId = serverId;
        this.checkFlg = checkFlg;
    }
}

/// <summary>
/// アイテムインベントのメインリシーン
/// </summary>
public class ItemInventoryScene : SceneBase
{
    /// <summary>
    /// パーツトグルグループ
    /// </summary>
    [SerializeField]
    private GameObject partsToggle = null;
    /// <summary>
    /// 台座トグル
    /// </summary>
    [SerializeField]
    private Toggle batteryToggle = null;
    /// <summary>
    /// 装飾トグル
    /// </summary>
    [SerializeField]
    private Toggle decorationToggle = null;
    /// <summary>
    /// アイテムスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView itemScrollView = null;
    /// <summary>
    /// アイテムスクロールビュー要素プレハブ
    /// </summary>
    [SerializeField]
    private ItemInventoryPartsScrollViewItem partsScrollViewItemPrefab = null;
    /// <summary>
    /// パーツダイアログコンテンツプレハブ
    /// </summary>
    [SerializeField]
    private ItemInventoryPartsDialogContent partsDialogContentPrefab = null;
    /// <summary>
    /// ギアダイアログコンテンツプレハブ
    /// </summary>
    [SerializeField]
    private ItemInventoryGearDialogContent gearDialogContentPrefab = null;
    /// <summary>
    /// その他アイテムダイアログコンテンツプレハブ
    /// </summary>
    [SerializeField]
    private ItemInventoryOtherDialogContent otherDialogContentPrefab = null;
    /// <summary>
    /// まとめて分解時分解ボタンをクリック時
    /// </summary>
    [SerializeField]
    private InventoryDecompositionAllDialogContent decompositionAllDialogContentPrefab = null;
    /// <summary>
    /// アイテム拡張ボタンクリック時、ダイアログ
    /// </summary>
    [SerializeField]
    private ItemInventoryPlusPossessionDialogContent plusPossessionDialogContentPrefab = null;
    /// <summary>
    /// 所持量オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject partsPossessionObject = null;
    /// <summary>
    /// ギア所持量オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject gearPossessionObject = null;

    [Header("ボタン")]
    /// <summary>
    /// ロック確認ボタン
    /// </summary>
    [SerializeField]
    private Button confirmButton = null;
    /// <summary>
    /// ロックボタン
    /// </summary>
    [SerializeField]
    private Button lockButton = null;
    /// <summary>
    /// まとめて分解ボタン
    /// </summary>
    [SerializeField]
    private Button decompositioncollectButton = null;
    /// <summary>
    /// まとめて分解ボタン確認
    /// </summary>
    [SerializeField]
    private Button decompositionAllConfirmButton = null;
    /// <summary>
    /// まとめて分解キャンセル
    /// </summary>
    [SerializeField]
    private Button decompositionAllCancelButton = null;
    /// <summary>
    /// 枠拡張
    /// </summary>
    [SerializeField]
    private Button plusPossessioExpansionButton = null;

    [Header("テキスト")]
    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField]
    private Text selectingText = null;
    // 選択中アイテムカウンターテキスト
    [SerializeField]
    private Text selectingCount = null;
    // 所持制限テキスト
    [SerializeField]
    private Text possessionCount = null;
    // ギア所持制限テキスト
    [SerializeField]
    private Text gearPossessionCount = null;
    // 台座所持制限テキスト
    [SerializeField]
    private Text batteryPossessionCount = null;
    // 砲身所持制限テキスト
    [SerializeField]
    private Text barrelPossessionCount = null;
    // 弾丸所持制限テキスト
    [SerializeField]
    private Text bulletPossessionCount = null;

    /// <summary>
    /// ステート管理
    /// </summary>
    private StateManager stateManager = new StateManager();
    /// <summary>
    /// アセットローダー
    /// </summary>
    private AssetListLoader assetLoader = new AssetListLoader();

    private SimpleDialog dialog = null;
    
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
        this.stateManager.AddState(new PartsChangeState{ scene = this });
        this.stateManager.AddState(new BatteryItemState{ scene = this });
        this.stateManager.AddState(new BarrelItemState{ scene = this });
        this.stateManager.AddState(new BulletItemState{ scene = this });
        this.stateManager.AddState(new DecorationItemState{ scene = this });
        this.stateManager.AddState(new GearItemChangeState{ scene = this });
        this.stateManager.AddState(new GearBatteryItemState{ scene = this });
        this.stateManager.AddState(new GearBarrelItemState{ scene = this });
        this.stateManager.AddState(new GearBulletItemState{ scene = this });
        this.stateManager.AddState(new OtherItemState{ scene = this });

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        // Apiロード
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
        uint[] decorationIds = UserData.Get().accessoriesData.Select(x => x.itemId).Distinct().ToArray();
        uint[] gearIds = UserData.Get().gearData.Select(x => x.gearId).Distinct().ToArray();
        uint[] otherItemIds = UserData.Get().itemData.Select(x => x.itemId).Distinct().ToArray();
        // 分解詩、所持していないバトルアイテムもロード
        uint[] allIotherItemIds = Masters.BattleItemDB.GetList().Select(x => x.id).Distinct().ToArray();
        uint[] fvAttackTypes = null;
        var fvAttackIds = new List<uint>();

        foreach(uint id in batteryIds)
        {  
            // 台座スプライトのロード
            var data = Masters.BatteryDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBatterySpritePath(data.key));

            //FVアタックIDを詰める
            fvAttackIds.Add(data.fvAttackId);
        }

        foreach(uint id in barrelIds)
        {
            // 砲身スプライトのロード
            var data = Masters.BarrelDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBarrelSpritePath(data.key));
        }

        foreach(uint id in bulletIds)
        {
            // 砲弾サムネイルのロード
            var data = Masters.BulletDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBulletThumbnailPath(data.key));
        }

        foreach(uint id in decorationIds)
        {
            // アクセサリサムネイルのロード
            var data = Masters.AccessoriesDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetAccessoryThumbnailPath(data.key));
        }

        foreach(uint id in allIotherItemIds)
        {
            // バトルアイテムスプライトのロード
            var data = Masters.BattleItemDB.FindById(id);
            this.assetLoader.Add<Sprite>(SharkDefine.GetBattleItemIconSpritePath(data.key));
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

        // ロード開始
        this.assetLoader.Load(this.OnLoaded);
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    private void OnLoaded()
    {
        //ローディング表示消す
        SharedUI.Instance.HideSceneChangeAnimation();

        // PartsChangeState移動
        this.stateManager.ChangeState<PartsChangeState>();
    }

    /// <summary>
    /// 砲台ボタンクリック時
    /// </summary>
    public void OnClickTurretButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickTurretButton();
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
    /// アクセサリーボタンクリック時
    /// </summary>
    public void OnClickDecorationButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickDecorationButton();
    }
    
    /// <summary>
    /// ギアボタンクリック時
    /// </summary>
    public void OnClickGearButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickGearButton();
    }

    /// <summary>
    /// その他(バトルアイテム)ボタンクリック時
    /// </summary>
    public void OnClickOtherButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickOtherButton();
    }

    /// <summary>
    /// ロックボタンクリック時
    /// </summary>
    public void OnClickLockButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickLockButton();
    }

    /// <summary>
    /// ロック確認ボタンクリック時
    /// </summary>
    public void OnClickCollectConfirmButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickCollectConfirmButton();
    }

    /// <summary>
    /// まとめて分解ボタンクリック時
    /// </summary>
    public void OnClickDecompositionButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickDecompositionButton();
    }

    /// <summary>
    /// まとめて分解確認ボタンクリック時
    /// </summary>
    public void OnClickDecompositionAllConfirmButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickDecompositionAllConfirmButton();
    }

    /// <summary>
    /// まとめて分解キャンセルボタンクリック時
    /// </summary>
    public void OnClickDecompositionAllCancelButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickDecompositionAllCancelButton();
    }

    /// <summary>
    /// 枠拡張ボタンクリック時
    /// </summary>
    public void OnClickPlusItemPossessionButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        (this.stateManager.currentState as MyState).OnClickPlusItemPossessionButton();
    }

    /// <summary>
    /// ステート基底
    /// </summary>
    public abstract class MyState : StateBase
    {
        public ItemInventoryScene scene = null;
        public List<uint> changeValuePartIdList = new List<uint>();
        public virtual void OnClickTurretButton(){}
        public virtual void OnClickBatteryButton(){}
        public virtual void OnClickBarrelButton(){}
        public virtual void OnClickBulletButton(){}
        public virtual void OnClickDecorationButton(){}
        public virtual void OnClickGearButton(){}
        public virtual void OnClickOtherButton(){}
        public virtual void OnClickLockButton(){}
        public virtual void OnClickCollectConfirmButton(){}
        public virtual void OnClickDecompositionButton(){}
        public virtual void OnClickDecompositionAllConfirmButton(){}
        public virtual void OnClickDecompositionAllCancelButton(){}
        public virtual void OnClickPlusItemPossessionButton(){}
    }

    /// <summary>
    /// ロード.砲台ボタンクリック時
    /// </summary>
    private class PartsChangeState : MyState
    {
        /// <summary>
        /// 子ステートのKey
        /// </summary>
        private Type childStateType = typeof(BatteryItemState);

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // パーツのトグルボタンセットOn
            this.scene.partsToggle.SetActive(true);
            // 所持量オブジェクトセットOn/Off
            this.scene.partsPossessionObject.SetActive(true);
            this.scene.gearPossessionObject.SetActive(false);
            // Plusボタンon
            this.scene.plusPossessioExpansionButton.gameObject.SetActive(true);
            // BatteryItemStateへ移動
            this.scene.stateManager.ChangeState<BatteryItemState>();
        }
    }

    /// <summary>
    /// パーツ切替子ステート
    /// </summary>
    private abstract class PartsChangeChildState : MyState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected abstract UserPartsData[] elementDatas { get; }
        /// <summary>
        /// パーツタイプ
        /// </summary>
        protected abstract uint partType { get; }
        /// <summary>
        /// スクロールビュー整列値(少ない値段から)
        /// </summary>
        private UserPartsData[] sortElementDatas;
        /// <summary>
        /// サーバに適用前のクライアントのLockFlg
        /// </summary>
        private uint isChangeLockFlg;
        // 所持中パーツテキスト
        private uint partsPossession;
        /// <summary>
        /// パーツUtilityId
        /// </summary>
        private uint partsUtilityId;
        /// <summary>
        /// ロック・分解のチェック、臨時データ
        /// </summary>
        private List<TempFlgData> lockTargetList = new List<TempFlgData>();

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // 臨時リストクリア
            this.lockTargetList.Clear();

            // Item PrefabセットOn
            this.scene.itemScrollView.gameObject.SetActive(true);
            // 少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.itemId).ToArray();

            // スクロールビュー初期化
            this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.elementDatas.Length,
                this.OnUpdatePartsScrollViewItem
            );

            //// APIに送信するServerIdリスト初期化
            this.changeValuePartIdList.Clear();

            // ボタンセット
            this.scene.confirmButton.gameObject.SetActive(false);
            this.scene.decompositioncollectButton.gameObject.SetActive(true);
            this.scene.lockButton.gameObject.SetActive(true);
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(false);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(false);
            this.scene.selectingText.text = null;
            this.scene.selectingCount.text = null;
            this.scene.plusPossessioExpansionButton.interactable = true;

            // パーツ所持制限UtilityId取得
            this.partsUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxParts).utilityId;

            // パーツの所持制限マスターデータ
            var partsMaxCount = Masters.PartsExpansionDB.FindById(this.partsUtilityId).maxPossession;

            // 所持中パーツ
            this.partsPossession = (uint)this.sortElementDatas.Length;

            // 所持·所持制限テキスト
            this.scene.possessionCount.text = Masters.LocalizeTextDB.GetFormat("ItemPossessionCount", partsPossession, partsMaxCount);
        }

        /// <summary>
        /// スクロールビュー要素表示構築
        /// </summary>
        private void OnUpdatePartsScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryPartsScrollViewItem>();
            var itemInfo = gobj.GetComponent<CommonIcon>();
            var data = this.sortElementDatas[elementId];
            itemInfo.gearArea.SetActive(true);
            
            // パーツデータセット
            item.Set(
                data: data,
                isEquipped: data.useFlg,
                isLock: data.lockFlg,
                onClick: this.OnClickPartsScrollViewItem
            );
        }
        
        /// <summary>
        /// スクロールビュー要素クリック時
        /// </summary>
        private void OnClickPartsScrollViewItem(ItemInventoryPartsScrollViewItem item)
        {
            // 詳細ダイアログ
            var localize =  Masters.LocalizeTextDB;
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            dialog.titleText.text = localize.Get("PartsDetailTitle");
            var content = dialog.AddContent(this.scene.partsDialogContentPrefab);
            SoundManager.Instance.PlaySe(SeName.YES);
            
            // パーツ情報ロード
            content.SetPartsData(
                data: item.partsData,
                isEquipped: item.partsData.useFlg,
                onReflesh: () =>
                {
                    this.scene.itemScrollView.UpdateElement();
                },
                onRefleshDecomposition: () =>
                {
                    Start();
                    dialog.Close();
                }
            );
        }

        /// <summary>
        /// 台座ボタンクリック時
        /// </summary>
        public override void OnClickBatteryButton()
        {
            this.manager.ChangeState<BatteryItemState>();
        }

        /// <summary>
        /// 砲身ボタンクリック時
        /// </summary>
        public override void OnClickBarrelButton()
        {
            this.manager.ChangeState<BarrelItemState>();
        }

        /// <summary>
        /// 砲弾ボタンクリック時
        /// </summary>
        public override void OnClickBulletButton()
        {
            this.manager.ChangeState<BulletItemState>();
        }

        /// <summary>
        /// アクセサリーボタンクリック時
        /// </summary>
        public override void OnClickDecorationButton()
        {
            this.manager.ChangeState<DecorationItemState>();
        }

        /// <summary>
        /// ギアボタンクリック時
        /// </summary>
        public override void OnClickGearButton()
        {
            this.manager.ChangeState<GearItemChangeState>();
            // 台座トグル On
            this.scene.batteryToggle.isOn = true;
            // 装飾トグル SetActive Off
            this.scene.decorationToggle.gameObject.SetActive(false);
        }

        /// <summary>
        /// その他(バトルアイテム)ボタンクリック時
        /// </summary>
        public override void OnClickOtherButton()
        {
            this.manager.ChangeState<OtherItemState>();
        }

        /// <summary>
        /// まとめて、ロックボタン
        /// </summary>
        public override void OnClickLockButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            // 少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.itemId).ToArray();

            for(int i = 0; i< sortElementDatas.Length; i++)
            {
                var serverId = sortElementDatas[i].serverId;
                var checkFlg = sortElementDatas[i].lockFlg;
                this.lockTargetList.Add(new TempFlgData(serverId, checkFlg));
            }

            // スクロールビューアイテム再生性
            this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.sortElementDatas.Length,
                this.OnUpdateLockPartsScrollViewItem
            );

            // ボタンセット
            this.scene.confirmButton.gameObject.SetActive(true);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);
            this.scene.lockButton.gameObject.SetActive(false);
            this.scene.plusPossessioExpansionButton.interactable = false;
        }

        /// <summary>
        /// スクロールビューアイテムセット
        /// </summary>
        private void OnUpdateLockPartsScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryPartsScrollViewItem>();
            var itemInfo = gobj.GetComponent<CommonIcon>();
            var data = this.sortElementDatas[elementId];
            itemInfo.gearArea.SetActive(true);
            var checkFlg = this.lockTargetList.Find(x => x.serverId == data.serverId).checkFlg;
            
            // パーツデータセット
            item.Set(
                data: data,
                isEquipped: data.useFlg,
                isLock: checkFlg,
                onClick: this.OnClickPartsItemLockButton
            );

            // 仮ロックフラッグチェック更新
            if(checkFlg > 0)
            {
                item.SetTemplockImage(checkFlg);
            }
        }

        /// <summary>
        /// アイテムクリック時
        /// </summary>
        private void OnClickPartsItemLockButton(ItemInventoryPartsScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            
            // Api通信前 LockImage切り替え
            if (item.lockImage.gameObject.activeInHierarchy)
            {
                // 臨時リストがら、クリックした、データ削除
                var partsData = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId);
                this.lockTargetList.Remove(partsData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.partsData.serverId, 0));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId).checkFlg;
                // 仮ロックフラッグチェック更新
                item.SetTemplockImage(checkFlg);
                this.isChangeLockFlg = checkFlg;
            }
            else
            {
                // 臨時リストがら、クリックした、データ削除
                var partsData = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId);
                this.lockTargetList.Remove(partsData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.partsData.serverId, 1));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId).checkFlg;
                // 仮ロックフラッグチェック更新
                item.SetTemplockImage(checkFlg);
                this.isChangeLockFlg = checkFlg;
            }

            // 変更するID取得
            var changeId = this.elementDatas
            .Where(x => x.serverId == item.partsData.serverId && item.partsData.lockFlg != isChangeLockFlg)
            .Select(x => x.serverId)
            .SingleOrDefault();

            // 変更するIDリストに追加
            if (changeId > 0)
            {
                changeValuePartIdList.Add(changeId);
            }

            // 多重クリックで、データ変化がないデータ
            var deleteId = this.elementDatas
            .Where(x => x.serverId == item.partsData.serverId && item.partsData.lockFlg == isChangeLockFlg)
            .Select(x => x.serverId)
            .SingleOrDefault();
            
            // リストで、変化がないId 削除
            this.changeValuePartIdList.Remove(deleteId);
        }

        /// <summary>
        /// まとめて分解ボタンクリック時
        /// </summary>
        public override void OnClickDecompositionButton()
        {
            // 少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.itemId).ToArray();

            // 分解臨時リストセット
            for(int i = 0; i < sortElementDatas.Length; i++)
            {
                var serverId = sortElementDatas[i].serverId;
                this.lockTargetList.Add(new TempFlgData(serverId, 0));
            }

            // アイテムセット
            this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.sortElementDatas.Length,
                this.OnUpdateDecompositionPartsScrollViewItem
            );

            // ボタンセット
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(true);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(true);
            this.scene.lockButton.gameObject.SetActive(false);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);
            // 拡張ボタンクリック禁止
            this.scene.plusPossessioExpansionButton.interactable = false;
        }

        /// <summary>
        /// まとめて分解アイテムデータロード
        /// </summary>
        private void OnUpdateDecompositionPartsScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryPartsScrollViewItem>();
            var itemInfo = gobj.GetComponent<CommonIcon>();
            var data = this.sortElementDatas[elementId];
            var checkFlg = this.lockTargetList.Find(x => x.serverId == data.serverId).checkFlg;
            itemInfo.gearArea.SetActive(true);

            item.SetCheckBox(
                data: data,
                isEquipped: data.useFlg,
                isLock: data.lockFlg,
                checkFlg: checkFlg,
                onClick: this.OnClickPartsItemDecompositionButton
            );

            // まとめて分解の時アイテムをクリック時選択中パーツの個数表示
            this.scene.selectingCount.text = this.changeValuePartIdList.Count.ToString();
            this.scene.selectingText.text = Masters.LocalizeTextDB.Get("Selecting");

            this.scene.decompositionAllConfirmButton.interactable = this.changeValuePartIdList.Count > 0;

            if(checkFlg > 0)
            {
                item.SetTempCheckImage(checkFlg);
            }
        }

        /// <summary>
        /// まとめて分解の時アイテムをクリック時
        /// </summary>
        private void OnClickPartsItemDecompositionButton(ItemInventoryPartsScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);

            // ボタン切り替え
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(true);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(true);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);

            // Api通信前 LockImage切り替え
            if (item.checkImage.gameObject.activeInHierarchy)
            {
                // 臨時リストがら、クリックした、データ削除
                var partsData = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId);
                this.lockTargetList.Remove(partsData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.partsData.serverId, 0));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId).checkFlg;
                // 仮選択フラッグチェック更新
                item.SetTempCheckImage(checkFlg);
                // リストのID削除
                this.changeValuePartIdList.Remove(item.partsData.serverId);
            }
            else
            {
                // 臨時リストがら、クリックした、データ削除
                var partsData = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId);
                this.lockTargetList.Remove(partsData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.partsData.serverId, 1));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.partsData.serverId).checkFlg;
                // 仮選択フラッグチェック更新
                item.SetTempCheckImage(checkFlg);
                // リストにID追加
                this.changeValuePartIdList.Add(item.partsData.serverId);
            }

            // まとめて分解の時アイテムをクリック時選択中パーツの個数表示
            this.scene.selectingCount.text = this.changeValuePartIdList.Count.ToString();
            this.scene.selectingText.text = Masters.LocalizeTextDB.Get("Selecting");

            this.scene.decompositionAllConfirmButton.interactable = this.changeValuePartIdList.Count > 0;
        }

        // パーツ枠拡張
        public override void OnClickPlusItemPossessionButton()
        {
            // 次の拡張ID 0の場合なし
            uint nextId = Masters.PartsExpansionDB.FindById(this.partsUtilityId).nextId;

            // 拡張不可能ダイアログ
            if(nextId == 0)
            {
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                dialog.AddText(Masters.LocalizeTextDB.Get("CannonExtendedLimit"));
                var backButton = dialog.AddOKButton();
                backButton.onClick = dialog.Close;
            }
            // 拡張可能ダイアログ
            else
            {
                // ダイアログセット
                var localize = Masters.LocalizeTextDB;
                this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
                this.scene.dialog.closeButtonEnabled = true;
                this.scene.dialog.titleText.text = localize.Get("PlusPartsPossession");
                var content = this.scene.dialog.AddContent(this.scene.plusPossessionDialogContentPrefab);
                var okButton = this.scene.dialog.AddOKButton();
                okButton.text.text = Masters.LocalizeTextDB.Get("Expansion");
                okButton.onClick = this.OnClickPlusItemPossession;

                // コンテンツデータロード
                content.Set(this.partType, this.partsUtilityId, this.scene.dialog, okButton);
            }
        }

        /// <summary>
        /// 拡張ボタンクリック時、API実行
        /// </summary>
        private void OnClickPlusItemPossession()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            UtilityApi.CallExpansionCannonApi(this.onCompletedPlusPartsPossession);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void onCompletedPlusPartsPossession()
        {
            this.Start();
            this.scene.dialog.Close();
        }
    }

    /// <summary>
    /// 台座パーツステート
    /// </summary>
    private class BatteryItemState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().batteryData;
        /// <summary>
        /// パーツタイプ
        /// </summary>
        protected override uint partType => (uint)ItemType.Battery;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallCannonLockList((uint)ItemType.Battery, this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<BatteryItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Battery, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<BatteryItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellCannonList(partType, this.changeValuePartIdList, this.OnCompletedPartsDecompositionConfirm);            
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedPartsDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<BatteryItemState>();
        }
    }

    /// <summary>
    /// 砲身パーツステート
    /// </summary>
    private class BarrelItemState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().barrelData;
        /// <summary>
        /// パーツタイプ
        /// </summary>
        protected override uint partType => (uint)ItemType.Barrel;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallCannonLockList((uint)ItemType.Barrel, this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<BarrelItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Barrel, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<BarrelItemState>();
        }
        
        // <summary>
        /// API まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellCannonList(partType, this.changeValuePartIdList, this.OnCompletedPartsDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedPartsDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<BarrelItemState>();
        }
    }

    /// <summary>
    /// 砲弾パーツステート
    /// </summary>
    private class BulletItemState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().bulletData;
        /// <summary>
        /// パーツタイプ
        /// </summary>
        protected override uint partType => (uint)ItemType.Bullet;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallCannonLockList((uint)ItemType.Bullet, this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<BulletItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Bullet, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<BulletItemState>();
        }

        /// <summary>
        /// API まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellCannonList(partType, this.changeValuePartIdList, this.OnCompletedPartsDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedPartsDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<BulletItemState>();
        }
    }

    /// <summary>
    /// アクセサリパーツステート
    /// </summary>
    private class DecorationItemState : PartsChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected override UserPartsData[] elementDatas => UserData.Get().accessoriesData;
        /// <summary>
        /// パーツタイプ
        /// </summary>
        protected override uint partType => (uint)ItemType.Accessory;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallCannonLockList((uint)ItemType.Accessory, this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<DecorationItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Accessory, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<DecorationItemState>();
        }

        /// <summary>
        /// API まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellCannonList(partType, this.changeValuePartIdList, this.OnCompletedPartsDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedPartsDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<DecorationItemState>();
        }
    }

    /// <summary>
    /// ギアボタンクリック時
    /// </summary>
    private class GearItemChangeState : MyState
    {
        /// <summary>
        /// 子ステートのKey
        /// </summary>
        private Type childStateType = typeof(GearBatteryItemState);

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // パーツのトグルボタンセットOn
            this.scene.partsToggle.SetActive(true);
            // 所持量オブジェクトセットOn/Off
            this.scene.partsPossessionObject.SetActive(false);
            this.scene.gearPossessionObject.SetActive(true);
            // Plusボタンon
            this.scene.plusPossessioExpansionButton.gameObject.SetActive(true);
            // GearBatteryItemStateへ移動
            this.scene.stateManager.ChangeState<GearBatteryItemState>();
        }
    }

    /// <summary>
    /// ギアステート
    /// </summary>
    private abstract class GearChangeChildState : MyState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        protected abstract UserGearData[] elementDatas { get; }
        /// <summary>
        /// スクロールビュー整列値(少ない値段から)
        /// </summary>
        private UserGearData[] sortElementDatas;
        /// <summary>
        /// サーバに適用前のクライアントのLockFlg
        /// </summary>
        private uint isChangeLockFlg;
        /// <summary>
        /// 所持中パーツテキスト
        /// </summary>
        private uint gearPossession;
        /// <summary>
        /// ギアUtilityId
        /// </summary>
        protected uint gearUtilityId;
        /// <summary>
        /// ロック・分解のチェック、臨時データ
        /// </summary>
        private List<TempFlgData> lockTargetList = new List<TempFlgData>();

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // 臨時リストクリア
            this.lockTargetList.Clear();

            // ギアがない場合
            if(elementDatas.Length == 0)
            {
            }
            else
            {
                // itemScrollViewセットOn
                this.scene.itemScrollView.gameObject.SetActive(true);
                // 少ない値から順序別に整列
                this.sortElementDatas = this.elementDatas.OrderBy(x => x.gearId).ToArray();
            
                // スクロールビュー初期化
                this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.elementDatas.Length,
                this.OnUpdateGearScrollViewItem
                );
            }

            // APIに送信するServerIdリスト初期化
            this.changeValuePartIdList.Clear();

            // ボタンセット
            this.scene.confirmButton.gameObject.SetActive(false);
            this.scene.decompositioncollectButton.gameObject.SetActive(true);
            this.scene.lockButton.gameObject.SetActive(true);
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(false);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(false);
            this.scene.plusPossessioExpansionButton.interactable = true;
            this.scene.selectingText.text = null;
            this.scene.selectingCount.text = null;
            // ギアデータ
            var gearData = UserData.Get().gearData;

            // ギア所持制限UtilityId取得
            this.gearUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxGear).utilityId;

            // パーツの所持制限マスターデータ
            var gearMaxCount = Masters.GearExpansionDB.FindById(this.gearUtilityId).maxPossession;
            // 現在所持しているギアの数
            var gearTotalCount = gearData.Length;
            
            // 所持·所持制限テキストセット
            this.scene.gearPossessionCount.text = Masters.LocalizeTextDB.GetFormat("ItemPossessionCount", gearTotalCount, gearMaxCount);
            this.scene.batteryPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Battery).ToString();
            this.scene.barrelPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Barrel).ToString();
            this.scene.bulletPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Bullet).ToString();
        }

        /// <summary>
        /// スクロールビュー要素表示構築
        /// </summary>
        private void OnUpdateGearScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryGearScrollViewItem>();
            var data = this.sortElementDatas[elementId];
            // 装着中ギア装着中パンネル表示
            var isEquipped = data.partsServerId > 0;
            
            // ギアデータセット
            item.Set(
                data: data,
                isEquipped: isEquipped,
                isLock: data.lockFlg,
                onClick: this.OnClickGearScrollViewItem
            );
        }

        /// <summary>
        /// スクロールビュー要素クリック時
        /// </summary>
        private void OnClickGearScrollViewItem(ItemInventoryGearScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);

            // 詳細ダイアログ
            var localize =  Masters.LocalizeTextDB;
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            dialog.titleText.text = localize.Get("GearDetailTitle");
            var content = dialog.AddContent(this.scene.gearDialogContentPrefab);

            // ギア情報ロード
            content.SetGearData(
                data: item.gearData,
                onReflesh: () =>
                {
                    this.scene.itemScrollView.UpdateElement();
                },
                onRefleshDecomposition: () =>
                {
                    Start();
                    // ダイアログ閉じる
                    dialog.Close();
                }
            );
        }

        /// <summary>
        /// 台座ボタンクリック時
        /// </summary>
        public override void OnClickTurretButton()
        {
            this.manager.ChangeState<PartsChangeState>();
            // 台座トグル On
            this.scene.batteryToggle.isOn = true;
            // 装飾トグル SetActive On
            this.scene.decorationToggle.gameObject.SetActive(true);
        }

        public override void OnClickBatteryButton()
        {
            this.manager.ChangeState<GearBatteryItemState>();
        }

        public override void OnClickBarrelButton()
        {
            this.manager.ChangeState<GearBarrelItemState>();
        }

        public override void OnClickBulletButton()
        {
            this.manager.ChangeState<GearBulletItemState>();
        }
        
        /// <summary>
        /// その他(バトルアイテム)ボタンクリック時
        /// </summary>
        public override void OnClickOtherButton()
        {
            this.manager.ChangeState<OtherItemState>();
        }

        /// <summary>
        /// /// <summary>
        /// まとめて、ロックボタン
        /// </summary>
        public override void OnClickLockButton()
        {
            // 少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.gearId).ToArray();

            // ロック臨時リストセット
            for(int i = 0; i < sortElementDatas.Length; i++)
            {
                var serverId = sortElementDatas[i].serverId;
                var checkFlg = sortElementDatas[i].lockFlg;
                this.lockTargetList.Add(new TempFlgData(serverId, checkFlg));
            }

            // スクロールビューアイテム再生性
            this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.sortElementDatas.Length,
                this.OnUpdateLockGearScrollViewItem
            );

            // ボタンセット
            this.scene.confirmButton.gameObject.SetActive(true);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);
            this.scene.lockButton.gameObject.SetActive(false);
            // 拡張ボタンクリック禁止
            this.scene.plusPossessioExpansionButton.interactable = false;
        }

        /// <summary>
        /// スクロールビューアイテムセット
        /// </summary>
        private void OnUpdateLockGearScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryGearScrollViewItem>();
            var itemInfo = gobj.GetComponent<CommonIcon>();
            var data = this.sortElementDatas[elementId];            
            var isEquipped = data.partsServerId > 0;
            var checkFlg = this.lockTargetList.Find(x => x.serverId == data.serverId).checkFlg;

            // ギアデータセット
            item.Set(
                data: data,
                isEquipped: isEquipped,
                isLock: checkFlg,
                onClick: this.OnClickGearItemLockButton
            );

            // 仮ロックフラッグチェック更新
            if(checkFlg > 0)
            {
                item.SetTemplockImage(checkFlg);
            }
        }

        /// <summary>
        /// アイテムクリック時
        /// </summary>
        private void OnClickGearItemLockButton(ItemInventoryGearScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);

            // LockImage切り替え
            if(item.lockImage.gameObject.activeInHierarchy)
            {
                // 臨時リストがら、クリックした、データ削除
                var gearData = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId);
                this.lockTargetList.Remove(gearData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.gearData.serverId, 0));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId).checkFlg;
                // 仮ロックフラッグチェック更新
                item.SetTemplockImage(checkFlg);
                this.isChangeLockFlg = checkFlg;
            }
            else
            {
                // 臨時リストがら、クリックした、データ削除
                var gearData = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId);
                this.lockTargetList.Remove(gearData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.gearData.serverId, 1));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId).checkFlg;
                item.SetTemplockImage(checkFlg);
                this.isChangeLockFlg = checkFlg;
            }

            // 変更するID取得
            var changeId = UserData.Get().gearData
            .Where(x => x.serverId == item.gearData.serverId && item.gearData.lockFlg != isChangeLockFlg)
            .Select(x => x.serverId)
            .SingleOrDefault();

            // 変更するIDリストに追加
            if(changeId > 0)
            {
                changeValuePartIdList.Add(changeId);
            }

            // 多重クリックで、データ変化がないデータ
            var deleteId = UserData.Get().gearData
            .Where(x => x.serverId == item.gearData.serverId && item.gearData.lockFlg == isChangeLockFlg)
            .Select(x => x.serverId)
            .SingleOrDefault();

            // リストで、変化がないId 削除
            this.changeValuePartIdList.Remove(deleteId);
        }

        /// <summary>
        /// まとめて分解ボタンクリック時
        /// </summary>
        public override void OnClickDecompositionButton()
        {
            // 少ない値から順序別に整列
            this.sortElementDatas = this.elementDatas.OrderBy(x => x.gearId).ToArray();

            // 分解臨時リストセット
            for(int i = 0; i < sortElementDatas.Length; i++)
            {
                var serverId = sortElementDatas[i].serverId;
                this.lockTargetList.Add(new TempFlgData(serverId, 0));
            }

            // アイテムセット
            this.scene.itemScrollView.Initialize(
                this.scene.partsScrollViewItemPrefab.gameObject,
                this.sortElementDatas.Length,
                this.OnUpdateDecompositionGearScrollViewItem
            );

            // ボタンセット
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(true);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(true);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);
            this.scene.lockButton.gameObject.SetActive(false);
            // 拡張ボタンクリック禁止
            this.scene.plusPossessioExpansionButton.interactable = false;
        }

        /// <summary>
        /// まとめて分解アイテムデータロード
        /// </summary>
        private void OnUpdateDecompositionGearScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryGearScrollViewItem>();
            var itemInfo = gobj.GetComponent<CommonIcon>();
            var data = this.sortElementDatas[elementId];            
            var isEquipped = data.partsServerId > 0;
            var checkFlg = this.lockTargetList.Find(x => x.serverId == data.serverId).checkFlg;

            // ギアデータセット
            item.SetCheckBox(
                data: data,
                isEquipped: isEquipped,
                isLock: data.lockFlg,
                checkFlg: checkFlg,
                onClick: OnClickGearDecompositionButton
            );

            // まとめて分解の時アイテムをクリック時選択中パーツの個数表示
            this.scene.selectingCount.text = this.changeValuePartIdList.Count.ToString();
            this.scene.selectingText.text = Masters.LocalizeTextDB.Get("Selecting");

            this.scene.decompositionAllConfirmButton.interactable = this.changeValuePartIdList.Count > 0;

            // 仮ロックフラッグチェック更新
            if(checkFlg > 0)
            {
                item.SetTempCheckImage(checkFlg);
            }
        }

        /// <summary>
        /// まとめて分解の時アイテムをクリック時
        /// </summary>
        private void OnClickGearDecompositionButton(ItemInventoryGearScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);

            // ボタン切り替え
            this.scene.decompositionAllConfirmButton.gameObject.SetActive(true);
            this.scene.decompositionAllCancelButton.gameObject.SetActive(true);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);

            // Api通信前 checkImage切り替え
            if (item.checkImage.gameObject.activeInHierarchy)
            {
                // 臨時リストがら、クリックした、データ削除
                var gearData = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId);
                this.lockTargetList.Remove(gearData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.gearData.serverId, 0));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId).checkFlg;
                // 仮選択フラッグチェック更新
                item.SetTempCheckImage(checkFlg);
                // リストのID削除
                this.changeValuePartIdList.Remove(item.gearData.serverId);
            }
            else
            {
                // 臨時リストがら、クリックした、データ削除
                var gearData = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId);
                this.lockTargetList.Remove(gearData);
                // 臨時リストに、新しいデータ追加
                this.lockTargetList.Add(new TempFlgData(item.gearData.serverId, 1));
                // 臨時リストのcheckFlg取得
                var checkFlg = this.lockTargetList.Find(x => x.serverId == item.gearData.serverId).checkFlg;
                item.SetTempCheckImage(checkFlg);
                // リストにID追加
                this.changeValuePartIdList.Add(item.gearData.serverId);
            }

            // まとめて分解の時アイテムをクリック時選択中パーツの個数表示
            this.scene.selectingCount.text = this.changeValuePartIdList.Count.ToString();
            this.scene.selectingText.text = Masters.LocalizeTextDB.Get("Selecting");

            this.scene.decompositionAllConfirmButton.interactable = this.changeValuePartIdList.Count > 0;
        }

        /// <summary>
        /// ギア枠拡張
        /// </summary>
        public override void OnClickPlusItemPossessionButton()
        {
            uint nextId = Masters.GearExpansionDB.FindById(this.gearUtilityId).nextId;

            if(nextId == 0)
            {
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                dialog.AddText(Masters.LocalizeTextDB.Get("CannonExtendedLimit"));
                var backButton = dialog.AddOKButton();
                backButton.onClick = dialog.Close;
            }
            else
            {
                // ダイアログセット
                var localize = Masters.LocalizeTextDB;
                this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
                this.scene.dialog.closeButtonEnabled = true;
                this.scene.dialog.titleText.text = localize.Get("PlusGearPossession");
                var content = this.scene.dialog.AddContent(this.scene.plusPossessionDialogContentPrefab);
                var okButton = this.scene.dialog.AddOKButton();
                okButton.text.text = Masters.LocalizeTextDB.Get("Expansion");
                okButton.onClick = this.OnClickPlusItemPossession;

                // コンテンツデータロード
                content.Set((uint)ItemType.Gear, this.gearUtilityId, this.scene.dialog, okButton);
            }
        }

        /// <summary>
        /// 拡張ボタンクリック時、API実行
        /// </summary>
        private void OnClickPlusItemPossession()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            UtilityApi.CallExpansionGearApi(this.onCompletedPlusGearPossession);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void onCompletedPlusGearPossession()
        {
            this.Start();
            this.scene.dialog.Close();
        }
    }

    /// <summary>
    /// ギア台座パーツステート
    /// </summary>
    private class GearBatteryItemState : GearChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素(ギアタイプ別)
        /// </summary>
        protected override UserGearData[] elementDatas => UserData.Get().gearData.Where(x => x.gearType == (uint)GearType.Battery).ToArray();

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // ギア所持制限UtilityId取得
            this.gearUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxGear).utilityId;

            if(elementDatas.Length == 0)
            {
                this.scene.itemScrollView.gameObject.SetActive(false);
                // ボタンセット
                this.scene.lockButton.gameObject.SetActive(false);
                this.scene.confirmButton.gameObject.SetActive(false);
                this.scene.decompositionAllConfirmButton.gameObject.SetActive(false);
                this.scene.decompositioncollectButton.gameObject.SetActive(false);
                this.scene.plusPossessioExpansionButton.interactable = true;
                this.scene.selectingCount.text = null;
                this.scene.selectingText.text = null;
                var gearData = UserData.Get().gearData;

                // パーツの所持制限マスターデータ
                var gearMaxCount = Masters.GearExpansionDB.FindById(this.gearUtilityId).maxPossession;
                // 現在所持しているギアの数
                var gearTotalCount = gearData.Length;

                // 所持·所持制限テキストセット
                this.scene.gearPossessionCount.text = Masters.LocalizeTextDB.GetFormat("ItemPossessionCount", gearTotalCount, gearMaxCount);
                this.scene.batteryPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Battery).ToString();
                this.scene.barrelPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Barrel).ToString();
                this.scene.bulletPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Bullet).ToString();
            }
            else
            {
                base.Start();
            }
        }

        // まとめて、ロック確認ボタン(API実行)
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallGearLockList(this.changeValuePartIdList, OnCompletedLockIcon);
        }

        // API通信に成功した場合、コールバック
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<GearBatteryItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Gear, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<GearBatteryItemState>();
        }

        /// <summary>
        /// API まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellGearList(this.changeValuePartIdList, this.OnCompletedGearDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedGearDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<GearBatteryItemState>();
        }
    }

    /// <summary>
    /// ギア砲身パーツステート
    /// </summary>
    private class GearBarrelItemState : GearChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素(ギアタイプ別)
        /// </summary>
        protected override UserGearData[] elementDatas => UserData.Get().gearData.Where(x => x.gearType == (uint)GearType.Barrel).ToArray();

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // ギア所持制限UtilityId取得
            this.gearUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxGear).utilityId;

            if(elementDatas.Length ==0)
            {
                this.scene.itemScrollView.gameObject.SetActive(false);
                
                // ボタンセット
                this.scene.lockButton.gameObject.SetActive(false);
                this.scene.confirmButton.gameObject.SetActive(false);
                this.scene.decompositionAllConfirmButton.gameObject.SetActive(false);
                this.scene.decompositioncollectButton.gameObject.SetActive(false);
                this.scene.plusPossessioExpansionButton.interactable = true;
                this.scene.selectingCount.text = null;
                this.scene.selectingText.text = null;

                var gearData = UserData.Get().gearData;

                // パーツの所持制限マスターデータ
                var gearMaxCount = Masters.GearExpansionDB.FindById(this.gearUtilityId).maxPossession;
                // 現在所持しているギアの数
                var gearTotalCount = gearData.Length;

                // 所持·所持制限テキストセット
                this.scene.gearPossessionCount.text = Masters.LocalizeTextDB.GetFormat("ItemPossessionCount", gearTotalCount, gearMaxCount);
                this.scene.batteryPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Battery).ToString();
                this.scene.barrelPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Barrel).ToString();
                this.scene.bulletPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Bullet).ToString();
            }
            else
            {
                base.Start();
            }
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallGearLockList(this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<GearBarrelItemState>();
        }

        /// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Gear, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<GearBarrelItemState>();
        }

        /// <summary>
        /// API まとめて、分解確認ダイアログで、分解ボタンクリック時
        /// </summary>
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellGearList(this.changeValuePartIdList, this.OnCompletedGearDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedGearDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<GearBarrelItemState>();
        }
    }

    /// <summary>
    /// ギア砲弾パーツステート
    /// </summary>
    private class GearBulletItemState : GearChangeChildState
    {
        /// <summary>
        /// スクロールビュー要素(ギアタイプ別)
        /// </summary>
        protected override UserGearData[] elementDatas => UserData.Get().gearData.Where(x => x.gearType == (uint)GearType.Bullet).ToArray();

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // ギア所持制限UtilityId取得
            this.gearUtilityId = UserData.Get().tUtilityData.First(x => x.utilityType == (uint)UtilityType.MaxGear).utilityId;

            if(elementDatas.Length == 0)
            {
                this.scene.itemScrollView.gameObject.SetActive(false);
                // ボタンセット
                this.scene.lockButton.gameObject.SetActive(false);
                this.scene.confirmButton.gameObject.SetActive(false);
                this.scene.decompositionAllConfirmButton.gameObject.SetActive(false);
                this.scene.decompositioncollectButton.gameObject.SetActive(false);
                this.scene.plusPossessioExpansionButton.interactable = true;
                this.scene.selectingCount.text = null;
                this.scene.selectingText.text = null;

                var gearData = UserData.Get().gearData;

                // パーツの所持制限マスターデータ
                var gearMaxCount = Masters.GearExpansionDB.FindById(this.gearUtilityId).maxPossession;
                // 現在所持しているギアの数
                var gearTotalCount = gearData.Length;

                // 所持·所持制限テキストセット
                this.scene.gearPossessionCount.text = Masters.LocalizeTextDB.GetFormat("ItemPossessionCount", gearTotalCount, gearMaxCount);
                this.scene.batteryPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Battery).ToString();
                this.scene.barrelPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Barrel).ToString();
                this.scene.bulletPossessionCount.text = gearData.Count(x => x.gearType == (uint)GearType.Bullet).ToString();
            }
            else
            {
                // 拡張ボタンクリック禁止
                
                base.Start();
            }
        }

        /// <summary>
        /// まとめて、ロック確認ボタン(API実行)
        /// </summary>
        public override void OnClickCollectConfirmButton()
        {
            TurretApi.CallGearLockList(this.changeValuePartIdList, OnCompletedLockIcon);
        }

        /// <summary>
        /// API通信に成功した場合、コールバック
        /// </summary>
        private void OnCompletedLockIcon()
        {
            // リスト初期化
            this.changeValuePartIdList.Clear();

            // ステージ戻し
            this.manager.ChangeState<GearBulletItemState>();
        }

        //// <summary>
        /// まとめて、分解確認ボタンクリック時ダイアログ
        /// </summary>
        public override void OnClickDecompositionAllConfirmButton()
        {
            // まとめて、分解確認ダイアログ
            var localize = Masters.LocalizeTextDB;
            this.scene.dialog = SharedUI.Instance.ShowSimpleDialog();
            this.scene.dialog.closeButtonEnabled = true;
            this.scene.dialog.titleText.text = localize.Get("AllDecomposition");
            var content = this.scene.dialog.AddContent(this.scene.decompositionAllDialogContentPrefab);
            
            // コンテンツデータロード
            content.Set((uint)ItemType.Gear, this.changeValuePartIdList);

            var okButton = this.scene.dialog.AddOKButton();
            okButton.text.text = Masters.LocalizeTextDB.Get("Decomposition");
            okButton.onClick = this.OnClickDecompositionConfirmButton;
        }

        /// <summary>
        /// まとめて、分解キャンセルボタンクリック時
        /// </summary>
        public override void OnClickDecompositionAllCancelButton()
        {
            this.manager.ChangeState<GearBulletItemState>();
        }

        // API まとめて、分解確認ダイアログで、分解ボタンクリック時
        private void OnClickDecompositionConfirmButton()
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            TurretApi.CallSellGearList(this.changeValuePartIdList, this.OnCompletedGearDecompositionConfirm);
        }

        /// <summary>
        /// 完了時コールバック
        /// </summary>
        private void OnCompletedGearDecompositionConfirm()
        {
            this.changeValuePartIdList.Clear();
            this.scene.dialog.Close();

            this.manager.ChangeState<GearBulletItemState>();
        }
    }

    /// <summary>
    /// その他アイテムステート
    /// </summary>
    private class OtherItemState : MyState
    {
        /// <summary>
        /// スクロールビュー要素
        /// </summary>
        private UserItemData[] elementDatas;
        /// <summary>
        /// スクロールビュー整列値(少ない値段から)
        /// </summary>
        private UserItemData[] sortElementDatas;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            // ボタンセット
            this.scene.lockButton.gameObject.SetActive(false);
            this.scene.decompositioncollectButton.gameObject.SetActive(false);
            // パーツのトグルボタンセットOFF
            this.scene.partsToggle.SetActive(false);
            // 所持量オブジェクトセットOFF
            this.scene.partsPossessionObject.SetActive(false);
            // ItemDataがListなので、配列に変更
            UserItemData[] itemDatas = UserData.Get().itemData.ToArray();
            // スクロールビュー要素
            this.elementDatas = itemDatas;

            // その他アイテムがない場合
            if(elementDatas.Length == 0)
            {
                this.scene.itemScrollView.gameObject.SetActive(false);
            }
            else
            {
                // Item PrefabセットOn
                this.scene.itemScrollView.gameObject.SetActive(true);
                
                this.sortElementDatas = this.elementDatas.OrderBy(x => x.itemId).ToArray();
                
                // スクロールビュー初期化
                this.scene.itemScrollView.Initialize(
                    this.scene.partsScrollViewItemPrefab.gameObject,
                    this.elementDatas.Length,
                    this.OnUpdateOtherItemScrollViewItem
                );
            }
        }

        /// <summary>
        /// スクロールビュー要素表示構築
        /// </summary>
        private void OnUpdateOtherItemScrollViewItem(GameObject gobj, int elementId)
        {
            var item = gobj.GetComponent<ItemInventoryOtherItemScrollViewItem>();
            var data = this.sortElementDatas[elementId];

            item.Set(
                data: data,
                onClick: this.OnClickOtherItemScrollViewItem
            );
        }

        /// <summary>
        /// スクロールビュー要素クリック時
        /// </summary>
        private void OnClickOtherItemScrollViewItem(ItemInventoryOtherItemScrollViewItem item)
        {
            SoundManager.Instance.PlaySe(SeName.YES);

            var localize =  Masters.LocalizeTextDB;
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            dialog.titleText.text = localize.Get("OtherItemDetailTitle");
            var content = dialog.AddContent(this.scene.otherDialogContentPrefab);
            content.SetOtherItemData(
                data: item.itemData
            );
        }

        /// <summary>
        /// 台座ボタンクリック時
        /// </summary>
        public override void OnClickTurretButton()
        {
            this.manager.ChangeState<PartsChangeState>();
            // 台座トグル On
            this.scene.batteryToggle.isOn = true;
            // 装飾トグル SetActive On
            this.scene.decorationToggle.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// ギアボタンクリック時
        /// </summary>
        public override void OnClickGearButton()
        {
            this.manager.ChangeState<GearItemChangeState>();
            // 台座トグル On
            this.scene.batteryToggle.isOn = true;
            // 装飾トグル SetActive Off
            this.scene.decorationToggle.gameObject.SetActive(false);
        }
    }
}

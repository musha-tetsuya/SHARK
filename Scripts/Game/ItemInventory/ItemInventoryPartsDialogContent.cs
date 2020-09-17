using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// アイテムインベントのメインリシーンで、
/// パーツアイテムをクリックした時のポップアップコンテンツ
/// </summary>
public class ItemInventoryPartsDialogContent : MonoBehaviour
{
    [Header("テキスト")]
    /// <summary>
    /// 同一のパーツ数テキスト
    /// </summary>
    [SerializeField]
    private Text overlapCountText = null;
    /// <summary>
    /// 初期砲台削除できない、テキスト
    /// </summary>
    [SerializeField]
    private Text defaultCanonText = null;

    [Header("パーツ")]
    /// <summary>
    /// パーツアイコン
    /// </summary>
    [SerializeField]
    private ItemInventoryPartsScrollViewItem partsCommonIcon = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text partsNameText = null;
    /// <summary>
    /// 説明テキスト
    /// </summary>
    [SerializeField]
    private Text partsDescriptionText = null;

    [Header("FVアタック")]
    /// <summary>
    /// エリアオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject fvAttackArea = null;
    /// <summary>
    /// アイコンイメージ
    /// </summary>
    [SerializeField]
    private Image partsViewFvAttackIconImage = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text partsViewFvAttackNameText = null;
    /// <summary>
    /// 説明テキスト
    /// </summary>
    [SerializeField]
    private Text partsViewFvAttackDescriptionText = null;

    [Header("ゲージ")]
    /// <summary>
    /// ゲージ
    /// </summary>
    [SerializeField]
    private GameObject gaugeArea = null;
    [SerializeField]
    private CommonStatusGauge powerGauge = null;
    [SerializeField]
    private CommonStatusGauge bulletSpeedGauge = null;
    [SerializeField]
    private CommonStatusGauge fvPointGetCalueGauge = null;
    
    [Header("ギアスロット")]
    /// <summary>
    /// ギアスロットエリア
    /// </summary>
    [SerializeField]
    private GameObject gearsArea = null;
    /// <summary>
    /// ギアスロットオブジェクト配列
    /// </summary>
    [SerializeField]
    private CustomTurretGearSlotButton[] gearSlotObject = null;
    /// <summary>
    /// ギア装着ポップアップオブジェクト
    /// </summary>
    [SerializeField]
    private CustomGearEquipPopup gearEquipPopupObject = null;

    [Header("分解")]
    /// <summary>
    /// 分解リストスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView decompositionScrollView = null;
    /// <summary>
    /// 分解時、修得アイテムアイコン
    /// </summary>
    [SerializeField]
    private ItemInventoryDecompositionScrollViewItem decompositionItemPrefab = null;
    /// <summary>
    /// 分解ボタングレイアウト
    /// </summary>
    [SerializeField]
    private Graphic grayoutButton = null;
    /// <summary>
    /// 分解ボタン
    /// </summary>
    [SerializeField]
    private Button decompositionButton = null;

    [Header("ロック")]
    /// <summary>
    /// ロックされている場合
    /// </summary>
    [SerializeField]
    private GameObject lockOn = null;
    /// <summary>
    /// ロックされていない場合
    /// </summary>
    [SerializeField]
    private GameObject lockOff = null;
    /// <summary>
    /// パーツ別、情報
    /// </summary>
    private PartsInfoBase partsInfo = null;
    /// <summary>
    /// 装着中情報
    /// </summary>
    private bool isEquipped = false;
    /// <summary>
    /// 終了時コールバック
    /// </summary>
    private Action onReflesh = null;
    /// <summary>
    /// 終了時コールバック
    /// </summary>
    private Action onRefleshDecomposition = null;
    
    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;

    private Master.ItemSellData[] getItem = null;

    /// <summary>
    /// ギアがロックの場合
    /// </summary>
    private uint isGearLockCount = 0;

    /// <summary>
    /// パーツデータ
    /// </summary>
    public UserPartsData partsData { get; private set; }

    /// <summary>
    /// パーツ情報セット
    /// </summary>
    public void SetPartsData(UserPartsData data,　bool isEquipped,  Action onReflesh, Action onRefleshDecomposition)
    {
        this.partsData = data;
        this.onReflesh = onReflesh;
        this.onRefleshDecomposition = onRefleshDecomposition;
        this.isEquipped = isEquipped;

        // パーツアイコンセット
        partsCommonIcon.Set(
            data: data,
            isEquipped: isEquipped,
            isLock: data.lockFlg,
            onClick: null
        );
        
        // パーツが台座の場合
        if (this.partsData.itemType == (uint)ItemType.Battery)
        {
            partsInfo = new BatteryPartsInfo(data);
            
            // 台座FVアタックスプライトセット
            this.partsViewFvAttackIconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)partsInfo.fvType)).asset as Sprite;
            // 台座FVアタック情報セット
            this.partsViewFvAttackNameText.text = partsInfo.fvName;
            this.partsViewFvAttackDescriptionText.text = partsInfo.fvDescription;
        }
        // パーツが砲身の場合
        else if (this.partsData.itemType == (uint)ItemType.Barrel)
        {
            partsInfo = new BarrelPartsInfo(data);
            this.fvAttackArea.SetActive(false);
        }
        // パーツが砲弾の場合
        else if (this.partsData.itemType == (uint)ItemType.Bullet)
        {
            partsInfo = new BulletPartsInfo(data);
            this.fvAttackArea.SetActive(false);
        }
        // パーツが装飾の場合
        else if (this.partsData.itemType == (uint)ItemType.Accessory)
        {
            partsInfo = new DecorationPartsInfo(data);
        }

        // 同一のパーツ数テキスト
        this.overlapCountText.text = partsInfo.partsCount;

        // パーツ情報セット
        this.partsNameText.text = partsInfo.name;
        this.partsDescriptionText.text = partsInfo.description;

        // ゲージ
        var config = Masters.ConfigDB.FindById(1);
        var gearDatas = this.partsData.gearMasterIds.Select(gearId => Masters.GearDB.FindById(gearId)).ToArray();
        // ギアスロットの全てのギアを能力値別に、合算
        var gearPower = gearDatas.Select(x => (long)x.power).Sum();
        var gearSpeed = gearDatas.Select(x => (long)x.speed).Sum();
        var gearFvPoint = gearDatas.Select(x => (long)x.fvPoint).Sum();
        // パーツはギア能力値を合算
        uint power = partsInfo.power + (uint)gearPower;
        uint speed = partsInfo.speed + (uint)gearSpeed;
        uint fvPoint = partsInfo.fvPoint + (uint)gearFvPoint;
        
        Debug.LogFormat("power : {0}, bulletSpeed : {1}, fvPointGetValue{2} : {2}", power, speed, fvPoint);

        // ゲージ初期化
        float gaugePower = 0f;
        float gaugeSpeed = 0f;
        float gaugeFvPoint = 0f;

        if (this.partsData.itemType == (uint)ItemType.Accessory)
        {
            this.fvAttackArea.SetActive(false);
            this.gaugeArea.SetActive(false);
            this.gearsArea.SetActive(false);
        }
        else
        {
            // パーツが砲弾の場合
            if (this.partsData.itemType == (uint)ItemType.Bullet)
            {
                if (power < config.maxBulletPower / 5)
                    gaugePower = 0.2f;
                else
                    gaugePower = (float)power / config.maxBulletPower;

                gaugeSpeed = (float)speed / config.maxBarrelSpeed;
                gaugeFvPoint = (float)fvPoint / config.maxBatteryFvPoint;
            }
            // パーツが砲身の場合
            else if (this.partsData.itemType == (uint)ItemType.Barrel)
            {
                if (speed < config.maxBarrelSpeed / 5)
                    gaugeSpeed = 0.2f;
                else
                    gaugeSpeed = (float)speed / config.maxBarrelSpeed;

                gaugePower = (float)power / config.maxBulletPower;
                gaugeFvPoint = (float)fvPoint / config.maxBatteryFvPoint;
            }
            // パーツが台座の場合
            else if (this.partsData.itemType == (uint)ItemType.Battery)
            {
                if (speed < config.maxBatteryFvPoint / 5)
                    gaugeFvPoint = 0.2f;
                else
                    gaugeFvPoint = (float)fvPoint / config.maxBatteryFvPoint;

                gaugePower = (float)power / config.maxBulletPower;
                gaugeSpeed = (float)speed / config.maxBarrelSpeed;
            }

            // ゲージセット
            this.powerGauge.SetGaugeValue(Mathf.Clamp01(gaugePower));
            this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01(gaugeSpeed));
            this.fvPointGetCalueGauge.SetGaugeValue(Mathf.Clamp01(gaugeFvPoint));

            // ギアスロットセット
            this.GearSlotReflesh(data);
        }

        // ロックチェック
        if(data.lockFlg == 1)
        {
            this.lockOn.gameObject.SetActive(true);
            this.lockOff.gameObject.SetActive(false);
        }
        else
        {
            this.lockOn.gameObject.SetActive(false);
            this.lockOff.gameObject.SetActive(true);
        }

        // 分解詩修得アイテムマスターID取得(ギアがない場合)
        if (gearDatas.Length == 0)
        {
            this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == partsInfo.itemSellId).ToArray();

            // 分解詩修得アイテムスクロールビュー
            this.decompositionScrollView.Initialize(
                this.decompositionItemPrefab.gameObject,
                this.getItem.Length,
                this.OnUpdateDecompositionScrollViewItem
            );
        }
        // 分解詩修得アイテムマスターID取得(ギアがある場合)
        else
        {
            // パーツの分解時アイテム取得
            this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == partsInfo.itemSellId).ToArray();
            // パーツの分解時取得アイテムをリストの変更
            List<Master.ItemSellData> sellDatas = getItem.ToList();

            for(int i = 0; i < gearDatas.Length; i++)
            {
                // ギアの分解時アイテム取得
                Master.ItemSellData[] getGearItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == gearDatas[i].itemSellId).ToArray();

                // getItemに追加
                for(int y = 0; y < getGearItem.Length; y++)
                {
                    sellDatas.Add(getGearItem[y]);
                }
            }

            // リスト配列に変更
            this.getItem = sellDatas.ToArray();

            // アイコンセット
            this.decompositionScrollView.Initialize(
                this.decompositionItemPrefab.gameObject,
                getItem.Length,
                this.OnUpdateDecompositionScrollViewItem
            );
        }

        // ボタングレーアウトON/OFF
        if(this.partsData.lockFlg == 1 || this.partsData.useFlg || this.isGearLockCount > 0 || partsInfo.itemSellId == 0)
        {
            this.SetGrayout(true);
            this.decompositionButton.interactable = false;
        }
        else
        {
            this.SetGrayout(false);
            this.decompositionButton.interactable = true;
        }

        // 初期砲台の場合、分解不可能の案内メッセージ、テキストセット
        if (partsInfo.itemSellId == 0)
        {
            this.defaultCanonText.text = Masters.LocalizeTextDB.Get("CannotDisassembledDefaultCanon");
        }
        else
        {
            this.defaultCanonText.text = null;
        }
    }

    /// <summary>
    /// ボタングレーアウトON/OFF
    /// </summary>
    private void SetGrayout(bool isGrayout)
    {
        this.grayoutButton.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
    }

    /// <summary>
    /// 分解詩修得アイテムスクロールビューアイテムセット
    /// </summary>
    private void OnUpdateDecompositionScrollViewItem(GameObject gobj, int elementId)
    {
        var item = gobj.GetComponent<ItemInventoryDecompositionScrollViewItem>();
        item.Set(getItem[elementId].itemType, getItem[elementId].itemId, getItem[elementId].itemNum);
    }

    /// <summary>
    /// ギアスロットセット
    /// </summary>
    private void GearSlotReflesh(UserPartsData data)
    {
        this.isGearLockCount = 0;
        // パーツ別に、情報ロード
        PartsInfoBase partsInfo = null;
        // パーツが台座の場合
        if (data.itemType == (uint)ItemType.Battery)
        {
            partsInfo = new BatteryPartsInfo(data);
        }
        // パーツが砲身の場合
        else if (data.itemType == (uint)ItemType.Barrel)
        {
            partsInfo = new BarrelPartsInfo(data);
        }
        // パーツが台座の場合
        else if (data.itemType == (uint)ItemType.Bullet)
        {
            partsInfo = new BulletPartsInfo(data);
        }

        //装着中ギアを時間順にソート
        var gears = UserData.Get().gearData
            .Where(x => x.partsServerId == data.serverId && x.gearType == (uint)partsInfo.gearType)
            .OrderBy(x => x.setDateTime)
            .ToArray();

        // ギアID取得
        for (int i = 0; i < SharkDefine.MAX_GEAR_SLOT_SIZE; i++)
        {
            uint gearId = 0;
            if (i < gears.Length)
            {
                gearId = gears[i].gearId;

                // 装着中ギアがロックされてたら、カウンター
                if (gears[i].lockFlg > 0)
                {
                    this.isGearLockCount ++;
                }
            }

            // ギア取付可能なスロット
            bool isExtended = i < partsInfo.defaultGearSlotSize + data.gearSlotExpandCount;

            // ギアスロットボタンセット
            this.gearSlotObject[i].SetUp(data, gearId, isExtended, this.OnClickGearSlotButton);
        }
    }

    /// <summary>
    /// ギアスロットボタンクリック時
    /// </summary>
    private void OnClickGearSlotButton(CustomTurretGearSlotButton slot)
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        //拡張済みスロットの場合
        if (slot.isExtended)
        {
            //ギア一覧ダイアログ開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            var content = dialog.AddContent(this.gearEquipPopupObject);
            content.Setup(
                dialog: dialog,
                partsData: slot.partsData,
                gearId: slot.gearId,
                onReflesh: () =>
                {
                    // 終了時コールバック
                    this.onReflesh?.Invoke();
                    this.SetPartsData(slot.partsData, slot.partsData.useFlg, this.onReflesh, this.onRefleshDecomposition);
                });
        }
        //未拡張スロットの場合
        else
        {
            //TODO:スロット拡張
        }
    }

    /// <summary>
    /// ロックイメージクリック時にAPI実行
    /// </summary>
    public void OnClickLockIcon()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        TurretApi.CallCannonLock(this.partsData, OnCompletedLockIcon);
    }

    /// <summary>
    /// ロックAPI成功時のコールバック
    /// </summary>
    private void OnCompletedLockIcon()
    {
        // パーツアイコンセット
        partsCommonIcon.Set(
            data: this.partsData,
            isEquipped: this.isEquipped,
            isLock: this.partsData.lockFlg,
            onClick: null
        );

        // ロックチェック
        // ボタングレーアウトON/OFF
        if(this.partsData.lockFlg == 1 || this.partsData.useFlg || this.isGearLockCount > 0 || partsInfo.itemSellId == 0)
        {
            this.SetGrayout(true);
            this.decompositionButton.interactable = false;
        }
        else
        {
            this.SetGrayout(false);
            this.decompositionButton.interactable = true;
        }

        if(this.partsData.lockFlg == 1)
        {
            this.lockOn.gameObject.SetActive(true);
            this.lockOff.gameObject.SetActive(false);
        }
        else
        {
            this.lockOn.gameObject.SetActive(false);
            this.lockOff.gameObject.SetActive(true);
        }

        // インベントリシーンのアイテム更新
        this.onReflesh?.Invoke();
    }

    /// <summary>
    /// パーツ分解ボタンのポップアップ
    /// </summary>
    public void OnClickDecompositionButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        if (this.partsData.lockFlg == 0)
        {
            if (this.partsData.useFlg)
            {
                // ボタンクリック不可能
            }
            else
            {
                this.dialog = SharedUI.Instance.ShowSimpleDialog();
                var yesNo = dialog.SetAsYesNoMessageDialog(
                    Masters.LocalizeTextDB.Get("DecompositionConfirm")
                );

                yesNo.yesNo.yes.onClick = OnClickDecompositionConfirmYesButton;
                yesNo.yesNo.no.onClick = OnClickDecompositionConfirmNoButton;
            }

        }
        else
        {
            // ボタンクリック不可能
        }
    }

    private void OnClickDecompositionConfirmNoButton()
    {
        SoundManager.Instance.PlaySe(SeName.NO);
        this.dialog.Close();
    }

    /// <summary>
    /// パーツ分解API実行
    /// </summary>
    private void OnClickDecompositionConfirmYesButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        // パーツタイプ
        var itemType = this.partsData.itemType;
        // パーツサーバーID
        var itemId = this.partsData.serverId;

        // API実行
        TurretApi.CallSellCannon(itemType, itemId, this.OnCompletedCallSellCannon);
    }

    /// <summary>
    /// API成功時のコールバック
    /// </summary>
    private void OnCompletedCallSellCannon()
    {
        this.dialog.Close();
        this.onRefleshDecomposition?.Invoke();
    }

    /// <summary>
    /// パーツ情報ベース
    /// </summary>
    private abstract class PartsInfoBase
    {
        public virtual GearType gearType { get; }
        public virtual string name => null;
        public virtual string description => null;
        public virtual uint fvAttackId => 0;
        public virtual uint fvType => 0;
        public virtual string fvName => null;
        public virtual string fvDescription => null;
        public virtual uint power => 0;
        public virtual uint speed => 0;
        public virtual uint fvPoint => 0;
        public virtual string partsCount => null;
        public virtual uint defaultGearSlotSize => 0;
        public virtual uint itemSellId => 0;
 
        protected PartsInfoBase(UserPartsData partsData)
        {

        }
    }

    private class BatteryPartsInfo : PartsInfoBase
    {
        private Master.BatteryData master = null;
        private Master.FvAttackData FvMaster = null;
        private int count;

        public override GearType gearType => GearType.Battery;
        public override string name => this.master.name;
        public override string description => this.master.description;
        public override uint fvAttackId => this.master.fvAttackId;
        public override uint fvType => this.FvMaster.type;
        public override string fvName => this.FvMaster.name;
        public override string fvDescription => this.FvMaster.description;
        public override uint fvPoint => this.master.fvPoint;
        public override string partsCount => Masters.LocalizeTextDB.GetFormat("BatteryPartsOverlapCount", this.count - 1);
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;
        public override uint itemSellId => this.master.itemSellId;

        public BatteryPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BatteryDB.FindById(partsData.itemId);
            this.FvMaster = Masters.FvAttackDB.FindById(master.fvAttackId);

            // 現在パーツと所有するパーツを比較の上、同数
            uint[] batteryIds = UserData.Get().batteryData.Select(x => x.itemId).ToArray();
            for (int i = 0; i < batteryIds.Length; i++)
            {
                var userPartsIds = batteryIds[i].ToString();
                var currentPartsId = partsData.itemId.ToString();

                if (currentPartsId == userPartsIds)
                {
                    this.count++;
                }
            }
        }
    }

    private class BarrelPartsInfo : PartsInfoBase
    {
        private Master.BarrelData master = null;
        private int count;

        public override GearType gearType => GearType.Barrel;
        public override string name => this.master.name;
        public override string description => this.master.description;
        public override uint speed => this.master.speed;
        public override string partsCount => Masters.LocalizeTextDB.GetFormat("BarrelPartsOverlapCount",this.count - 1);
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;
        public override uint itemSellId => this.master.itemSellId;

        public BarrelPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BarrelDB.FindById(partsData.itemId);

            // 現在パーツと所有するパーツを比較の上、同数
            uint[] barrelIds = UserData.Get().barrelData.Select(x => x.itemId).ToArray();
            for (int i = 0; i < barrelIds.Length; i++)
            {
                var userPartsIds = barrelIds[i].ToString();
                var currentPartIds = partsData.itemId.ToString();

                if(currentPartIds == userPartsIds)
                {
                    this.count++;
                }
            }
        }
    }

    private class BulletPartsInfo : PartsInfoBase
    {
        private Master.BulletData master = null;
        private int count;

        public override GearType gearType => GearType.Bullet;
        public override string name => this.master.name;
        public override string description => this.master.description;
        public override uint power => this.master.power;
        public override string partsCount => Masters.LocalizeTextDB.GetFormat("BulletPartsOverlapCount", count - 1);
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;
        public override uint itemSellId => this.master.itemSellId;

        public BulletPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BulletDB.FindById(partsData.itemId);

            // 現在パーツと所有するパーツを比較の上、同数
            uint[] bulletIds = UserData.Get().bulletData.Select(x => x.itemId).ToArray();
            for(int i = 0; i < bulletIds.Length; i++)
            {
                var userPartsIds = bulletIds[i].ToString();
                var currentPartsId = partsData.itemId.ToString();

                if(currentPartsId == userPartsIds)
                {
                    this.count++;
                }
            }
        }
    }

    private class DecorationPartsInfo : PartsInfoBase
    {
        private Master.AccessoriesData master = null;
        private int count;

        public override string name => this.master.name;
        public override string description => this.master.info;
        public override string partsCount => Masters.LocalizeTextDB.GetFormat("DecorationPartsOverlapCount", this.count -1);
        public override uint itemSellId => master.itemSellId;

        public DecorationPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.AccessoriesDB.FindById(partsData.itemId);

            // 現在パーツと所有するパーツを比較の上、同数
            uint[] DecorationIds = UserData.Get().accessoriesData.Select(x => x.itemId).ToArray();
            for(int i = 0; i < DecorationIds.Length; i++)
            {
                var userPartsIds = DecorationIds[i].ToString();
                var currentPartIds = partsData.itemId.ToString();

                if (currentPartIds == userPartsIds)
                {
                    this.count++;
                }
            }

        }
    }

}

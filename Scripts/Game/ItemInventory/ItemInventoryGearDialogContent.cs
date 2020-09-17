using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ItemInventoryGearDialogContent : MonoBehaviour
{
    [Header("テキスト")]
    /// <summary>
    /// 同一のパーツ数テキスト
    /// </summary>
    [SerializeField]
    private Text overlapCountText = null;
    /// <summary>
    /// 必要コインテキストエリア
    /// </summary>
    [SerializeField]
    private GameObject needCoinArea = null;
    /// <summary>
    /// ギア外すテキスト
    /// </summary>
    [SerializeField]
    private Text unEquipGearText = null;

    [Header("ギア")]
    /// <summary>
    /// ギアアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// ギアアイコン
    /// </summary>
    [SerializeField]
    private ItemInventoryGearScrollViewItem gearCommonIcon = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text gearNameText = null;
    /// <summary>
    /// 説明テキスト
    /// </summary>
    [SerializeField]
    private Text gearDescriptionText = null;

    [Header("パーツ")]
    [SerializeField]
    private CommonIcon partsCommonIcon = null;
    /// <summary>
    /// 装着中パンネル
    /// </summary>
    [SerializeField]
    private GameObject partsEquippedMark = null;
  
    [Header("ゲージ")]
    /// <summary>
    /// ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge powerGauge = null;
    [SerializeField]
    private CommonStatusGauge bulletSpeedGauge = null;
    [SerializeField]
    private CommonStatusGauge fvPointGetCalueGauge = null;

    [Header("ボタン")]
    [SerializeField]
    private Button lockbutton = null;
    [SerializeField]
    private Button decompositionButton = null;

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

    [Header("ポップアップ")]
    [SerializeField]
    private CustomGearConfirmDialogContent confirmDialogContentPrefab = null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;

    private Master.ItemSellData[] getItem = null;

    // 終了時コールバック
    private Action onReflesh = null;
    //終了時コールバック
    private Action onRefleshDecomposition = null;

    public UserGearData gearData { get; private set; }

    /// <summary>
    /// ギア情報セット
    /// </summary>
    public void SetGearData(UserGearData data, Action onReflesh, Action onRefleshDecomposition)
    {
        this.gearData = data;
        this.onReflesh = onReflesh;
        this.onRefleshDecomposition = onRefleshDecomposition;

        // CommonIconをギアに変更
        this.commonIcon.SetGearCommonIcon(true);
        // ギアアイコン情報セット
        this.gearCommonIcon.Set(
            data: data,
            isEquipped: data.partsServerId > 0,
            isLock: data.lockFlg,
            onClick: null
        );

        // マスターデータ
        var gearMaster = Masters.GearDB.FindById(data.gearId);

        //現在ギアと所有するギアを比較の上、同数
        uint[] gearIds = UserData.Get().gearData.Select(x => x.gearId).ToArray();
        int count = 0;
        for (int i = 0; i < gearIds.Length; i++)
        {
            var userGearIds = gearIds[i].ToString();
            var currentGearIds = data.gearId.ToString();

            if (currentGearIds == userGearIds)
            {
                count++;
            }
        }

        // 同一のギア数テキスト
        this.overlapCountText.text = Masters.LocalizeTextDB.GetFormat("GearOverlapCount", count - 1);

        // 名前
        this.gearNameText.text = gearMaster.name;
        // 説明
        this.gearDescriptionText.text = gearMaster.description;

        // ゲージ、ギアを能力値
        var config = Masters.ConfigDB.FindById(1);
        var power = gearMaster.power;
        var speed = gearMaster.speed;
        var fvPoint = gearMaster.fvPoint;

        // ゲージセット
        this.powerGauge.SetGaugeValue(Mathf.Clamp01((float)power / config.maxGearPower));
        this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01((float)speed / config.maxGearSpeed));
        this.fvPointGetCalueGauge.SetGaugeValue(Mathf.Clamp01((float)fvPoint / config.maxGearFvPoint));

        // パーツ別に、情報ロード
        PartsInfoBase partsInfo = null;

        if (this.gearData.gearType == (uint)GearType.Battery)
        {
            partsInfo = new BatteryPartsInfo(data);
        }
        else if (this.gearData.gearType == (uint)GearType.Barrel)
        {
            partsInfo = new BarrelPartsInfo(data);
        }
        else if (this.gearData.gearType == (uint)GearType.Bullet)
        {
            partsInfo = new BulletPartsInfo(data);
        }

        if (data.partsServerId == null)
        {
            this.partsCommonIcon.gameObject.SetActive(false);
            this.needCoinArea.gameObject.SetActive(false);
            this.lockbutton.gameObject.SetActive(false);
            this.decompositionButton.gameObject.SetActive(true);
            this.lockbutton.gameObject.SetActive(false);
        }
        else
        {
            // パーツアイコン
            this.partsCommonIcon.countText.text = null;
            this.partsCommonIcon.gearArea.gameObject.SetActive(true);
            this.decompositionButton.gameObject.SetActive(false);
            this.lockbutton.gameObject.SetActive(true);

            // CommonIconパーツスプライトセット
            string Key = CommonIconUtility.GetSpriteKey((uint)partsInfo.itemType, partsInfo.partsMasterId);
            string Path = CommonIconUtility.GetSpritePath((uint)partsInfo.itemType, partsInfo.partsMasterId);

            var partsHandle = AssetManager.FindHandle<Sprite>(Path);
            this.partsCommonIcon.SetIconSprite(partsHandle.asset as Sprite);

            // CommonIconパーツランキングセット
            var partsRank = CommonIconUtility.GetRarity((uint)partsInfo.itemType, partsInfo.partsMasterId);
            this.partsCommonIcon.SetRank(partsRank);

            // CommonIconパーツ装着中ギアセット
            var partsGearSize = partsInfo.partsGearSize;

            for (int i = 0; i < this.partsCommonIcon.gearIcon.Length; i++)
            {
                this.partsCommonIcon.gearIcon[i].enabled = i < partsInfo.partsGearSlotCount;
                if(i < partsGearSize)
                {
                    this.partsCommonIcon.gearOnIcon[i].gameObject.SetActive(true);
                }
            }

            // 装着中表示
            this.partsEquippedMark.SetActive(partsInfo.partsIsEquiped);
            // 外すテキスト
            this.unEquipGearText.text = Masters.LocalizeTextDB.GetFormat("unitNeedCoin", Masters.GearDB.FindById(gearData.gearId).rejectCoin);
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

        // 分解詩修得アイテムマスターID取得
        this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == Masters.GearDB.FindById(data.gearId).itemSellId).ToArray();

        // 分解詩修得アイテムスクロールビュー
        this.decompositionScrollView.Initialize(
            this.decompositionItemPrefab.gameObject,
            this.getItem.Length,
            this.OnUpdateDecompositionScrollViewItem
        );

        // ボタングレーアウトON/OFF
        if(this.gearData.lockFlg == 1)
        {
            this.SetGrayout(true);
            this.decompositionButton.interactable = false;
        }
        else
        {
            this.SetGrayout(false);
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
        item.Set(this.getItem[elementId].itemType, this.getItem[elementId].itemId, this.getItem[elementId].itemNum);
    }

    /// <summary>
    /// ロックイメージクリック時にAPI実行
    /// </summary>
    public void OnClickLockIcon()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        TurretApi.CallGearLock(this.gearData, OnCompletedLockIcon);
    }

    /// <summary>
    /// ロックAPI成功時のコールバック
    /// </summary>
    private void OnCompletedLockIcon()
    {
        // ギアアイコン情報セット
        this.gearCommonIcon.Set(
            data: this.gearData,
            isEquipped: gearData.partsServerId > 0,
            isLock: gearData.lockFlg,
            onClick: null
        );

         // ロックチェック
        if(this.gearData.lockFlg == 1)
        {
            this.lockOn.gameObject.SetActive(true);
            this.lockOff.gameObject.SetActive(false);
            this.SetGrayout(true);
            this.decompositionButton.interactable = false;
        }
        else
        {
            this.lockOn.gameObject.SetActive(false);
            this.lockOff.gameObject.SetActive(true);
            this.SetGrayout(false);
            this.decompositionButton.interactable = true;
        }

        this.onReflesh?.Invoke();
    }

    /// <summary>
    /// ギアを外すボタン
    /// </summary>
    public void OnClickUnEquipGearButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.confirmDialogContentPrefab);
        content.Setup(
            dialog: dialog,
            partsData: null,
            beforeGear: this.gearData,
            afterGear: null,
            onReflesh: () =>
            {
                this.onReflesh?.Invoke();
                SetGearData(this.gearData, this.onReflesh, this.onRefleshDecomposition);
            },
            onCancel: () =>
            {
                
            });
    }

    /// <summary>
    /// ギア分解ボタンのポップアップ
    /// </summary>
    public void OnClickDecompositionButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        if (this.gearData.lockFlg == 0)
        {
            if (this.gearData.partsServerId > 0)
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

    /// <summary>
    /// キャンセルボタンをクリック時
    /// </summary>
    private void OnClickDecompositionConfirmNoButton()
    {
        SoundManager.Instance.PlaySe(SeName.NO);
        this.dialog.Close();
    }

    /// <summary>
    /// CallSellGear API実行
    /// </summary>
    private void OnClickDecompositionConfirmYesButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        // ギアID
        var gearId = this.gearData.serverId;

        // API実行
        TurretApi.CallSellGear(gearId, this.OnCompletedCallSellGear);
    }

    /// <summary>
    /// 成功時コールバック
    /// </summary>
    private void OnCompletedCallSellGear()
    {
        this.dialog.Close();
        this.onRefleshDecomposition?.Invoke();
    }

    /// <summary>
    /// パーツ情報ベース
    /// </summary>
    private abstract class PartsInfoBase
    {
        public virtual ItemType itemType { get; }
        public virtual uint partsMasterId => 0;
        public virtual uint partsGearSlotExpandCount => 0;
        public virtual uint partsGearSize => 0;
        public virtual uint partsGearSlotCount => 0;
        public virtual bool partsIsEquiped => false;

        protected PartsInfoBase(UserGearData data)
        {
            
        }
    }

    /// <summary>
    /// 台座情報
    /// </summary>
    private class BatteryPartsInfo : PartsInfoBase
    {
        private uint batteryMasterId;
        private uint batteryServerId;
        private uint batteryslotExpandCount;
        private uint batteryGearSize;
        private uint batteryGearSlotCount;

        public override ItemType itemType => ItemType.Battery;
        public override uint partsMasterId => this.batteryMasterId;
        public override uint partsGearSlotExpandCount => this.batteryslotExpandCount;
        public override uint partsGearSize => this.batteryGearSize;
        public override uint partsGearSlotCount => this.batteryGearSlotCount;
        public override bool partsIsEquiped => UserData.Get().turretData.Select(x => x.batteryServerId == this.batteryServerId).First();

        public BatteryPartsInfo(UserGearData data) : base(data)
        {
            if (data.partsServerId == null)
            {

            }
            else
            {
                // パーツデータ取得
                UserPartsData partsData = UserData.Get().batteryData
                .Where(x => x.serverId == data.partsServerId)
                .First();

                // 本、ギアを装着中のパーツのマスターID
                this.batteryMasterId = partsData.itemId;

                // 本、ギアを装着中のパーツのサーバーID
                this.batteryServerId = partsData.serverId;

                // ギアスロット拡張回数
                this.batteryslotExpandCount = partsData.gearSlotExpandCount;

                // ギアのマスターデータID
                uint[] batteryGearMasterIds = partsData.gearMasterIds;
                
                // パーツのギア情報
                this.batteryGearSize = (uint)batteryGearMasterIds.Length;
                var batteryMaster = Masters.BatteryDB.FindById(this.batteryMasterId);
                uint defaultCount = batteryMaster.defaultGearSlotSize;
                this.batteryGearSlotCount = defaultCount + batteryslotExpandCount;
            }
        }
    }

    /// <summary>
    /// 砲身情報
    /// </summary>
    private class BarrelPartsInfo : PartsInfoBase
    {
        private uint barrelMasterId;
        private uint barrelServerId;
        private uint barrelSlotExpandCount;
        private uint barrelGearSize;
        private uint barrelGearSlotCount;

        public override ItemType itemType => ItemType.Barrel;
        public override uint partsMasterId => this.barrelMasterId;
        public override uint partsGearSlotExpandCount => this.barrelSlotExpandCount;
        public override uint partsGearSize => this.barrelGearSize;
        public override uint partsGearSlotCount => this.barrelGearSlotCount;
        public override bool partsIsEquiped => UserData.Get().turretData.Select(x => x.barrelServerId == this.barrelServerId).First();

        public BarrelPartsInfo(UserGearData data) : base(data)
        {
            if (data.partsServerId == null)
            {
                
            }
            else
            {
                // パーツデータ取得
                UserPartsData partsData = UserData.Get().barrelData
                .Where(x => x.serverId == data.partsServerId)
                .First();

                // 本、ギアを装着中のパーツのマスターID
                this.barrelMasterId = partsData.itemId;

                // 本、ギアを装着中のパーツのサーバーID
                this.barrelServerId = partsData.serverId;

                // ギアスロット拡張回数
                this.barrelSlotExpandCount = partsData.gearSlotExpandCount;

                // ギアのマスターデータID
                uint[] barrelGearMasterIds = partsData.gearMasterIds;

                // パーツのギア情報
                this.barrelGearSize = (uint)barrelGearMasterIds.Length;
                var barrelMaster = Masters.BarrelDB.FindById(this.barrelMasterId);
                uint defaultCount = barrelMaster.defaultGearSlotSize;
                this.barrelGearSlotCount = defaultCount + barrelSlotExpandCount;
            }
        }

    }

    /// <summary>
    /// 砲弾情報
    /// </summary>
    private class BulletPartsInfo : PartsInfoBase
    {
        private uint bulletMasterId;
        private uint bulletServerId;
        private uint bulletSlotExpandCount;
        private uint bulletGearSize;
        private uint bulletGearSlotCount;

        public override ItemType itemType => ItemType.Bullet;
        public override uint partsMasterId => this.bulletMasterId;
        public override uint partsGearSlotExpandCount => this.bulletSlotExpandCount;
        public override uint partsGearSize => this.bulletGearSize;
        public override uint partsGearSlotCount => this.bulletGearSlotCount;
        public override bool partsIsEquiped => UserData.Get().turretData.Select(x => x.bulletServerId == this.bulletServerId).First();

        public BulletPartsInfo(UserGearData data) : base(data)
        {
            if(data.partsServerId == null)
            {

            }
            else
            {
                // パーツデータ取得
                UserPartsData partsData = UserData.Get().bulletData
                .Where(x => x.serverId == data.partsServerId)
                .First();

                // 本、ギアを装着中のパーツのマスターID
                this.bulletMasterId = partsData.itemId;

                // 本、ギアを装着中のパーツのサーバーID
                this.bulletServerId = partsData.serverId;

                // ギアスロット拡張回数
                this.bulletSlotExpandCount = partsData.gearSlotExpandCount;

                // ギアのマスターデータID
                uint[] bulletGearMasterIds = partsData.gearMasterIds;

                this.bulletGearSize = (uint)bulletGearMasterIds.Length;
                var bulletMaster = Masters.BulletDB.FindById(this.bulletMasterId);

                uint defaultCount = bulletMaster.defaultGearSlotSize;
                this.bulletGearSlotCount = defaultCount + bulletSlotExpandCount;
            }
        }
    }
}

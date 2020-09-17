using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// ギア情報ページ
/// </summary>
[Serializable]
public class CustomTurretGearView
{
    /// <summary>
    /// ギアビューエリア
    /// </summary>
    [Header("Parts Icon")]
    [SerializeField]
    private GameObject gearView = null;
    // パーツの種類
    [SerializeField]
    private Text titleNameText = null;
    /// <summary>
    /// パーツアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon partsIcon = null;
    /// <summary>
    /// パーツ名テキスト
    /// </summary>
    [SerializeField]
    private Text partsNameText = null;
    /// <summary>
    /// パーツ情報テキスト
    /// </summary>
    [SerializeField]
    private Text partsInfoText = null;

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

    /// <summary>
    /// ゲージ
    /// </summary>
    [Header("Status Area")]
    [SerializeField]
    private CommonStatusGauge powerGauge = null;
    [SerializeField]
    private CommonStatusGauge bulletSpeedGauge = null;
    [SerializeField]
    private CommonStatusGauge fvPointGetValueGauge = null;
    
    /// <summary>
    /// ゲージに適用する能力値
    /// </summary>
    private uint fvPointGetValue;
    private uint bulletSpeed;
    private uint power;

    private uint gearFvPointGetValue;
    private uint gearBulletSpeed;
    private uint gearPower;

    /// <summary>
    /// ギア Infoセット
    /// </summary>
    public void Reflesh(UserPartsData data)
    {
        PartsInfoBase partsInfo = null;

        if (data.itemType == (uint)ItemType.Battery)
        {
            partsInfo = new BatteryPartsInfo(data);
        }
        else if (data.itemType == (uint)ItemType.Barrel)
        {
            partsInfo = new BarrelPartsInfo(data);
        }
        else if (data.itemType == (uint)ItemType.Bullet)
        {
            partsInfo = new BulletPartsInfo(data);
        }

        // ゲージ最大値設定
        var config = Masters.ConfigDB.FindById(1);
        var MAX_POWER = config.maxBulletPower;
        var MAX_BULLET_SPEED = config.maxBarrelSpeed;
        var MAX_FV_POINT_GET_VALUE = config.maxBatteryFvPoint;

        // パーツ別各能力
        this.fvPointGetValue = partsInfo.fvPoint;
        this.bulletSpeed = partsInfo.speed;
        this.power = partsInfo.power;

        // ギア別各能力
        this.gearFvPointGetValue = partsInfo.gearFvPointGetValue;
        this.gearBulletSpeed = partsInfo.gearBulletSpeed;
        this.gearPower = partsInfo.gearPower;

        // Sprite, CommonIcon セット
        var iconSprite = AssetManager.FindHandle<Sprite>(partsInfo.spritePath).asset as Sprite;
        this.partsIcon.SetIconSprite(iconSprite);
        this.partsIcon.SetRank((Rank)partsInfo.rarity);
        this.partsIcon.SetGearSlot(data);

        // 名前、説明
        this.titleNameText.text = partsInfo.titleName;
        this.partsNameText.text = partsInfo.name;
        this.partsInfoText.text = partsInfo.description;

        // FvPゲージ設定
        if (data.itemType == (uint)ItemType.Battery)
        {
            float f;
            if(this.fvPointGetValue + this.gearFvPointGetValue < MAX_FV_POINT_GET_VALUE / 5)
            {
                f = 0.2f;
            }
            else
            {
                f = (float)(this.fvPointGetValue + this.gearFvPointGetValue) / MAX_FV_POINT_GET_VALUE;
            }
            //float f = (float)((this.fvPointGetValue + this.gearFvPointGetValue) - CustomTurretScene.minBatteryFvPoint) / (MAX_FV_POINT_GET_VALUE - CustomTurretScene.minBatteryFvPoint) + 0.2f;
            this.fvPointGetValueGauge.SetGaugeValue(Mathf.Clamp01(f));
            this.powerGauge.SetGaugeValue(Mathf.Clamp01((float)(this.power + this.gearPower) / MAX_POWER));
            this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01((float)(this.bulletSpeed + this.gearBulletSpeed) / MAX_BULLET_SPEED));
        }
        // 発射速度ゲージ設定
        else if (data.itemType == (uint)ItemType.Barrel)
        {
            float s;
            if(this.bulletSpeed + this.gearBulletSpeed < MAX_BULLET_SPEED / 5)
            {
                s = 0.2f;
            }
            else
            {
                s = (float)(this.bulletSpeed + this.gearBulletSpeed) / MAX_BULLET_SPEED;
            }
            //float s = (float)((this.bulletSpeed + this.gearBulletSpeed) - CustomTurretScene.minBarrelSpeed) / (MAX_BULLET_SPEED - CustomTurretScene.minBarrelSpeed) + 0.2f;
            this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01(s));
            this.powerGauge.SetGaugeValue(Mathf.Clamp01((float)(this.power + this.gearPower) / MAX_POWER));
            this.fvPointGetValueGauge.SetGaugeValue(Mathf.Clamp01((float)(this.fvPointGetValue + this.gearFvPointGetValue) / MAX_FV_POINT_GET_VALUE));
        }
        // 攻撃力ゲージ設定
        else if (data.itemType == (uint)ItemType.Bullet)
        {
            float p;
            if(this.power + this.gearPower < MAX_POWER / 5)
            {
                p = 0.2f;
            }
            else
            {
                p = (float)(this.power + this.gearPower) / MAX_POWER;
            }
            //float p = (float)((this.power + this.gearPower) - CustomTurretScene.minBulletPower) / (MAX_POWER - CustomTurretScene.minBulletPower) + 0.2f;
            this.powerGauge.SetGaugeValue(Mathf.Clamp01(p));
            this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01((float)(this.bulletSpeed + this.gearBulletSpeed) / MAX_BULLET_SPEED));
            this.fvPointGetValueGauge.SetGaugeValue(Mathf.Clamp01((float)(this.fvPointGetValue + this.gearFvPointGetValue) / MAX_FV_POINT_GET_VALUE));
        }
    }

    /// <summary>
    /// ギアスロットセット
    /// </summary>
    public void GearSlotReflesh(UserPartsData data)
    {
        PartsInfoBase partsInfo = null;
        
        if (data.itemType == (uint)ItemType.Battery)
        {
            partsInfo = new BatteryPartsInfo(data);
        }
        else if (data.itemType == (uint)ItemType.Barrel)
        {
            partsInfo = new BarrelPartsInfo(data);
        }
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
                    this.Reflesh(slot.partsData);
                    this.GearSlotReflesh(slot.partsData);
                });
        }
        //未拡張スロットの場合
        else
        {
            //TODO:スロット拡張
        }
    }

    /// <summary>
    /// ギア情報ビューCallBack
    /// </summary>
    public void SetGearView(bool gearView)
    {
        this.gearView.SetActive(gearView);
    }

    /// <summary>
    /// パーツ情報ベース
    /// </summary>
    private abstract class PartsInfoBase
    {
        public abstract GearType gearType { get; }
        public virtual uint power => 0;
        public virtual uint speed => 0;
        public virtual uint fvPoint => 0;
        public virtual string spritePath => null;
        public virtual uint rarity => 0;
        public virtual uint defaultGearSlotSize => 0;
        public virtual string name => null;
        public virtual string titleName => null;
        public virtual string description => null;
        public uint gearFvPointGetValue = 0;
        public uint gearBulletSpeed = 0;
        public uint gearPower = 0;

        protected PartsInfoBase(UserPartsData partsData)
        {
            
        }
    }

    private class BatteryPartsInfo : PartsInfoBase
    {
        private Master.BatteryData master = null;
        private Master.GearData gearMaster = null;

        public override GearType gearType => GearType.Battery;
        public override string name => this.master.name;
        public override string description => this.master.description;
        public override uint fvPoint => this.master.fvPoint;
        public override string spritePath => SharkDefine.GetBatterySpritePath(this.master.key);
        public override string titleName => Masters.LocalizeTextDB.GetFormat("Battery");
        public override uint rarity => this.master.rarity;
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;

        public BatteryPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BatteryDB.FindById(partsData.itemId);
            
            var gearCount = partsData.gearMasterIds.Length;

            // 装着中のギアの能力値
            for(int i = 0; i < gearCount; i++)
            {
                this.gearMaster = Masters.GearDB.FindById(partsData.gearMasterIds[i]);

                this.gearFvPointGetValue += this.gearMaster.fvPoint;
                this.gearBulletSpeed += this.gearMaster.speed;
                this.gearPower += this.gearMaster.power;
            }
        }
    }

    private class BarrelPartsInfo : PartsInfoBase
    {
        private Master.BarrelData master = null;
        private Master.GearData gearMaster = null;

        public override GearType gearType => GearType.Barrel;
        public override string name => this.master.name;
        public override string titleName => Masters.LocalizeTextDB.GetFormat("Barrel");
        public override string description => this.master.description;
        public override uint speed => this.master.speed;
        public override string spritePath => SharkDefine.GetBarrelSpritePath(this.master.key);
        public override uint rarity => this.master.rarity;
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;

        public BarrelPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BarrelDB.FindById(partsData.itemId);

            var gearCount = partsData.gearMasterIds.Length;

            // 装着中のギアの能力値
            for(int i = 0; i < gearCount; i++)
            {
                this.gearMaster = Masters.GearDB.FindById(partsData.gearMasterIds[i]);

                this.gearFvPointGetValue += this.gearMaster.fvPoint;
                this.gearBulletSpeed += this.gearMaster.speed;
                this.gearPower += this.gearMaster.power;
            }
        }
    }

    private class BulletPartsInfo : PartsInfoBase
    {
        private Master.BulletData master = null;
        private Master.GearData gearMaster = null;

        public override GearType gearType => GearType.Bullet;
        public override string name => this.master.name;
        public override string titleName => Masters.LocalizeTextDB.GetFormat("Bullet");
        public override string description => this.master.description;
        public override uint power => this.master.power;
        public override string spritePath => SharkDefine.GetBulletThumbnailPath(this.master.key);
        public override uint rarity => this.master.rarity;
        public override uint defaultGearSlotSize => this.master.defaultGearSlotSize;

        public BulletPartsInfo(UserPartsData partsData) : base(partsData)
        {
            this.master = Masters.BulletDB.FindById(partsData.itemId);

            var gearCount = partsData.gearMasterIds.Length;

            // 装着中のギアの能力値
            for(int i = 0; i < gearCount; i++)
            {
                this.gearMaster = Masters.GearDB.FindById(partsData.gearMasterIds[i]);

                this.gearFvPointGetValue =+ this.gearMaster.fvPoint;
                this.gearBulletSpeed += this.gearMaster.speed;
                this.gearPower += this.gearMaster.power;
            }

        }
    }
}

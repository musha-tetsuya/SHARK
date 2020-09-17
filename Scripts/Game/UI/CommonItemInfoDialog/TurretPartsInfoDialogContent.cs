using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台パーツ詳細ダイアログ内容
/// </summary>
public class TurretPartsInfoDialogContent : CommonItemInfoDialogContentBase
{
    /// <summary>
    /// パーツ名
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// パーツ詳細説明
    /// </summary>
    [SerializeField]
    private Text infoText = null;

    /// <summary>
    /// 能力値の種類
    /// </summary>
    [SerializeField]
    private Text statsText = null;
    /// <summary>
    /// ゲージ
    /// </summary>
    [SerializeField]
    private GameObject statsGaugeObj = null;
    /// <summary>
    /// 能力値ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge statsGauge = null;

    /// <summary>
    /// 特殊能力オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject specialAbilityObject = null;
    /// <summary>
    /// 特殊能力オブジェクトテキスト
    /// </summary>
    [SerializeField]
    private Text specialAbilityTitleText = null;
    /// <summary>
    /// 特殊能力名テキスト
    /// </summary>
    [SerializeField]
    private Text specialAbilityNameText = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public override void Setup(IItemInfo itemInfo)
    {
        //ゲージ最大値設定
        var config = Masters.ConfigDB.FindById(1);
        var MAX_POWER = config.maxBulletPower;
        var MAX_BULLET_SPEED = config.maxBarrelSpeed;
        var MAX_FV_POINT_GET_VALUE = config.maxBatteryFvPoint;

        var localizeText = Masters.LocalizeTextDB;

        //砲座
        if (itemInfo is Master.BatteryData)
        {
            var batteryData = itemInfo as Master.BatteryData;
            var fvAttackData = Masters.FvAttackDB.FindById(batteryData.fvAttackId);
            uint fvPointGetValue = batteryData.fvPoint;
            
            this.nameText.text = batteryData.name;
            this.infoText.text = batteryData.description;

            this.statsText.text = localizeText.GetFormat("FvPointGetValue");
            
            //FVポイント獲得値ゲージ設定
            this.statsGauge.SetGaugeValue(Mathf.Clamp01((float)fvPointGetValue / MAX_FV_POINT_GET_VALUE));

            //FVアタック
            this.specialAbilityTitleText.text = localizeText.GetFormat("FvAttack");
            this.specialAbilityNameText.text = fvAttackData.name;
        }
        // 砲身
        else if (itemInfo is Master.BarrelData)
        {
            var barrelData = itemInfo as Master.BarrelData;
            uint bulletSpeed = barrelData.speed;
            
            this.nameText.text = barrelData.name;
            this.infoText.text = barrelData.description;

            this.statsText.text = localizeText.Get("BulletSpeed");

            //発射速度ゲージ設定
            this.statsGauge.SetGaugeValue(Mathf.Clamp01((float)bulletSpeed / MAX_BULLET_SPEED));
            
            this.specialAbilityObject.SetActive(false);
        }
        // 弾丸
        else if (itemInfo is Master.BulletData)
        {
            var bulletData = itemInfo as Master.BulletData;

            uint power = bulletData.power;

            this.nameText.text = bulletData.name;
            this.infoText.text = bulletData.description;

            this.statsText.text = localizeText.Get("Power");

            //攻撃力ゲージ設定
            this.statsGauge.SetGaugeValue(Mathf.Clamp01((float)power / MAX_POWER));

            //スキル
            this.specialAbilityTitleText.text = localizeText.GetFormat("BasicSkill");
            this.specialAbilityNameText.text = localizeText.GetFormat("DebugTest");
        }
        // アクセサリー
        else if (itemInfo is Master.AccessoriesData)
        {
            var accessoryData = itemInfo as Master.AccessoriesData;

            this.nameText.text = accessoryData.name;
            this.infoText.text = accessoryData.info;
            
            this.statsText.text = null;
            this.statsGaugeObj.SetActive(false);
            this.specialAbilityObject.SetActive(false);
        }
        //ギア
        else if (itemInfo is Master.GearData)
        {
            var gearData = itemInfo as Master.GearData;

            this.nameText.text = gearData.name;
            this.infoText.text = gearData.description;

            this.statsText.text = null;
            this.statsGaugeObj.SetActive(false);
            this.specialAbilityObject.SetActive(false);
        }
    }

    /// <summary>
    /// 内容構築
    /// </summary>
    public override void Setup(ProductBase product)
    {
        var itemInfo = CommonIconUtility.GetItemInfo(product.addItems[0].itemType, product.addItems[0].itemId);
        this.Setup(itemInfo);
    }
}

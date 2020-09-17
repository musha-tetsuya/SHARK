using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// パーツスクロールビュー要素
/// </summary>
public class CustomGearScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// ギアの名前
    /// </summary>
    [SerializeField]
    private Text gearName = null;
    /// <summary>
    /// ギアの説明
    /// </summary>
    [SerializeField]
    private Text gearDescription = null;
    /// <summary>
    /// 外すボタン
    /// </summary>
    [SerializeField]
    private GameObject removeViewObject = null;
    /// <summary>
    /// 未装着パンネル
    /// </summary>
    [SerializeField]
    private GameObject notEquippedObject = null;

    /// <summary>
    /// 攻撃力ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge powerGauge = null;
    /// <summary>
    /// 発射速度ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge bulletSpeedGauge = null;
    /// <summary>
    /// FVポイント獲得値ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge fvPointGetValueGauge = null;

    [SerializeField]
    private Text powerGaugeText = null;

    [SerializeField]
    private Text bulletSpeedGaugeText = null;

    [SerializeField]
    private Text fvPointGetValueGaugeText = null;

    /// <summary>
    /// ユーザーギアデータ
    /// </summary>
    public UserGearData gearData { get; private set; }

    /// <summary>
    /// ギアボタン
    /// </summary>
    private Action<CustomGearScrollViewItem> onClick;

    /// <summary>
    /// このPrefab Itemをクリックした時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
    }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void SetGearData(UserGearData data, Action<CustomGearScrollViewItem> onClick)
    {
        this.gearData = data;
        this.onClick = onClick;
        var localizeText = Masters.LocalizeTextDB;

        if (data == null)
        {
            this.removeViewObject.SetActive(true);
        }
        else
        {
            var config = Masters.ConfigDB.FindById(1);
            //各能力のMAX値（仮）
            var MAX_POWER = config.maxGearPower;
            var MAX_BULLET_SPEED = config.maxGearSpeed;
            var MAX_FV_POINT_GET_VALUE = config.maxGearFvPoint;

            // 歯車能力値
            var master = Masters.GearDB.FindById(this.gearData.gearId);
            uint power =  master.power;
            uint bulletSpeed = master.speed;
            uint fvPointGetValue = master.fvPoint;
            
            //攻撃力ゲージ設定
            this.powerGauge.SetGaugeValue(Mathf.Clamp01((float)power / MAX_POWER));
            
            //発射速度ゲージ設定
            this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01((float)bulletSpeed / MAX_BULLET_SPEED));
            
            //FVポイント獲得値ゲージ設定
            this.fvPointGetValueGauge.SetGaugeValue(Mathf.Clamp01((float)fvPointGetValue / MAX_FV_POINT_GET_VALUE));

            //ランクスプライト
            this.commonIcon.SetRank((Rank)master.rarity);

            // CommonIconをギアに変更
            this.commonIcon.SetGearCommonIcon(true);

            // ギアCommonIconセット
            var bgSprite = CommonIconUtility.GetGearBgSprite(this.gearData.gearType);
            var mainSprite = CommonIconUtility.GetGearMainImageSprite(master.key);
            var subSprite = CommonIconUtility.GetGearSubImageSprite(master.subKey);
            this.commonIcon.SetGearSprite(bgSprite, mainSprite, subSprite);

            //ギアInfo
            this.gearName.text = master.name;
            this.gearDescription.text = master.description;

            // localize
            this.powerGaugeText.text = localizeText.Get("Power");
            this.bulletSpeedGaugeText.text = localizeText.Get("BulletSpeed");
            this.fvPointGetValueGaugeText.text = localizeText.GetFormat("FvPointGetValue");
        }
    }

    /// <summary>
    /// 固定されている装着ギアパネルが未装着の場合
    /// </summary>
    public void SetNotEquippedPanel()
    {
        this.notEquippedObject.SetActive(true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台情報
/// </summary>
[Serializable]
public class CustomTurretInfoView
{
    /// <summary>
    /// パーツステージの時、砲台情報
    /// </summary>
    [SerializeField]
    public GameObject partsView = null;
    /// <summary>
    /// 砲台ステージの時、砲台情報
    /// </summary>
    [SerializeField]
    public GameObject turretView = null;

    [SerializeField]
    public GameObject statusArea = null;

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
    public GameObject partsStatusArea = null;

    [SerializeField]
    private CommonStatusGauge partsPowerGauge = null;

    [SerializeField]
    private CommonStatusGauge partsBulletSpeedGauge = null;

    [SerializeField]
    private CommonStatusGauge partsfvPointGetCalueGauge = null;

    /// <summary>
    /// FVアタックアイコンイメージ
    /// </summary>
    [Header("Parts View FvAttack")]
    [SerializeField]
    private Image partsViewFvAttackIconImage = null;
    /// <summary>
    /// FVアタック名テキスト
    /// </summary>
    [SerializeField]
    private Text partsViewFvAttackNameText = null;
    /// <summary>
    /// FVアタック説明文
    /// </summary>
    [SerializeField]
    private Text partsViewFvAttackDescriptionText = null;

    /// <summary>
    /// セットスキルアイコンイメージ
    /// </summary>
    [Header("Parts View SetSkill")]
    [SerializeField]
    private Image partsViewSetSkillIconImage = null;
    /// <summary>
    /// セットスキル名テキスト
    /// </summary>
    [SerializeField]
    private Text partsViewSetSkillNameText = null;
    /// <summary>
    /// セットスキル説明文
    /// </summary>
    [SerializeField]
    private Text partsViewSetSkillDescriptionText = null;

    /// <summary>
    /// 台座アイコン
    /// </summary>
    [Header("Parts View Parts Icon")]
    [SerializeField]
    private CommonIcon partsViewBatteryIcon = null;
    /// <summary>
    /// 砲身アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon partsViewBarrelIcon = null;
    /// <summary>
    /// 砲弾アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon partsViewBulletIcon = null;
    /// <summary>
    /// アクセサリアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon partsViewAccessoryIcon = null;

    /// <summary>
    /// FVアタックアイコンイメージ
    /// </summary>
    [Header("Turret View FvAttack")]
    [SerializeField]
    private Image turretViewFvAttackIconImage = null;
    /// <summary>
    /// FVアタック名テキスト
    /// </summary>
    [SerializeField]
    private Text turretViewFvAttackNameText = null;
    /// <summary>
    /// FVアタック説明文
    /// </summary>
    [SerializeField]
    private Text turretViewFvAttackDescriptionText = null;

    /// <summary>
    /// セットスキルアイコンイメージ
    /// </summary>
    [Header("Turret View SetSkill")]
    [SerializeField]
    private Image turretViewSetSkillIconImage = null;
    /// <summary>
    /// セットスキル名テキスト
    /// </summary>
    [SerializeField]
    private Text turretViewSetSkillNameText = null;
    /// <summary>
    /// セットスキル説明文
    /// </summary>
    [SerializeField]
    private Text turretViewSetSkillDescriptionText = null;

    /// <summary>
    /// 台座アイコン
    /// </summary>
    [Header("Turret View Parts Icon")]
    [SerializeField]
    private CommonIcon turretViewBatteryIcon = null;
    /// <summary>
    /// 砲身アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon turretViewBarrelIcon = null;
    /// <summary>
    /// 砲弾アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon turretViewBulletIcon = null;
    /// <summary>
    /// アクセサリアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon turretViewAccessoryIcon = null;

    /// <summary>
    /// 表示更新
    /// </summary>
    public void Reflesh(UserTurretData data)
    {
        //ゲージ最大値設定
        var config = Masters.ConfigDB.FindById(1);
        var MAX_POWER = config.maxBulletPower;
        var MAX_BULLET_SPEED = config.maxBarrelSpeed;
        var MAX_FV_POINT_GET_VALUE = config.maxBatteryFvPoint;

        // マスターデータ
        var batteryData = Masters.BatteryDB.FindById(data.batteryMasterId);
        var barrelData = Masters.BarrelDB.FindById(data.barrelMasterId);
        var bulletData = Masters.BulletDB.FindById(data.bulletMasterId);
        var accessoryData = Masters.AccessoriesDB.FindById(data.accessoryMasterId);
        var fvAttackData = Masters.FvAttackDB.FindById(batteryData.fvAttackId);

        // 現在のパーツ
        var batteryPartsData = UserData.Get().batteryData.First(x => x.serverId == data.batteryServerId);
        var barrelPartsData = UserData.Get().barrelData.First(x => x.serverId == data.barrelServerId);
        var bulletPartsData = UserData.Get().bulletData.First(x => x.serverId == data.bulletServerId);

        // 装着中のギアを配列に合併
        var gearDatas = batteryPartsData.gearMasterIds
            .Concat(barrelPartsData.gearMasterIds)
            .Concat(bulletPartsData.gearMasterIds)
            .Select(gearId => Masters.GearDB.FindById(gearId))
            .ToArray();
        
        // 能力値別に合算
        var gearPower = gearDatas.Select(x => (long)x.power).Sum();
        var gearSpeed = gearDatas.Select(x => (long)x.speed).Sum();
        var gearFvPoint = gearDatas.Select(x => (long)x.fvPoint).Sum();

        // パーツ能力値デとギア能力値を合併
        uint power = bulletData.power + (uint)gearPower;
        uint bulletSpeed = barrelData.speed + (uint)gearSpeed;
        uint fvPointGetValue = batteryData.fvPoint + (uint)gearFvPoint;
        Debug.LogFormat("power : {0}, bulletSpeed : {1}, fvPointGetValue{2} : {2}", power, bulletSpeed, fvPointGetValue);

        var fvAttackIconSprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvAttackData.type)).asset as Sprite;
        var batterySprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetBatterySpritePath(batteryData.key)).asset as Sprite;
        var barrelSprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetBarrelSpritePath(barrelData.key)).asset as Sprite;
        var bulletSprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetBulletThumbnailPath(bulletData.key)).asset as Sprite;
        var accessorySprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetAccessoryThumbnailPath(accessoryData.key)).asset as Sprite;

        //攻撃力ゲージ設定
        //float p = (float)(power - CustomTurretScene.minBulletPower) / (MAX_POWER - CustomTurretScene.minBulletPower) + 0.2f;
        float p;
        if(power < MAX_POWER / 5)
        p = 0.2f;
        else
        p = (float)power / MAX_POWER;
        this.powerGauge.SetGaugeValue(Mathf.Clamp01(p));
        this.partsPowerGauge.SetGaugeValue(Mathf.Clamp01(p));

        //発射速度ゲージ設定
        //p = (float)(bulletSpeed - CustomTurretScene.minBarrelSpeed) / (MAX_BULLET_SPEED - CustomTurretScene.minBarrelSpeed) + 0.2f;
        if(bulletSpeed < MAX_BULLET_SPEED / 5)
        p = 0.2f;
        else
        p = (float)bulletSpeed / MAX_BULLET_SPEED;
        this.bulletSpeedGauge.SetGaugeValue(Mathf.Clamp01(p));
        this.partsBulletSpeedGauge.SetGaugeValue(Mathf.Clamp01(p));

        //FVポイント獲得値ゲージ設定
        //p = (float)(fvPointGetValue - CustomTurretScene.minBatteryFvPoint) / (MAX_FV_POINT_GET_VALUE - CustomTurretScene.minBatteryFvPoint) + 0.2f;
        if(fvPointGetValue < MAX_FV_POINT_GET_VALUE / 5)
        p = 0.2f;
        else
        p = (float)fvPointGetValue / MAX_FV_POINT_GET_VALUE;
        this.fvPointGetValueGauge.SetGaugeValue(Mathf.Clamp01(p));
        this.partsfvPointGetCalueGauge.SetGaugeValue(Mathf.Clamp01(p));

        //パーツページのFVアタック名、アイコン画像、説明文設定
        this.partsViewFvAttackNameText.text = fvAttackData.name;
        this.partsViewFvAttackIconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvAttackData.type)).asset as Sprite;
        this.partsViewFvAttackDescriptionText.text = fvAttackData.description;

        //砲台ページのFVアタック名、アイコン画像、説明文設定
        this.turretViewFvAttackNameText.text = fvAttackData.name;
        this.turretViewFvAttackIconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvAttackData.type)).asset as Sprite;
        this.turretViewFvAttackDescriptionText.text = fvAttackData.description;

        //セットスキル発動
        if (batteryData.seriesId == barrelData.seriesId && batteryData.seriesId == bulletData.seriesId)
        {
            var seriesData = Masters.TurretSerieseDB.FindById(batteryData.seriesId);
            var seriesSkillData = Masters.SerieseSkillDB.FindById(seriesData.seriesSkillId);

            //パーツページのセットスキル名、説明文、アイコン画像設定
            this.partsViewSetSkillIconImage.enabled = true;
            this.partsViewSetSkillIconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetSeriesSkillIconSpritePath(seriesSkillData.key)).asset as Sprite;
            this.partsViewSetSkillNameText.text = seriesSkillData.name;
            this.partsViewSetSkillDescriptionText.text = seriesSkillData.description;

            //砲台ページのセットスキル名、説明文、アイコン画像設定
            this.turretViewSetSkillIconImage.enabled = true;
            this.turretViewSetSkillIconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetSeriesSkillIconSpritePath(seriesSkillData.key)).asset as Sprite;
            this.turretViewSetSkillNameText.text = seriesSkillData.name;
            this.turretViewSetSkillDescriptionText.text = seriesSkillData.description;
        }
        //セットスキル不発
        else
        {
            //パーツページのセットスキル名、説明文、アイコン画像非表示
            this.partsViewSetSkillIconImage.enabled = false;
            this.partsViewSetSkillNameText.text = null;
            this.partsViewSetSkillDescriptionText.text = null;

            //砲台ページのセットスキル名、説明文、アイコン画像非表示
            this.turretViewSetSkillIconImage.enabled = false;
            this.turretViewSetSkillNameText.text = null;
            this.turretViewSetSkillDescriptionText.text = null;
        }

        //パーツページ台座アイコン
        this.partsViewBatteryIcon.SetIconSprite(batterySprite);
        this.partsViewBatteryIcon.SetRank((Rank)batteryData.rarity);
        this.partsViewBatteryIcon.SetGearSlot(batteryPartsData);

        //パーツページ砲身アイコン
        this.partsViewBarrelIcon.SetIconSprite(barrelSprite);
        this.partsViewBarrelIcon.SetRank((Rank)barrelData.rarity);
        this.partsViewBarrelIcon.SetGearSlot(barrelPartsData);

        //パーツページ砲弾アイコン
        this.partsViewBulletIcon.SetIconSprite(bulletSprite);
        this.partsViewBulletIcon.SetRank((Rank)bulletData.rarity);
        this.partsViewBulletIcon.SetGearSlot(bulletPartsData);

        //パーツページアクセサリアイコン
        this.partsViewAccessoryIcon.SetIconSprite(accessorySprite);
        this.partsViewAccessoryIcon.SetRank((Rank)accessoryData.rarity);

        //砲台ページ台座アイコン
        this.turretViewBatteryIcon.SetIconSprite(batterySprite);
        this.turretViewBatteryIcon.SetRank((Rank)batteryData.rarity);
        this.turretViewBatteryIcon.SetGearSlot(batteryPartsData);

        //砲台ページ砲身アイコン
        this.turretViewBarrelIcon.SetIconSprite(barrelSprite);
        this.turretViewBarrelIcon.SetRank((Rank)barrelData.rarity);
        this.turretViewBarrelIcon.SetGearSlot(barrelPartsData);

        //砲台ページ砲弾アイコン
        this.turretViewBulletIcon.SetIconSprite(bulletSprite);
        this.turretViewBulletIcon.SetRank((Rank)bulletData.rarity);
        this.turretViewBulletIcon.SetGearSlot(bulletPartsData);

        //砲台ページアクセサリアイコン
        this.turretViewAccessoryIcon.SetIconSprite(accessorySprite);
        this.turretViewAccessoryIcon.SetRank((Rank)accessoryData.rarity);
    }

    /// <summary>
    /// 砲台情報Area切り替え
    /// </summary>
    public void SetTurretInfo(bool turretView, bool partsView, bool statusArea)
    {
        this.turretView.SetActive(turretView);
        this.partsView.SetActive(partsView);
        this.statusArea.SetActive(statusArea);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Master;

/// <summary>
/// マスターデータ管理
/// </summary>
public static class Masters
{
    /// <summary>
    /// ミリ秒を秒にする係数
    /// </summary>
    public const float MilliSecToSecond = 0.001f;
    /// <summary>
    /// パーセントを小数にする係数
    /// </summary>
    public const float PercentToDecimal = 0.01f;

    public static readonly DataBase<BillingData> BillingDB = new DataBase<BillingData>("mBilling");
    public static readonly DataBase<BillingGroupData> BillingGroupDB = new DataBase<BillingGroupData>("mBillingGroup");
    public static readonly DataBase<BillingItemData> BillingItemDB = new DataBase<BillingItemData>("mBillingItem");
    public static readonly DataBase<AccessoriesData> AccessoriesDB = new DataBase<AccessoriesData>("mCannonAccessories"); 
    public static readonly DataBase<BarrelData> BarrelDB = new DataBase<BarrelData>("mCannonBarrel");
    public static readonly DataBase<BatteryData> BatteryDB = new DataBase<BatteryData>("mCannonBattery");
    public static readonly DataBase<BulletData> BulletDB = new DataBase<BulletData>("mCannonBullet");
    public static readonly DataBase<FvAttackData> FvAttackDB = new DataBase<FvAttackData>("mCannonFever");
    public static readonly DataBase<TurretSerieseData> TurretSerieseDB = new DataBase<TurretSerieseData>("mCannonSeries");
    public static readonly DataBase<CannonSetData> CannonSetDB = new DataBase<CannonSetData>("mCannonSet");
    public static readonly ConfigDataBase ConfigDB = new ConfigDataBase("mConfig");
    public static readonly DataBase<FishData> FishDB = new DataBase<FishData>("mFish");
    public static readonly DataBase<FishCaptureData> FishCaptureDB = new DataBase<FishCaptureData>("mFishCapture");
    public static readonly DataBase<FishCategoryData> FishCategoryDB = new DataBase<FishCategoryData>("mFishCategory");
    public static readonly DataBase<FishParticleData> FishParticleDB = new DataBase<FishParticleData>("mFishParticle");
    public static readonly DataBase<GearData> GearDB = new DataBase<GearData>("mGear");
    public static readonly DataBase<BattleItemData> BattleItemDB = new DataBase<BattleItemData>("mItem");
    public static readonly DataBase<ItemSellData> ItemSellDB = new DataBase<ItemSellData>("mItemSell");
    public static readonly DataBase<LevelData> LevelDB = new DataBase<LevelData>("mLevel");
    public static readonly DataBase<BetData> BetDB = new DataBase<BetData>("mLevelBet");
    public static readonly LocalizeTextDataBase LocalizeTextDB = new LocalizeTextDataBase("LocalizeTextData");
    public static readonly DataBase<LoginBonusData> LoginBonusDB = new DataBase<LoginBonusData>("mLoginBonus");
    public static readonly DataBase<LoginBonusSpecialData> LoginBonusSpecialDB = new DataBase<LoginBonusSpecialData>("mLoginBonusSpecial");
    public static readonly DataBase<PartsExpansionData> PartsExpansionDB = new DataBase<PartsExpansionData>("mMaxCannonPossession");
    public static readonly DataBase<CannonExpansionData> CannonExpansionDB = new DataBase<CannonExpansionData>("mMaxCannonSetting");
    public static readonly DataBase<GearExpansionData> GearExpansionDB = new DataBase<GearExpansionData>("mMaxGearPossession");
    public static readonly DataBase<MessageData> MessageDB = new DataBase<MessageData>("mMessage");
    public static readonly DataBase<MissionData> MissionDB = new DataBase<MissionData>("mMission");
    public static readonly DataBase<MissionTypeData> MissionTypeDB = new DataBase<MissionTypeData>("mMissionType");
    public static readonly DataBase<MissionRewardData> MissionRewardDB = new DataBase<MissionRewardData>("mMissionReward");
    public static readonly DataBase<MultiWorldData> MultiWorldDB = new DataBase<MultiWorldData>("mMultiWorld");
    public static readonly DataBase<MultiBallDropRateData> MultiBallDropRateDB = new DataBase<MultiBallDropRateData>("mMultiBallDropRate");
    public static readonly DataBase<MultiSoulDropRateData> MultiSoulDropRateDB = new DataBase<MultiSoulDropRateData>("mMultiSoulDropRate");
    public static readonly DataBase<MultiStageFishData> MultiStageFishDB = new DataBase<MultiStageFishData>("mMultiFish");
    public static readonly DataBase<SeriesSkillData> SerieseSkillDB = new DataBase<SeriesSkillData>("mSeriesSkill");
    public static readonly DataBase<ShopData> ShopDB = new DataBase<ShopData>("mShop");
    public static readonly DataBase<ShopGroupData> ShopGroupDB = new DataBase<ShopGroupData>("mShopGroup");
    public static readonly DataBase<ShopItemData> ShopItemDB = new DataBase<ShopItemData>("mShopItem");
    public static readonly DataBase<SingleStageData> SingleStageDB = new DataBase<SingleStageData>("mSingleStage");
    public static readonly DataBase<SingleStageFishData> SingleStageFishDB = new DataBase<SingleStageFishData>("mSingleStageFish");
    public static readonly DataBase<SingleStageFirstRewardData> SingleStageFirstRewardDB = new DataBase<SingleStageFirstRewardData>("mSingleStageRewardFirstGroup");
    public static readonly DataBase<SingleStageRewardData> SingleStageRewardDB = new DataBase<SingleStageRewardData>("mSingleStageRewardGroup");
    public static readonly DataBase<SingleStageRewardLotData> SingleStageRewardLotDB = new DataBase<SingleStageRewardLotData>("mSingleStageRewardLotGroup");
    public static readonly DataBase<SingleWorldData> SingleWorldDB = new DataBase<SingleWorldData>("mSingleWorld");
    public static readonly DataBase<SkillData> SkillDB = new DataBase<SkillData>("mSkill");
    public static readonly DataBase<SkillGroupData> SkillGroupDB = new DataBase<SkillGroupData>("mSkillGroup");
    public static readonly DataBase<VipBenefitData> VipBenefitDB = new DataBase<VipBenefitData>("mVipBenefit");
    public static readonly DataBase<VipBenefitTypeData> VipBenefitTypeDB = new DataBase<VipBenefitTypeData>("mVipBenefitType");
    public static readonly DataBase<VipLevelData> VipLevelDB = new DataBase<VipLevelData>("mVipLevel");
    public static readonly DataBase<VipRewardData> VipRewardDB = new DataBase<VipRewardData>("mVipReward");
}

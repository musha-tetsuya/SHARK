using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle {

/// <summary>
/// バトル用ユーザーデータ
/// </summary>
public abstract class BattleUserData
{
    /// <summary>
    /// FVポイント
    /// </summary>
    public long fvPoint = 0;
    /// <summary>
    /// 砲台データ
    /// </summary>
    public UserTurretData turretData = null;
    /// <summary>
    /// 現在のBETデータ
    /// </summary>
    public Master.BetData currentBetData = null;
    /// <summary>
    /// 砲台に付いてるギア一覧
    /// </summary>
    public Master.GearData[] gears = null;
    /// <summary>
    /// スキル管理
    /// </summary>
    public SkillGroupManager skill = null;
    /// <summary>
    /// 砲台
    /// </summary>
    public Turret turret = null;

    /// <summary>
    /// construct
    /// </summary>
    protected BattleUserData(UserTurretData turretData)
    {
        //砲台データ
        this.turretData = turretData;
        var batteryData = Masters.BatteryDB.FindById(turretData.batteryMasterId);
        var barrelData = Masters.BarrelDB.FindById(turretData.barrelMasterId);
        var bulletData = Masters.BulletDB.FindById(turretData.bulletMasterId);
        //砲台に付いてるギア一覧
        this.gears = this.turretData.GetGearMasterIds().Select(id => Masters.GearDB.FindById(id)).ToArray();
        //スキル効果追加
        this.skill = new SkillGroupManager();
        this.skill.AddRangeSkillGroup(gears.Select(x => x.skillGroupId));
        this.skill.AddSkillGroup(bulletData.skillGroupId);
        //シリーズスキル効果追加
        if (batteryData.seriesId == barrelData.seriesId && batteryData.seriesId == bulletData.seriesId)
        {
            var seriesData = Masters.TurretSerieseDB.FindById(batteryData.seriesId);
            var seriesSkillData = Masters.SerieseSkillDB.FindById(seriesData.seriesSkillId);
            this.skill.AddSkillGroup(seriesSkillData.skillGroupId);
        }
        //VIPスキル効果追加
        this.skill.AddRangeSkillGroup(Masters.VipBenefitDB
            .GetList()
            .Where(x => x.skillGroupId > 0 && x.vipLevel <= UserData.Get().vipLevel)
            .Select(x => x.skillGroupId)
        );
    }

}//class BattleUserData

/// <summary>
/// シングルバトル用ユーザーデータ
/// </summary>
public class SingleBattleUserData : BattleUserData
{
    /// <summary>
    /// HP
    /// </summary>
    public int hp = 30;
    /// <summary>
    /// 最大HP
    /// </summary>
    public int maxHp = 30;

    /// <summary>
    /// construct
    /// </summary>
    public SingleBattleUserData(Master.SingleStageData stageData, UserTurretData turretData)
        : base(turretData)
    {
        this.maxHp =
        this.hp = (int)stageData.userHp;
        this.currentBetData = new Master.BetData{
            maxBet = stageData.bet,
            needFvPoint = stageData.needFvPoint,
        };

        //スキル効果による開戦時FVゲージの上昇
        float fvGaugeUp = this.skill.InitFvGaugeUp();
        this.fvPoint += (long)(this.currentBetData.needFvPoint * fvGaugeUp);
        this.fvPoint = (long)Mathf.Min(this.currentBetData.needFvPoint, this.fvPoint);
        //Debug.LogFormat("スキル効果によりFVゲージ{0}%上昇。{1}/{2}から開始。", fvGaugeUp * 100, this.fvPoint, this.currentBetData.needFvPoint);
    }
}

/// <summary>
/// マルチバトル用ユーザーデータ
/// </summary>
public class MultiBattleUserData : BattleUserData
{
    /// <summary>
    /// 龍玉所持数
    /// </summary>
    public uint ballNum = 0;
    /// <summary>
    /// 龍魂所持数
    /// </summary>
    public uint soulNum = 0;

    /// <summary>
    /// construct
    /// </summary>
    public MultiBattleUserData(UserTurretData turretData, long fvPoint, Master.BetData betData, MultiPlayApi.TMultiSoulBall soulBall)
        : base(turretData)
    {
        this.fvPoint = fvPoint;
        this.currentBetData = betData;

        if (soulBall != null)
        {
            this.ballNum = soulBall.ball % 9;
            this.soulNum = soulBall.soul % 9;
        }
    }
}

}//namespace Battle
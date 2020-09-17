using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// バトルアイテムスキル基底
/// </summary>
public abstract class SkillBattleItemBase : SkillBase
{
    /// <summary>
    /// 効果時間
    /// </summary>
    [JsonProperty("duration")]
    public uint duration;
    /// <summary>
    /// 使用間隔
    /// </summary>
    [JsonProperty("coolTime")]
    public uint coolTime;
    /// <summary>
    /// 砲台制御するかどうか
    /// </summary>
    public virtual bool isTurretController => false;
    /// <summary>
    /// SE名
    /// </summary>
    public abstract string seName { get; }
}
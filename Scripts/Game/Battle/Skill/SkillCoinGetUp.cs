using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// コイン獲得量UP
/// </summary>
public class SkillCoinGetUp : SkillBase
{
    /// <summary>
    /// 上昇％（万分率）
    /// </summary>
    [JsonProperty("gaugeUp")]
    public uint gaugeUp;

    /// <summary>
    /// コイン獲得量上昇％
    /// </summary>
    public override float CoinGetUp()
    {
        return this.gaugeUp * 0.0001f;
    }
}

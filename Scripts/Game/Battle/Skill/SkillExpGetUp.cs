using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 経験値獲得量UP
/// </summary>
public class SkillExpGetUp : SkillBase
{
    /// <summary>
    /// 上昇％（万分率）
    /// </summary>
    [JsonProperty("gaugeUp")]
    public uint gaugeUp;

    /// <summary>
    /// 経験値獲得量上昇％
    /// </summary>
    public override float ExpGetUp()
    {
        return this.gaugeUp * 0.0001f;
    }
}

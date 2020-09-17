using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// FVゲージ獲得量UP（マルチのみ）
/// </summary>
public class SkillFvGetUp : SkillBase
{
    /// <summary>
    /// 上昇％（万分率）
    /// </summary>
    [JsonProperty("gaugeUp")]
    public uint gaugeUp;

    /// <summary>
    /// FVゲージ獲得量上昇％
    /// </summary>
    public override float FvGetUp()
    {
        return this.gaugeUp * 0.0001f;
    }
}

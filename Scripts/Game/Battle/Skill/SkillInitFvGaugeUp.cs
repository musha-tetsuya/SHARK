using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// スキル：開戦時FVゲージ上昇
/// </summary>
public class SkillInitFvGaugeUp : SkillBase
{
    /// <summary>
    /// 上昇％
    /// </summary>
    [JsonProperty("gaugeUp")]
    public uint gaugeUp;

    /// <summary>
    /// 開戦時FVゲージ上昇％
    /// </summary>
    public override float InitFvGaugeUp()
    {
        return this.gaugeUp * Masters.PercentToDecimal;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// スキル：被ダメージカット
/// </summary>
public class SkillDamageCut : SkillBase
{
    /// <summary>
    /// ダメージカット量％
    /// </summary>
    [JsonProperty("damageCut")]
    public uint damageCut;

    /// <summary>
    /// 被ダメージカット量計算
    /// </summary>
    public override float DamageCut(int baseDamage)
    {
        return baseDamage * Mathf.Clamp(this.damageCut, 0, 100) * Masters.PercentToDecimal;
    }
}

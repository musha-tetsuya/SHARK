using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// スキル：与ダメージ増加
/// </summary>
public class SkillDamageUp : SkillBase
{
    /// <summary>
    /// ダメージ増加量％
    /// </summary>
    [JsonProperty("damageUp")]
    public uint damageUp;

    /// <summary>
    /// 増加ダメージ量計算
    /// </summary>
    public override float DamageUp(uint power, Battle.Fish fish)
    {
        return power * this.damageUp * Masters.PercentToDecimal;
    }
}

/// <summary>
/// スキル：魚タイプキラー
/// </summary>
public class SkillTypeKiller : SkillDamageUp
{
    /// <summary>
    /// 魚タイプ
    /// </summary>
    [JsonProperty("fishType")]
    public uint[] fishType;

    /// <summary>
    /// 増加ダメージ量計算
    /// </summary>
    public override float DamageUp(uint power, Battle.Fish fish)
    {
        //魚タイプが一致するならダメージ増加
        return this.fishType.Contains(fish.master.fishType) ? base.DamageUp(power, fish) : 0;
    }
}

/// <summary>
/// スキル：魚サイズキラー
/// </summary>
public class SkillSizeKiller : SkillDamageUp
{
    /// <summary>
    /// 魚サイズ
    /// </summary>
    [JsonProperty("fishSize")]
    public uint[] fishSize;

    /// <summary>
    /// 増加ダメージ量計算
    /// </summary>
    public override float DamageUp(uint power, Battle.Fish fish)
    {
        //魚サイズが一致するならダメージ増加
        return this.fishSize.Contains(fish.master.size) ? base.DamageUp(power, fish) : 0;
    }
}

/// <summary>
/// スキル：ボスキラー
/// </summary>
public class SkillBossKiller : SkillDamageUp
{
    /// <summary>
    /// 増加ダメージ量計算
    /// </summary>
    public override float DamageUp(uint power, Battle.Fish fish)
    {
        //ボス魚ならダメージ増加
        return (fish.master.isBoss > 0) ? base.DamageUp(power, fish) : 0;
    }
}
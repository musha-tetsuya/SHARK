using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Battle;

/// <summary>
/// スキル：氷結
/// </summary>
public class SkillFishFreezing : SkillBase
{
    /// <summary>
    /// 確率
    /// </summary>
    [JsonProperty("probability")]
    public uint pobability;
    /// <summary>
    /// 効果時間
    /// </summary>
    [JsonProperty("duration")]
    public uint duration;

    /// <summary>
    /// 魚被ダメ時
    /// </summary>
    public override void OnFishDamaged(Fish fish)
    {
        //発動確率チェック
        if (Random.Range(0, 100) < this.pobability)
        {
            fish.Freezing(this.duration * Masters.MilliSecToSecond);
        }
    }
}

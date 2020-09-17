using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Battle;

/// <summary>
/// スキル：魚のスピードダウン
/// </summary>
public class SkillFishSpeedDown : SkillBase
{
    /// <summary>
    /// 確率
    /// </summary>
    [JsonProperty("probability")]
    public uint probability;
    /// <summary>
    /// 低下率％
    /// </summary>
    [JsonProperty("speedDown")]
    public uint speedDown;
    /// <summary>
    /// 効果時間
    /// </summary>
    [JsonProperty("duration")]
    public uint duration;
    /// <summary>
    /// 効果時間
    /// </summary>
    private float maxTime => this.duration * Masters.MilliSecToSecond;

    /// <summary>
    /// 魚被ダメ時
    /// </summary>
    public override void OnFishDamaged(Fish fish)
    {
        //発動確率チェック
        if (Random.Range(0, 100) < this.probability)
        {
            //移動速度低下状態になる
            fish.conditionManager.Add(new FishConditionSpeedDown(this.maxTime, this.speedDown));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// スキル：アイテム再使用間隔短縮
/// </summary>
public class SkillShortenCoolTime : SkillBase
{
    /// <summary>
    /// 短縮秒数
    /// </summary>
    [JsonProperty("shortenCoolTime")]
    public uint shortenCoolTime;

    /// <summary>
    /// アイテム再使用間隔短縮
    /// </summary>
    public override float ShortenCoolTime()
    {
        return this.shortenCoolTime * Masters.MilliSecToSecond;
    }
}

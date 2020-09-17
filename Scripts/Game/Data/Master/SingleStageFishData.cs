using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 魚ステータス基底
    /// </summary>
    public abstract class IStageFishData : ModelBase
    {
        [JsonProperty("fishId")]
        public uint fishId { get; set; }

        [JsonProperty("fvRate")]
        public uint fvRate { get; set; }
    }

    /// <summary>
    /// シングルステージでの魚ステータス
    /// </summary>
    public class SingleStageFishData : IStageFishData
    {
        [JsonProperty("stageId")]
        public uint stageId { get; set; }

        [JsonProperty("hp")]
        public uint hp { get; set; }

        [JsonProperty("power")]
        public uint power { get; set; }
    }
}
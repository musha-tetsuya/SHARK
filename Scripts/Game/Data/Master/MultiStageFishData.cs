using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// マルチステージでの魚ステータス
    /// </summary>
    public class MultiStageFishData : IStageFishData
    {
        [JsonProperty("multiWorldId")]
        public uint worldId { get; set; }

        [JsonProperty("hp")]
        public uint hp { get; set; }

        [JsonProperty("rate")]
        public uint rate { get; set; }

        [JsonProperty("captureId")]
        public uint captureId { get; set; }

        [JsonProperty("specialFishId")]
        public uint specialFishId { get; set; }

        [JsonProperty("ballRateId")]
        public uint ballRateId { get; set; }

        [JsonProperty("soulRateId")]
        public uint soulRateId { get; set; }
    }
}
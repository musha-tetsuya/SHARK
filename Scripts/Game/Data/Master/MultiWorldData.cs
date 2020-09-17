using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルワールドデータ
    /// </summary>
    public class MultiWorldData : ModelBase
    {
        [JsonProperty("worldName"), Localize]
        public string name { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("minMultipliedBet")]
        public uint minBet { get; set; }

        [JsonProperty("maxMultipliedBet")]
        public uint maxBet { get; set; }

        [JsonProperty("betCoin")]
        public uint betCoin { get; set; }
        
        [JsonProperty("releaseConditionStageId")]
        public uint releaseConditionStageId { get; set; }

        [JsonProperty("isComingSoon")]
        public uint isComingSoon { get; set; }

        [JsonProperty("bgmName")]
        public string bgmName { get; set; }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルワールドデータ
    /// </summary>
    public class SingleWorldData : ModelBase
    {
        [JsonProperty("worldName"), Localize]
        public string name { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("releaseConditionStageId")]
        public uint releaseConditionStageId { get; set; }

        [JsonProperty("isComingSoon")]
        public uint isComingSoon { get; set; }

        [JsonProperty("bgmName")]
        public string bgmName { get; set; }
    }
}
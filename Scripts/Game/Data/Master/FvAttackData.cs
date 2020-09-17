using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// FVアタックデータ
    /// </summary>
    public class FvAttackData : ModelBase
    {
        [JsonProperty("fvName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("fvCategory")]
        public uint type { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("power")]
        public uint power { get; set; }

        [JsonProperty("effectTime")]
        public uint time { get; set; }

        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 砲台拡張情報
    /// </summary>
    public class CannonExpansionData : ModelBase
    {
        [JsonProperty("nextId")]
        public uint nextId { get; set; }

        [JsonProperty("needGem")]
        public uint needGem { get; set; }

        [JsonProperty("maxPossession")]
        public uint maxPossession { get; set; }
    }
}
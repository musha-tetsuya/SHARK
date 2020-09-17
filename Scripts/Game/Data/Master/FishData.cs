using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 魚データ
    /// </summary>
    public class FishData : ModelBase
    {
        [JsonProperty("fishName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("fishCategoryId")]
        public uint fishType { get; set; }

        [JsonProperty("size")]
        public uint size { get; set; }

        [JsonProperty("isBoss")]
        public uint isBoss { get; set; }

        [JsonProperty("isFixAxis")]
        public uint isFixAxis { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// レベルデータ
    /// </summary>
    public class LevelData : ModelBase
    {
        [JsonProperty("level")]
        public uint lv { get; set; }

        [JsonProperty("nextExp")]
        public uint exp { get; set; }

        [JsonProperty("betId")]
        public uint betId { get; set; }
    }
}

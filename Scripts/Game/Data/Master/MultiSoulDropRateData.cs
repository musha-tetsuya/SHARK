using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 龍魂ドロップ率データ
    /// </summary>
    public class MultiSoulDropRateData : ModelBase
    {
        [JsonProperty("soulRateId")]
        public uint soulRateId { get; set; }

        [JsonProperty("rate")]
        public uint rate { get; set; }
    }
}
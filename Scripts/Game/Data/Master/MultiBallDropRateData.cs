using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 龍玉ドロップ率データ
    /// </summary>
    public class MultiBallDropRateData : ModelBase
    {
        [JsonProperty("ballRateId")]
        public uint ballRateId { get; set; }

        [JsonProperty("minHoldBall")]
        public uint minHoldBall { get; set; }

        [JsonProperty("maxHoldBall")]
        public uint maxHoldBall { get; set; }

        [JsonProperty("rate")]
        public uint rate { get; set; }
    }
}
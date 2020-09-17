using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// BETデータ
    /// </summary>
    public class BetData : ModelBase
    {
        [JsonProperty("maxBet")]
        public uint maxBet { get; set; }

        [JsonProperty("needFvPoint")]
        public uint needFvPoint { get; set; }
    }
}

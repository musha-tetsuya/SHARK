using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 捕獲確率データ
    /// </summary>
    public class FishCaptureData : ModelBase
    {
        [JsonProperty("captureId")]
        public uint groupId { get; set; }

        [JsonProperty("fishHp")]
        public uint hpRate { get; set; }

        [JsonProperty("captureProbability")]
        public uint probability { get; set; }
    }
}
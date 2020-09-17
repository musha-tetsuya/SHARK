using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 魚パーティクルデータ
    /// </summary>
    public class FishParticleData : ModelBase
    {
        [JsonProperty("fishId")]
        public uint fishId { get; set; }

        [JsonProperty("attachingPosition")]
        public string attachingPosition { get; set; }

        [JsonProperty("particleName")]
        public string particleName { get; set; }
    }
}
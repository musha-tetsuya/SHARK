using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 砲台シリーズデータ
    /// </summary>
    public class TurretSerieseData : ModelBase
    {
        [JsonProperty("seriesName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("seriesSkillId")]
        public uint seriesSkillId { get; set; }
    }
}

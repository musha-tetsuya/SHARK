using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 砲台シリーズスキルデータ
    /// </summary>
    public class SeriesSkillData : ModelBase
    {
        [JsonProperty("seriesSkillName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }
    }
}

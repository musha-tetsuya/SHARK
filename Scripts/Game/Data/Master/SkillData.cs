using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// スキルデータ
    /// </summary>
    public class SkillData : ModelBase
    {
        [JsonProperty("effectType")]
        public uint skillType { get; set; }

        [JsonProperty("skillName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }
    }
}

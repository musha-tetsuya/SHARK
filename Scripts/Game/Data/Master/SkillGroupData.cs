using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// スキルグループデータ
    /// </summary>
    public class SkillGroupData : ModelBase
    {
        [JsonProperty("skillGroupId")]
        public uint groupId { get; set; }

        [JsonProperty("skillId")]
        public uint skillId { get; set; }

        [JsonProperty("effect")]
        public string effectValue { get; set; }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルステージ初回報酬データ
    /// </summary>
    public class SingleStageFirstRewardData : ModelBase
    {
        [JsonProperty("rewardFirstGroupId")]
        public uint groupId { get; set; }

        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint amount { get; set; }
    }
}
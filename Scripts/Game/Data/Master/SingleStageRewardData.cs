using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルステージ報酬データ
    /// </summary>
    public class SingleStageRewardData : ModelBase
    {
        [JsonProperty("rewardGroupId")]
        public uint groupId { get; set; }

        [JsonProperty("rewardLotGroupId")]
        public uint lotGroupId { get; set; }
    }
}
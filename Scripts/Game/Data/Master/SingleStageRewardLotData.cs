using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルステージ報酬データ
    /// </summary>
    public class SingleStageRewardLotData : ModelBase
    {
        [JsonProperty("rewardLotGroupId")]
        public uint lotGroupId { get; set; }

        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}

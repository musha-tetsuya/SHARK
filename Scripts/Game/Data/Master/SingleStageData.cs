using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// シングルステージデータ
    /// </summary>
    public class SingleStageData : ModelBase
    {
        /// <summary>
        /// ステージタイプ
        /// </summary>
        public enum StageType
        {
            Story,  //ストーリー
            Battle, //バトル
        }

        [JsonProperty("worldId")]
        public uint worldId { get; set; }

        [JsonProperty("stageType")]
        public uint type { get; set; }

        [JsonProperty("stageName"), Localize]
        public string name { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("needCoin")]
        public uint needCoin { get; set; }

        [JsonProperty("hp")]
        public uint userHp { get; set; }

        [JsonProperty("bet")]
        public uint bet { get; set; }

        [JsonProperty("needFvPoint")]
        public uint needFvPoint { get; set; }

        [JsonProperty("itemId1")]
        public uint itemId1 { get; set; }

        [JsonProperty("amount1")]
        public uint amount1 { get; set; }

        [JsonProperty("itemId2")]
        public uint itemId2 { get; set; }

        [JsonProperty("amount2")]
        public uint amount2 { get; set; }

        [JsonProperty("rewardFirstGroupId")]
        public uint rewardFirstGroupId { get; set; }

        [JsonProperty("rewardGroupId")]
        public uint rewardGroupId { get; set; }
    }
}
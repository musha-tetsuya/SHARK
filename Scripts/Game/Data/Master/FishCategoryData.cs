using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 魚カテゴリデータ
    /// </summary>
    public class FishCategoryData : ModelBase
    {
        [JsonProperty("fishCategoryName"), Localize]
        public string name { get; set; }
    }
}
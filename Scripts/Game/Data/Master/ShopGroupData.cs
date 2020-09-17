using System;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 商品グループデータ
    /// </summary>
    public class ShopGroupData : ModelBase
    {
        [JsonProperty("shopGroupName"), Localize]
        public string name { get; set; }

        [JsonProperty("startDate")]
        public DateTime? startDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? endDate { get; set; }
    }
}

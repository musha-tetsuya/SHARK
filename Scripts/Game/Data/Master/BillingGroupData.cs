using System;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 課金商品グループデータ
    /// </summary>
    public class BillingGroupData : ModelBase
    {
        [JsonProperty("billingGroupName"), Localize]
        public string name { get; set; }

        [JsonProperty("startDate")]
        public DateTime? startDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? endDate { get; set; }
    }
}

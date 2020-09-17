using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 課金商品データ
    /// </summary>
    public class BillingData : ModelBase
    {
        [JsonProperty("productId")]
        public string productId { get; set; }

        [JsonProperty("platform")]
        public uint platform { get; set; }

        [JsonProperty("billingGroupId")]
        public uint billingGroupId { get; set; }

        [JsonProperty("billingName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("recommended")]
        public uint recommended { get; set; }

        [JsonProperty("sort")]
        public uint sort { get; set; }

        [JsonProperty("needMoney")]
        public uint needMoney { get; set; }

        [JsonProperty("billingItemId")]
        public uint billingItemId { get; set; }

        [JsonProperty("maxCount")]
        public uint? maxCount { get; set; }
    }
}

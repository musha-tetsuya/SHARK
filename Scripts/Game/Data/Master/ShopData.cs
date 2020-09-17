using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 商品データ
    /// </summary>
    public class ShopData : ModelBase
    {
        [JsonProperty("shopGroupId")]
        public uint shopGroupId { get; set; }

        [JsonProperty("shopName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("recommended")]
        public uint recommended { get; set; }

        [JsonProperty("sort")]
        public uint sort { get; set; }

        [JsonProperty("needGem")]
        public uint needChargeGem { get; set; }

        [JsonProperty("needFreeGem")]
        public uint needFreeGem { get; set; }

        [JsonProperty("needCoin")]
        public uint needCoin { get; set; }

        [JsonProperty("shopItemPayId")]
        public uint? shopItemPayId { get; set; }

        [JsonProperty("shopItemId")]
        public uint shopItemId { get; set; }

        [JsonProperty("maxCount")]
        public uint? maxCount { get; set; }

        [JsonProperty("multiFlg")]
        public uint multiFlg { get; set; }
    }
}

using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 課金商品アイテムデータ
    /// </summary>
    public class BillingItemData : ModelBase
    {
        [JsonProperty("billingItemId")]
        public uint billingItemId { get; set; }

        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}

using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 商品アイテムデータ
    /// </summary>
    public class ShopItemData : ModelBase
    {
        [JsonProperty("shopItemId")]
        public uint shopItemId { get; set; }

        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}

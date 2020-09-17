using Newtonsoft.Json;

namespace Master
{
    public class ItemSellData : ModelBase
    {
        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}


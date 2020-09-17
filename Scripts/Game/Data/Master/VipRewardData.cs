using Newtonsoft.Json;

namespace Master
{
    public class VipRewardData : ModelBase
    {
        [JsonProperty("vipLevel")]
        public uint vipLevel { get; set; }

        [JsonProperty("itemType")]
        public uint itemType {get; set;}

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}

using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// VIPレベルデータ
    /// </summary>
    public class VipLevelData : ModelBase
    {
        [JsonProperty("vipLevel")]
        public uint vipLevel { get; set; }

        [JsonProperty("nextExp")]
        public uint nextExp { get; set; }
    }
}

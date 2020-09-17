using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// ミッションリワードデータ
    /// </summary>
    public class MissionRewardData : ModelBase
    {
        [JsonProperty("itemType")]
        public uint itemType { get; set; }

        [JsonProperty("itemId")]
        public uint itemId { get; set; }

        [JsonProperty("itemNum")]
        public uint itemNum { get; set; }
    }
}

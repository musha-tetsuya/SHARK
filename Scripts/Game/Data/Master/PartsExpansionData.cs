using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// パーツ拡張情報
    /// </summary>
    public class PartsExpansionData : ModelBase
    {
        [JsonProperty("nextId")]
        public uint nextId { get; set; }

        [JsonProperty("needGem")]
        public uint needGem { get; set; }

        [JsonProperty("maxPossession")]
        public uint maxPossession { get; set; }
    }
}

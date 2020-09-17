using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// ギア拡張情報
    /// </summary>
    public class GearExpansionData : ModelBase
    {
        [JsonProperty("nextId")]
        public uint nextId { get; set; }

        [JsonProperty("needGem")]
        public uint needGem { get; set; }

        [JsonProperty("maxPossession")]
        public uint maxPossession { get; set; }
    }
}

using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// VIP得点データ
    /// </summary>
    public class VipBenefitData : ModelBase
    {
        [JsonProperty("benefitTypeId")]
        public uint benefitTypeId { get; set; }

        [JsonProperty("vipLevel")]
        public uint vipLevel { get; set; }

        [JsonProperty("effect")]
        public uint effect { get; set; }

        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }
    }
}

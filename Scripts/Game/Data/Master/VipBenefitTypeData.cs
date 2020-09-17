using Newtonsoft.Json;

namespace Master
{
    public class VipBenefitTypeData : ModelBase
    {
        [JsonProperty("benefitTypeName"), Localize]
        public string benefitTypeName { get; set; }
    }
}
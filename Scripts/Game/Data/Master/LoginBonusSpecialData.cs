using Newtonsoft.Json;

namespace Master
{
    public class LoginBonusSpecialData : ModelBase
    {
        [JsonProperty("loginBonusName"), Localize]
        public string name { get; set;}
        
        [JsonProperty("itemType")]
        public uint itemType { get; set;}

        [JsonProperty("itemId")]
        public uint itemId { get; set;}

        [JsonProperty("itemNum")]
        public uint itemNum { get; set;}
    }
}


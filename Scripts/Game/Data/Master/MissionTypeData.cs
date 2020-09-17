using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// ミッションタイプデータ
    /// </summary>
    public class MissionTypeData : ModelBase
    {
        [JsonProperty("missionName"), Localize]
        public string missionName { get; set; }
    }
}

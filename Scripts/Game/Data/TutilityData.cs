using Newtonsoft.Json;

/// <summary>
/// ユーザーutilityデータ
/// </summary>
public class TutilityData
{
    [JsonProperty("id")]
    public uint id;
    [JsonProperty("userId")]
    public uint userId;
    [JsonProperty("utilityId")]
    public uint utilityId;
    [JsonProperty("utilityType")]
    public uint utilityType;
    [JsonProperty("utilityNum")]
    public uint utilityNum;
}

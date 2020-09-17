using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// メッセージデータ
    /// </summary>
    public class MessageData : ModelBase
    {
        [JsonProperty("messageText"), Localize]
        public string messageText { get; set; }
    }
}

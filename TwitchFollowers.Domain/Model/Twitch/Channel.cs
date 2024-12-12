using Newtonsoft.Json;

namespace TwitchFollowers.Domain.Model.Twitch
{
    public class Channel
    {
        [JsonProperty("data")]
        public ChannelData[] Data { get; set; }
    }

    public class ChannelData
    {
        [JsonProperty("broadcaster_id")]
        public string Id { get; set; }

        [JsonProperty("broadcaster_login")]
        public string Login { get; set; }

        [JsonProperty("broadcaster_language")]
        public string Language { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tags")]
        public string[] Tags{ get; set; }
    }
}

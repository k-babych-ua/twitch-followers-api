using Newtonsoft.Json;

namespace TwitchFollowers.Domain.Model.Twitch
{
    public class User
    {
        [JsonProperty("data")]
        public UserData[] Data { get; set; }
    }

    public class UserData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}

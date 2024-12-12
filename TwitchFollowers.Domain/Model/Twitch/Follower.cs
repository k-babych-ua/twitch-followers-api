using Newtonsoft.Json;

namespace TwitchFollowers.Domain.Model.Twitch
{
    public class Follower
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "followedAt")]
        public string FollowedAt { get; set; }
    }
}

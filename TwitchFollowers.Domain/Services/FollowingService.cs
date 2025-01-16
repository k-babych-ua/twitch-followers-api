using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Extensions;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Model.Twitch;

namespace TwitchFollowers.Domain.Services
{
    public class FollowingService
    {
        private IHttpClientFactory _httpClientFactory;

        private Urls _urls;

        public FollowingService(IHttpClientFactory httpClientFactory,
            IOptions<Urls> urlOptions,
            IOptions<TagsConfig> tagsConfig)
        {
            _httpClientFactory = httpClientFactory;

            _urls = urlOptions.Value;
        }

        public async Task<Follower[]> GetUserFollowings(string username)
        {
            using HttpClient client = _httpClientFactory.CreateClient();

            using var req = new HttpRequestMessage(HttpMethod.Get, _urls.GetFollows.Replace("{username}", username));

            var response = await client.SendAsync(req);

            return await response.Content.ReadAsJsonAsync<Follower[]>();
        }
    }
}

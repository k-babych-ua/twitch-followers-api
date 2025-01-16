using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Extensions;
using TwitchFollowers.Domain.Model.API;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Model.Twitch;

namespace TwitchFollowers.Domain.Services
{
    public class TwitchService
    {
        private IHttpClientFactory _httpClientFactory;

        private Token _token = null;
        private DateTime _tokenExpiresOn = DateTime.MinValue;

        private Urls _urls;
        private Twitch _twitchOptions;
        private TagsConfig _tagsConfig;

        public TwitchService(IHttpClientFactory httpClientFactory, 
            IOptions<Urls> urlOptions, 
            IOptions<Twitch> twitchOptions, 
            IOptions<TagsConfig> tagsConfig)
        {
            _httpClientFactory = httpClientFactory;

            _urls = urlOptions.Value;
            _twitchOptions = twitchOptions.Value;
            _tagsConfig = tagsConfig.Value;
        }

        public async Task<User> GetUserInfo(string username)
        {
            await AcquireToken();

            using HttpClient client = _httpClientFactory.CreateClient();

            using var req = new HttpRequestMessage(HttpMethod.Get, _urls.UserInfo.Replace("{username}", username));
            req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_token.access_token}");
            req.Headers.TryAddWithoutValidation("Client-Id", _twitchOptions.client_id);

            var response = await client.SendAsync(req);

            return await response.Content.ReadAsJsonAsync<User>();
        }

        public async Task<Channel> GetChannelInfo(string broadcasterid)
        {
            await AcquireToken();

            using HttpClient client = _httpClientFactory.CreateClient();

            using var req = new HttpRequestMessage(HttpMethod.Get, _urls.ChannelInfo.Replace("{channel}", broadcasterid));
            req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_token.access_token}");
            req.Headers.TryAddWithoutValidation("Client-Id", _twitchOptions.client_id);

            var response = await client.SendAsync(req);

            return await response.Content.ReadAsJsonAsync<Channel>();
        }

        public TagsAnalytics GetTagsAnalytics(ChannelData channel)
        {
            return new TagsAnalytics()
            {
                TotalTags = channel.Tags.Count(),
                GreenTags = channel.Tags.Count(x => _tagsConfig.Green.Any(y => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0)),
                RedTags = channel.Tags.Count(x => _tagsConfig.Red.Any(y => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0))
            };
        }

        private async Task AcquireToken()
        {
            if (_token == null || DateTime.UtcNow > _tokenExpiresOn.AddSeconds(30) || 
                string.IsNullOrWhiteSpace(_token.access_token))
            {
                _token = await GetToken();
                _tokenExpiresOn = DateTime.UtcNow.AddSeconds(_token.expires_in);
            }
        }

        private async Task<Token> GetToken()
        {
            using HttpClient client = _httpClientFactory.CreateClient();

            var keyValues = new Dictionary<string, string>()
            {
                { "client_id", _twitchOptions.client_id },
                { "client_secret", _twitchOptions.client_secret },
                { "grant_type", _twitchOptions.grant_type }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, _urls.ClientCredentialFlow) 
            { 
                Content = new FormUrlEncodedContent(keyValues) 
            };

            var response = await client.SendAsync(req);

            return await response.Content.ReadAsJsonAsync<Token>();
        }
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TwitchFollowers.Domain.Extensions;
using TwitchFollowers.Domain.Model.API;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Model.Twitch;

namespace TwitchFollowers.Domain.Services
{
    public class TwitchService
    {
        private readonly IMemoryCache _cache;

        private IHttpClientFactory _httpClientFactory;

        private Token _token = null;
        private DateTime _tokenExpiresOn = DateTime.MinValue;

        private Urls _urls;
        private Twitch _twitchOptions;
        private TagsConfig _tagsConfig;
        private AppConfig _appConfig;

        public TwitchService(IHttpClientFactory httpClientFactory, IMemoryCache cache,
            IOptions<Urls> urlOptions, 
            IOptions<Twitch> twitchOptions, 
            IOptions<TagsConfig> tagsConfig,
            IOptions<AppConfig> appConfig)
        {
            _cache = cache; 
            _httpClientFactory = httpClientFactory;

            _urls = urlOptions.Value;
            _twitchOptions = twitchOptions.Value;
            _tagsConfig = tagsConfig.Value;
            _appConfig = appConfig.Value;
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

        public async Task<Channel> GetChannelInfoCached(string broadcasterid)
        {
            if (_cache.TryGetValue(broadcasterid, out Channel data))
            {
                return data;
            }
            
            data = await GetChannelInfo(broadcasterid);

            if (data != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_appConfig.CacheAbsoluteExpirationInMinutes));

                _cache.Set(broadcasterid, data, cacheOptions);
            }

            return data;
        }

        public TagsAnalytics GetTagsAnalytics(ChannelData channel)
        {
            if (channel != null && channel.Tags.Any())
            {
                return new TagsAnalytics()
                {
                    TotalTags = channel.Tags.Count(),
                    GreenTags = channel.Tags.Count(x => _tagsConfig.Green.Any(y => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0)),
                    RedTags = channel.Tags.Count(x => _tagsConfig.Red.Any(y => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0))
                };
            }

            return new TagsAnalytics(0, 0, 0);
        }

        public TagsAnalytics GetTagsAnalyticsCached(ChannelData channel)
        {
            string key = $"{channel.Id}-ta";
            if (_cache.TryGetValue(key, out TagsAnalytics data))
            {
                return data;
            }

            data = GetTagsAnalytics(channel);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_appConfig.CacheAbsoluteExpirationInMinutes));

            _cache.Set(key, data, cacheOptions);

            return data;
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

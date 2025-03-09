using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Services;

namespace TwitchFollowers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Health : ControllerBase
    {
        private TwitchService _twitchService;

        private AppConfig _appConfig;

        public Health(IHttpClientFactory httpClientFactory, TwitchService twitchService, FollowingService followingService,
            IOptions<AppConfig> appConfig)
        {
            _twitchService = twitchService;
            _appConfig = appConfig.Value;
        }

        [HttpHead("check")]
        public async Task<ActionResult> Check()
        {
            string cacheChannel = _appConfig.CacheChannels.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(cacheChannel))
            {
                await _twitchService.GetChannelInfoCached(cacheChannel);
            }

            return Ok();
        }
    }
}

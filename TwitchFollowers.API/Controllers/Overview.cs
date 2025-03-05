using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Model.API;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Model.Twitch;
using TwitchFollowers.Domain.Services;

namespace TwitchFollowers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Overview : ControllerBase
    {
        private TwitchService _twitchService;
        private FollowingService _followingService;

        private AppConfig _appConfig;

        private ParallelOptions _parallelOptions;

        public Overview(IHttpClientFactory httpClientFactory, TwitchService twitchService, FollowingService followingService,
            IOptions<AppConfig> appConfig)
        {
            _twitchService = twitchService;
            _followingService = followingService;

            if (appConfig.Value != null)
            {
                _appConfig = appConfig.Value;

                _parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = appConfig.Value.MaxDegreeOfParallelism
                };
            }
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<string>> Get(string username)
        {
            var userInfo = await _twitchService.GetUserInfo(username);

            if (userInfo == null || userInfo.Data == null)
            {
                return NotFound($"No user info found for {username}. Please try again");
            }

            var channelInfo = await _twitchService.GetChannelInfoCached(userInfo.Data[0].Id);

            if (channelInfo == null || channelInfo.Data == null)
            {
                return NotFound($"No channel info found for {username}. Please try again");
            }

            var response = new OverviewResponse()
            {
                UserInfo = userInfo?.Data?.FirstOrDefault(),
                ChannelInfo = channelInfo?.Data?.FirstOrDefault()
            };

            if (response.ChannelInfo != null)
            {
                response.TagsAnalytics = _twitchService.GetTagsAnalyticsCached(response.ChannelInfo);

                var follows = await _followingService.GetUserFollowings(username);

                List<TagsAnalytics> channelTagAnalytics = new List<TagsAnalytics>();
                await Parallel.ForEachAsync(follows, _parallelOptions, async (follow, ct) =>
                {
                    var followChannelInfo = await _twitchService.GetChannelInfoCached(follow.Id);

                    if (followChannelInfo?.Data?.FirstOrDefault() is ChannelData data)
                    {
                        channelTagAnalytics.Add(_twitchService.GetTagsAnalyticsCached(data));
                    }
                });

                response.FollowingAnalytics.TotalFollowings = follows.Count();
                response.FollowingAnalytics.RedFollowings = channelTagAnalytics.Count(x => x.RedTags > 0 && x.GreenTags <= 0);
                response.FollowingAnalytics.GreenFollowings = channelTagAnalytics.Count(x => x.GreenTags > 0 && x.RedTags <= 0);
                response.FollowingAnalytics.Mixed = channelTagAnalytics.Count(x => x.GreenTags > 0 && x.RedTags > 0);
            }

            return Ok(response);
        }
    }
}

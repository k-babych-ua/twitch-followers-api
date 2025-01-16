using Microsoft.AspNetCore.Mvc;
using TwitchFollowers.Domain.Model.API;
using TwitchFollowers.Domain.Services;

namespace TwitchFollowers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Overview : ControllerBase
    {
        private TwitchService _twitchService;
        private FollowingService _followingService;

        private ParallelOptions _parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 4
        };

        public Overview(IHttpClientFactory httpClientFactory, TwitchService twitchService, FollowingService followingService)
        {
            _twitchService = twitchService;
            _followingService = followingService;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<string>> Get(string username)
        {
            var userInfo = await _twitchService.GetUserInfo(username);
            var channelInfo = await _twitchService.GetChannelInfo(userInfo.Data[0].Id);

            var response = new OverviewResponse()
            {
                UserInfo = userInfo.Data.FirstOrDefault(),
                ChannelInfo = channelInfo.Data.FirstOrDefault()
            };

            if (response.ChannelInfo != null)
            {
                response.TagsAnalytics = _twitchService.GetTagsAnalytics(response.ChannelInfo);

                var follows = await _followingService.GetUserFollowings(username);

                response.FollowingAnalytics.TotalFollowings = follows.Count();

                List<TagsAnalytics> channelTagAnalytics = new List<TagsAnalytics>();

                await Parallel.ForEachAsync(follows, _parallelOptions, async (follow, ct) =>
                {
                    var followChannelInfo = await _twitchService.GetChannelInfo(follow.Id);
                    channelTagAnalytics.Add(_twitchService.GetTagsAnalytics(followChannelInfo.Data.FirstOrDefault()));
                });

                response.FollowingAnalytics.RedFollowings = channelTagAnalytics.Count(x => x.RedTags > 0 && x.GreenTags <= 0);
                response.FollowingAnalytics.GreenFollowings = channelTagAnalytics.Count(x => x.GreenTags > 0 && x.RedTags <= 0);
                response.FollowingAnalytics.Mixed = channelTagAnalytics.Count(x => x.GreenTags > 0 && x.RedTags > 0);
            }

            return Ok(response);
        }
    }
}

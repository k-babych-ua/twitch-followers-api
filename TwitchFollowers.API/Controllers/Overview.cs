using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Model.API;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Services;

namespace TwitchFollowers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Overview : ControllerBase
    {
        private IHttpClientFactory _httpClientFactory;

        private TwitchService _twitchService;

        public Overview(IHttpClientFactory httpClientFactory, TwitchService twitchService)
        {
            _httpClientFactory = httpClientFactory;

            _twitchService = twitchService;
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
            }

            return Ok(response);
        }
    }
}

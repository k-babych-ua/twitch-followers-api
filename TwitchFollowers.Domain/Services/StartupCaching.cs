using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Model.Twitch;

namespace TwitchFollowers.Domain.Services
{
    public class StartupCaching : IHostedService
    {
        private TwitchService _twitchService;
        private FollowingService _followingService;

        private AppConfig _appConfig;

        private ParallelOptions _parallelOptions;

        public StartupCaching(TwitchService twitchService, FollowingService followingService, IOptions<AppConfig> appConfig)
        {
            _twitchService = twitchService;
            _followingService = followingService;

            _appConfig = appConfig.Value;

            _parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _appConfig.MaxDegreeOfParallelism
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_appConfig.CacheChannels.Any())
            {
                foreach (var channel in _appConfig.CacheChannels)
                {
                    var follows = await _followingService.GetUserFollowings(channel);

                    await Parallel.ForEachAsync(follows, _parallelOptions, async (follow, ct) =>
                    {
                        var followChannelInfo = await _twitchService.GetChannelInfoCached(follow.Id);

                        if (followChannelInfo?.Data?.FirstOrDefault() is ChannelData data)
                        {
                            _twitchService.GetTagsAnalyticsCached(data);
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

namespace TwitchFollowers.Domain.Model.Configuration
{
    public class AppConfig
    {
        public int MaxDegreeOfParallelism { get; set; }

        public int CacheAbsoluteExpirationInMinutes { get; set; }

        public string[] CacheChannels { get; set; }
    }
}

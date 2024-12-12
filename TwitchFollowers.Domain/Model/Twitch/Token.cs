namespace TwitchFollowers.Domain.Model.Twitch
{
    public class Token
    {
        public required string access_token { get; set; }

        /// <summary>
        /// Seconds
        /// </summary>
        public ulong expires_in { get; set; }
    }
}

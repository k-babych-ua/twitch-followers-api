using TwitchFollowers.Domain.Model.Twitch;

namespace TwitchFollowers.Domain.Model.API
{
    public class OverviewResponse
    {
        public UserData UserInfo { get; set; }

        public ChannelData ChannelInfo { get; set; }

        public TagsAnalytics TagsAnalytics { get; set; }

        public FollowingAnalytics FollowingAnalytics { get; set; }

        public OverviewResponse()
        {
            UserInfo = new UserData();
            ChannelInfo = new ChannelData();
            TagsAnalytics = new TagsAnalytics();
            FollowingAnalytics = new FollowingAnalytics();
        }
    }

    public class TagsAnalytics
    {
        public int TotalTags { get; set; }

        public int GreenTags { get; set; }

        public int RedTags { get; set; }

        public TagsAnalytics()
        {
            TotalTags = 0;
            GreenTags = 0;
            RedTags = 0;
        }

        public TagsAnalytics(int totalTags, int greenTags, int redTags)
        {
            TotalTags = totalTags;
            GreenTags = greenTags;
            RedTags = redTags;
        }
    }

    public class FollowingAnalytics
    {
        public int TotalFollowings { get; set; }

        public int GreenFollowings { get; set; }

        public int RedFollowings { get; set; }

        public int Mixed { get; set; }
    }
}

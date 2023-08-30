using MyTracker.Models;

namespace RedditPostTracker.Interfaces
{
    public interface IRedditApiClient
    {
        Task<List<RedditPost>> GetNewPostsAsync(string subreddit);
        decimal RateLimitUsed { get; }
        decimal RateLimitRemaining { get; }
        decimal RateLimitResetInSeconds { get; }
    }
}

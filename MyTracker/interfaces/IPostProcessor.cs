using MyTracker.Models;

namespace MyTracker.Interfaces
{
    public interface IPostProcessor
    {
        RedditPostData GetPostWithMostUpvotes(string subreddit);
        RedditPostData GetPostWithMostUpvotesSinceMonitoringStart(string subreddit);
        string GetAuthorWithMostPosts(string subreddit);
        string GetAuthorWithMostPostsSinceMonitoringStart(string subreddit);
        Task ProcessNewPostsAsync(string subReddit, List<RedditPost> posts);
    }
}

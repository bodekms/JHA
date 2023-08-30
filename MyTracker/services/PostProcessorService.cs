using MyTracker.Interfaces;
using MyTracker.Models;

namespace MyTracker.Services
{
    public class PostProcessorService : IPostProcessor
    {
        private readonly Dictionary<string, Dictionary<string, RedditPostData>> _posts = new();
        private readonly Dictionary<string, Dictionary<string, CountStore>> _upvoteCount = new();
        private readonly Dictionary<string, Dictionary<string, CountStore>> _authorCount = new();

        private readonly TrackerLoggerService _logger;

        public PostProcessorService(TrackerLoggerService logger)
        {
            _logger = logger;
        }

        public async Task ProcessNewPostsAsync(string subreddit, List<RedditPost> posts)
        {
            CreateSubredditDictionariesIfNotExist(subreddit);

            try
            {
                foreach (var post in posts)
                {
                    if (post.Data == null)
                        continue;

                    AddOrUpdatePost(subreddit, post.Data);
                    AddOrUpdateUpvoteCount(subreddit, post.Data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        #region Dictionary modifiers
        private void CreateSubredditDictionariesIfNotExist(string subreddit)
        {
            if (!_posts.ContainsKey(subreddit))
                _posts.Add(subreddit, new());
            if (!_upvoteCount.ContainsKey(subreddit))
                _upvoteCount.Add(subreddit, new());
            if (!_authorCount.ContainsKey(subreddit))
                _authorCount.Add(subreddit, new());
        }
        private void AddOrUpdatePost(string subReddit, RedditPostData data)
        {
            if (!_posts[subReddit].ContainsKey(data.Id))
            {
                _posts[subReddit].Add(data.Id, data);
                _logger.Log($"[/r/{subReddit}] New post added: {data.Title}");
                AddOrUpdateAuthorCount(subReddit, data);
            }
            else
                _posts[subReddit][data.Id] = data;
        }
        private void AddOrUpdateUpvoteCount(string subreddit, RedditPostData data)
        {
            if (!_upvoteCount[subreddit].ContainsKey(data.Id))
                _upvoteCount[subreddit].Add(data.Id, new CountStore(data.Ups, data.Ups));
            else
                _upvoteCount[subreddit][data.Id].CurrentCount = data.Ups;
        }
        private void AddOrUpdateAuthorCount(string subreddit, RedditPostData data)
        {
            if (!_authorCount[subreddit].ContainsKey(data.Author))
                _authorCount[subreddit].Add(data.Author, new CountStore(1, 1));
            else
                _authorCount[subreddit][data.Author].CurrentCount += 1;
        }
        #endregion

        public RedditPostData GetPostWithMostUpvotes(string subreddit)
        {
            var post = _posts[subreddit].MaxBy(x => x.Value.Ups);
            return post.Value;
        }
        public RedditPostData GetPostWithMostUpvotesSinceMonitoringStart(string subreddit)
        {
            var post = _upvoteCount[subreddit].MaxBy(x => x.Value.DifferenceAfterMostRecentRequest);           
            return _posts[subreddit][post.Key];
        }

        public string GetAuthorWithMostPosts(string subreddit)
        {
            var grouped = _posts[subreddit].GroupBy(x => x.Value.Author).OrderByDescending(grp => grp.Count()).FirstOrDefault();
            if (grouped == null)
                return string.Empty;

            return $"{grouped.Key} ({grouped.Count()})";
        }
        public string GetAuthorWithMostPostsSinceMonitoringStart(string subreddit)
        {
            var kvp = _authorCount[subreddit].MaxBy(x => x.Value.DifferenceAfterMostRecentRequest);

            if (kvp.Value.DifferenceAfterMostRecentRequest == 0)
                return "No repeat authors since polling began";

            return $"{kvp.Key} ({kvp.Value.DifferenceAfterMostRecentRequest})";
        }
    }
}

using MyTracker.Extensions;
using MyTracker.Interfaces;
using RedditPostTracker.Interfaces;

namespace MyTracker.Services
{
    public class RedditTrackerService : IRedditTrackerService
    {
        private readonly IRedditApiClient _redditApiClient;
        private readonly IPostProcessor _postProcessor;
        private readonly IConfigurationService _configurationService;
        private readonly ITrackerLoggerService _trackerLoggerService;        

        public RedditTrackerService(IRedditApiClient redditApiClient, IPostProcessor processor, IConfigurationService configurationService, ITrackerLoggerService logger)
        {
            _redditApiClient = redditApiClient;
            _postProcessor = processor;
            _configurationService = configurationService;
            _trackerLoggerService = logger;
        }

        public async Task TrackNewPostsInSubRedditsAsync(List<string> subReddits, CancellationToken cancellationToken = default)
        {
            int displayStatsAfterNumberOfRequests = _configurationService.GetSetting("Reddit:DisplayStatsAfterNumberOfRequests", 10);

            try
            {
                Parallel.ForEach(subReddits, async subReddit =>
                {
                    int requestCounter = 0;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        ++requestCounter;
                        var response = await _redditApiClient.GetNewPostsAsync(subReddit);

                        if (response != null)
                            await _postProcessor.ProcessNewPostsAsync(subReddit, response);

                        DisplayStatsIfRequestCountMet(subReddit, requestCounter, displayStatsAfterNumberOfRequests);

                        await AddRequiredApiRequestDelayAsync(subReddits.Count);
                    }
                });
            }
            catch (Exception ex)
            {
                _trackerLoggerService.LogError(ex);
            }
            
        }

        private void DisplayStatsIfRequestCountMet(string subreddit, int requestCount, int requestIntervalToDisplayStats)
        {
            if (requestCount % requestIntervalToDisplayStats == 0)
            {
                _trackerLoggerService.LogToConsole(string.Empty);
                _trackerLoggerService.LogToConsole($"***************** Stats [/r/{subreddit}]  *****************");
                _trackerLoggerService.LogToConsole($"Post with most upvotes (overall): {_postProcessor.GetPostWithMostUpvotes(subreddit).MostUpvotesDisplay()}");
                _trackerLoggerService.LogToConsole($"Author with most posts (overall): {_postProcessor.GetAuthorWithMostPosts(subreddit)}");
                _trackerLoggerService.LogToConsole($"Post with most upvotes (since monitoring began): {_postProcessor.GetPostWithMostUpvotesSinceMonitoringStart(subreddit).Title}");
                _trackerLoggerService.LogToConsole($"Author with most posts (since monitoring began): {_postProcessor.GetAuthorWithMostPostsSinceMonitoringStart(subreddit)}");
                _trackerLoggerService.LogToConsole("********************************************************");
                _trackerLoggerService.LogToConsole(string.Empty);
            }
        }

        private async Task AddRequiredApiRequestDelayAsync(int numberOfSubredditsMonitoring)
        {
            if (_redditApiClient.RateLimitRemaining == 0)
            {
                _trackerLoggerService.LogToConsole($"Request limit reached, waiting for reset in {_redditApiClient.RateLimitResetInSeconds} seconds");
                await Task.Delay(Convert.ToInt32(_redditApiClient.RateLimitResetInSeconds) * 1000);
                return;
            }
            decimal delayInSeconds = _redditApiClient.RateLimitResetInSeconds / _redditApiClient.RateLimitRemaining;
            int roundedUpDelayInSeconds = Convert.ToInt32(Math.Ceiling(delayInSeconds));

            await Task.Delay(roundedUpDelayInSeconds * numberOfSubredditsMonitoring * 1000);

            Console.Write(".");
        }
    }
}

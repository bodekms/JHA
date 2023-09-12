using MyTracker.Interfaces;
using MyTracker.Models;
using MyTracker.Services;
using RedditPostTracker.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MyTracker
{
    internal class RedditApiClient : IRedditApiClient
    {
        public decimal RateLimitUsed { get; private set; }
        public decimal RateLimitRemaining { get; private set; }
        public decimal RateLimitResetInSeconds { get; private set; }

        private readonly string _defaultBaseUrl;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationService _configurationService;
        private readonly ITrackerLoggerService _logger;

        public RedditApiClient(IHttpClientFactory httpClientFactory, IConfigurationService configurationService, ITrackerLoggerService logger)
        {
            _httpClientFactory = httpClientFactory;
            _configurationService = configurationService;
            _logger = logger;
            _defaultBaseUrl = _configurationService.GetSetting("Reddit:BaseUrl", "");
        }

        public async Task<List<RedditPost>> GetNewPostsAsync(string subreddit)
        {
            if (string.IsNullOrEmpty(subreddit))
                throw new ArgumentNullException(nameof(subreddit));

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_defaultBaseUrl);

                SetRequestHeaders(client);

                var response = await client.GetAsync($"/r/{subreddit}/new");
                response.EnsureSuccessStatusCode();

                ParseRequestLimitHeadersFromResponse(response.Headers);

                var listing = await response.Content.ReadFromJsonAsync<RedditListing>();

                if (listing == null)
                    return new List<RedditPost>();

                return listing.Data.Children;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return new List<RedditPost>();
            }
            
        }

        private void SetRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "CodingExamRedditApiPostTracker/0.0.1.0");

            var token = _configurationService.GetSetting("Reddit:Token", "");
            client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
        }

        private void ParseRequestLimitHeadersFromResponse(HttpHeaders headers)
        {
            if (headers == null) 
                return;

            if (decimal.TryParse(headers.GetValues("x-ratelimit-remaining").FirstOrDefault(), out var remaining))
                RateLimitRemaining = remaining;

            if (decimal.TryParse(headers.GetValues("x-ratelimit-reset").FirstOrDefault(), out var reset))
                RateLimitResetInSeconds = reset;
        }
    }
}


using Newtonsoft.Json;

namespace MyTracker.Models
{
    public class RedditListing
    {
        [JsonProperty("data")]
        public RedditListingData Data { get; set; }
    }

    public class RedditListingData
    {
        [JsonProperty("children")]
        public List<RedditPost> Children { get; set; }
    }
}

using Newtonsoft.Json;

namespace MyTracker.Models
{
    public class RedditPost
    {
        [JsonProperty("data")]
        public RedditPostData? Data { get; set; }
    }
}

using Newtonsoft.Json;

namespace MyTracker.Models
{
    public class RedditPostData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string? Title { get; set; }
        [JsonProperty("ups")]
        public int Ups { get; set; }
        [JsonProperty("author")]
        public string? Author { get; set; }

        public RedditPostData() { }
    }
}

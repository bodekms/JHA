using Microsoft.Extensions.Logging;
using Moq;
using MyTracker.Models;
using MyTracker.Services;

namespace MyTracker.Tests
{
    [TestClass]
    public class PostProcessorServiceTests
    {
        private const string SUBREDDIT_NAME = "test";

        private List<RedditPost> GeneratePosts()
        {
            List<RedditPost> posts = new List<RedditPost>();
            for (int i = 1; i <= 10; i++)
            {
                posts.Add(new RedditPost
                {
                    Data = new RedditPostData
                    {
                        Id = i.ToString(),
                        Author = $"Author {i}",
                        Ups = i * 2,
                        Title = $"Test post title {i}"
                    }
                });
            }
            return posts;
        }

        [TestMethod]
        public async Task PostProcessor_GetPostWithMostUpvotes_Test()
        {
            //Arrange
            List<RedditPost> posts = GeneratePosts();

            var actualMostUpvotes = new RedditPost
            {
                Data = new RedditPostData
                {
                    Id = "300",
                    Author = "Highest upvote author",
                    Title = "Highest upvotes post",
                    Ups = 1000
                }
            };
            var lowestUpvotes = new RedditPost
            {
                Data = new RedditPostData
                {
                    Id = "100",
                    Author = "Lowest upvote author",
                    Title = "Lowest upvotes post",
                    Ups = 0
                }
            };
            posts.Add(actualMostUpvotes);
            posts.Add(lowestUpvotes);

            var loggerMock = new Mock<ILogger>();
            var processorService = new PostProcessorService(new TrackerLoggerService(loggerMock.Object));

            Assert.IsNotNull(posts);

            //Act
            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);
            var postWithMostUpvotes = processorService.GetPostWithMostUpvotes(SUBREDDIT_NAME);

            //Assert
            Assert.IsNotNull(postWithMostUpvotes);
            Assert.AreEqual(postWithMostUpvotes, actualMostUpvotes.Data);

        }

        [TestMethod]
        public async Task PostProcessor_GetPostWithMostUpvotesSinceMonitoringBegan_Test()
        {
            //Arrange
            var posts = GeneratePosts();
            var originalMostUpvotes = new RedditPost
            {
                Data = new RedditPostData
                {
                    Id = "300",
                    Author = "Highest upvote author",
                    Title = "Highest upvotes post",
                    Ups = 1000
                }
            };
            posts.Add(originalMostUpvotes);

            var loggerMock = new Mock<ILogger>();
            var processorService = new PostProcessorService(new TrackerLoggerService(loggerMock.Object));

            //Act
            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);
            Assert.AreEqual(processorService.GetPostWithMostUpvotes(SUBREDDIT_NAME), originalMostUpvotes.Data);

            posts[0].Data.Ups += 10;
            var newHighestUpvote = posts[0];

            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);
            var mostUpvotesSinceMonitoringStarted = processorService.GetPostWithMostUpvotesSinceMonitoringStart(SUBREDDIT_NAME);

            //Assert
            Assert.AreNotEqual(mostUpvotesSinceMonitoringStarted, originalMostUpvotes.Data);
            Assert.AreEqual(mostUpvotesSinceMonitoringStarted, posts[0].Data);
         
        }

        [TestMethod]
        public async Task PostProcessor_GetAuthorWithMostPosts_Test()
        {
            //Arrange
            var posts = GeneratePosts();
            string authorWithMostPosts = "Test User";
            int postCount = posts.Count;
            int increment = 4;
            for (int i = postCount; i < (postCount + increment); i++)
            {
                posts.Add(new RedditPost
                {
                    Data = new RedditPostData
                    {
                        Id = i.ToString(),
                        Title = $"Test post {i}",
                        Author = authorWithMostPosts,
                        Ups = i
                    }
                });
            }

            var loggerMock = new Mock<ILogger>();
            var processorService = new PostProcessorService(new TrackerLoggerService(loggerMock.Object));

            //Act
            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);
            var author = processorService.GetAuthorWithMostPosts(SUBREDDIT_NAME);

            //Assert
            Assert.AreEqual(author, $"{authorWithMostPosts} ({increment})");
        }

        [TestMethod]
        public async Task PostProcessor_GetAuthorWithMostPostsSinceMonitoringStart_Test()
        {
            //Arrange
            var posts = GeneratePosts();

            var loggerMock = new Mock<ILogger>();
            var processorService = new PostProcessorService(new TrackerLoggerService(loggerMock.Object));

            //Act
            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);

            var newPost = posts[3];
            newPost.Data.Id = "101";
            posts.Add(newPost);

            await processorService.ProcessNewPostsAsync(SUBREDDIT_NAME, posts);
            var newAuthor = processorService.GetAuthorWithMostPostsSinceMonitoringStart(SUBREDDIT_NAME);

            //Assert
            Assert.AreEqual(newAuthor, $"{newPost.Data.Author} (1)");

        }
    }
}
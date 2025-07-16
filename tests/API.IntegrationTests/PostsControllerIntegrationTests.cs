using System.Net;
using System.Net.Http.Json;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using WebApi;

namespace API.IntegrationTests
{
    public class PostsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PostsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPosts_ShouldReturnSuccess()
        {
            var response = await _client.GetAsync("/api/posts");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
            posts.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPostById_ShouldReturnSuccess_WhenPostExists()
        {
            var postId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetPostById_ShouldReturnNotFound_WhenPostNotExists()
        {
            var postId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreatePost_ShouldReturnSuccess_WhenValidData()
        {
            var createPostDto = new CreatePostDto
            {
                Content = "Test post content"
            };

            var response = await _client.PostAsJsonAsync("/api/posts", createPostDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<PostDto>();
            result.Should().NotBeNull();
            result!.Content.Should().Be(createPostDto.Content);
        }

        [Fact]
        public async Task CreatePost_ShouldReturnBadRequest_WhenContentTooLong()
        {
            var createPostDto = new CreatePostDto
            {
                Content = new string('a', 281)
            };

            var response = await _client.PostAsJsonAsync("/api/posts", createPostDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdatePost_ShouldReturnSuccess_WhenValidData()
        {
            var postId = Guid.NewGuid();
            var updatePostDto = new UpdatePostDto
            {
                Content = "Updated post content"
            };

            var response = await _client.PutAsJsonAsync($"/api/posts/{postId}", updatePostDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdatePost_ShouldReturnNotFound_WhenPostNotExists()
        {
            var postId = Guid.NewGuid();
            var updatePostDto = new UpdatePostDto
            {
                Content = "Updated content"
            };

            var response = await _client.PutAsJsonAsync($"/api/posts/{postId}", updatePostDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeletePost_ShouldReturnSuccess_WhenPostExists()
        {
            var postId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeletePost_ShouldReturnNotFound_WhenPostNotExists()
        {
            var postId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPostsByUser_ShouldReturnSuccess()
        {
            var userId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/posts/user/{userId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
            posts.Should().NotBeNull();
        }

        [Fact]
        public async Task LikePost_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.PostAsync($"/api/posts/{postId}/like", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UnlikePost_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/posts/{postId}/like");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetLikes_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/posts/{postId}/likes");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var likes = await response.Content.ReadFromJsonAsync<List<LikeDto>>();
            likes.Should().NotBeNull();
        }

        [Fact]
        public async Task AddComment_ShouldReturnSuccess_WhenValidData()
        {
            var postId = Guid.NewGuid();
            var createCommentDto = new CreateCommentDto
            {
                Content = "Test comment",
                ParentCommentId = null
            };

            var response = await _client.PostAsJsonAsync($"/api/posts/{postId}/comments", createCommentDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<CommentDto>();
            result.Should().NotBeNull();
            result!.Content.Should().Be(createCommentDto.Content);
        }

        [Fact]
        public async Task GetComments_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/posts/{postId}/comments");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
            comments.Should().NotBeNull();
        }

        [Fact]
        public async Task Repost_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();
            // В текущей архитектуре репост создаётся через сервис, а не через DTO
            // Здесь можно просто отправить POST-запрос без тела или с простым объектом
            var response = await _client.PostAsync($"/api/posts/{postId}/repost", null);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task AddBookmark_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.PostAsync($"/api/posts/{postId}/bookmark", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveBookmark_ShouldReturnSuccess()
        {
            var postId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/posts/{postId}/bookmark");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
using Application.Common.DTOs;
using Domain.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public PostService(IPostRepository postRepository, IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllAsync();
            return posts.Select(MapToDto);
        }

        public async Task<PostDto?> GetPostByIdAsync(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            return post != null ? MapToDto(post) : null;
        }

        public async Task<IEnumerable<PostDto>> GetPostsByAuthorIdAsync(Guid authorId)
        {
            var posts = await _postRepository.GetByAuthorIdAsync(authorId);
            return posts.Select(MapToDto);
        }

        public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto createPostDto)
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
            {
                throw new InvalidOperationException("Пользователь не найден");
            }

            var hashtags = ExtractHashtags(createPostDto.Content);

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Content = createPostDto.Content,
                AuthorId = authorId,
                MediaUrls = createPostDto.MediaUrls,
                Hashtags = hashtags,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postRepository.CreateAsync(post);
            return MapToDto(createdPost);
        }

        public async Task<PostDto> UpdatePostAsync(Guid id, Guid authorId, UpdatePostDto updatePostDto)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                throw new InvalidOperationException("Пост не найден");
            }

            if (post.AuthorId != authorId)
            {
                throw new InvalidOperationException("Вы не можете редактировать чужой пост");
            }

            post.Content = updatePostDto.Content;
            post.MediaUrls = updatePostDto.MediaUrls;
            post.Hashtags = ExtractHashtags(updatePostDto.Content);
            post.UpdatedAt = DateTime.UtcNow;

            var updatedPost = await _postRepository.UpdateAsync(post);
            return MapToDto(updatedPost);
        }

        public async Task DeletePostAsync(Guid id, Guid authorId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                throw new InvalidOperationException("Пост не найден");
            }

            if (post.AuthorId != authorId)
            {
                throw new InvalidOperationException("Вы не можете удалить чужой пост");
            }

            await _postRepository.DeleteAsync(id);
        }

        private static PostDto MapToDto(Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                AuthorId = post.AuthorId,
                AuthorUsername = post.Author?.Username ?? "Неизвестный пользователь",
                MediaUrls = post.MediaUrls,
                Hashtags = post.Hashtags,
                LikeCount = post.LikeCount,
                RepostCount = post.RepostCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }

        private static string? ExtractHashtags(string content)
        {
            var hashtags = new List<string>();
            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (word.StartsWith('#'))
                {
                    hashtags.Add(word.TrimStart('#'));
                }
            }

            return hashtags.Count > 0 ? string.Join(",", hashtags) : null;
        }
    }
}
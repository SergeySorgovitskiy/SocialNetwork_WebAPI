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

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(CancellationToken cancellationToken)
        {
            var posts = await _postRepository.GetAllAsync(cancellationToken);
            return posts.Select(MapToDto);
        }

        public async Task<PostDto?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            return post != null ? MapToDto(post) : null;
        }

        public async Task<IEnumerable<PostDto>> GetPostsByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken)
        {
            var posts = await _postRepository.GetByAuthorIdAsync(authorId, cancellationToken);
            return posts.Select(MapToDto);
        }

        public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto createPostDto, CancellationToken cancellationToken)
        {
            var author = await _userRepository.GetByIdAsync(authorId, cancellationToken);
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
                MediaUrls = createPostDto.MediaUrls ?? new HashSet<string>(),
                Hashtags = hashtags,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postRepository.CreateAsync(post, cancellationToken);
            return MapToDto(createdPost);
        }

        public async Task<PostDto> UpdatePostAsync(Guid id, Guid authorId, UpdatePostDto updatePostDto, CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                throw new InvalidOperationException("Пост не найден");
            }

            if (post.AuthorId != authorId)
            {
                throw new InvalidOperationException("Вы не можете редактировать чужой пост");
            }

            post.Content = updatePostDto.Content;
            post.MediaUrls = updatePostDto.MediaUrls ?? new HashSet<string>();
            post.Hashtags = ExtractHashtags(updatePostDto.Content);
            post.UpdatedAt = DateTime.UtcNow;

            var updatedPost = await _postRepository.UpdateAsync(post, cancellationToken);
            return MapToDto(updatedPost);
        }

        public async Task DeletePostAsync(Guid id, Guid authorId, CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                throw new InvalidOperationException("Пост не найден");
            }

            if (post.AuthorId != authorId)
            {
                throw new InvalidOperationException("Вы не можете удалить чужой пост");
            }

            await _postRepository.DeleteAsync(id, cancellationToken);
        }

        private static PostDto MapToDto(Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                AuthorId = post.AuthorId,
                AuthorUserName = post.Author?.UserName ?? "Неизвестный пользователь",
                MediaUrls = post.MediaUrls ?? new HashSet<string>(),
                Hashtags = post.Hashtags ?? new HashSet<string>(),
                RepostCount = post.RepostCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }

        private static ICollection<string> ExtractHashtags(string content)
        {
            var hashtags = new HashSet<string>();
            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (word.StartsWith('#'))
                {
                    hashtags.Add(word.TrimStart('#'));
                }
            }

            return hashtags;
        }
    }
}
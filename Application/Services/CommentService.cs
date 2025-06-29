using Application.Common.DTOs;
using Domain.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class CommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public CommentService(ICommentRepository commentRepository, IUserRepository userRepository, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync()
        {
            var comments = await _commentRepository.GetAllAsync();
            return comments.Select(MapToDto);
        }

        public async Task<CommentDto?> GetCommentByIdAsync(Guid id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            return comment != null ? MapToDto(comment) : null;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(Guid postId)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            return comments.Select(MapToDto);
        }

        public async Task<CommentDto> CreateCommentAsync(Guid authorId, CreateCommentDto createCommentDto)
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                throw new InvalidOperationException("Пользователь не найден");

            var post = await _postRepository.GetByIdAsync(createCommentDto.PostId);
            if (post == null)
                throw new InvalidOperationException("Пост не найден");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = createCommentDto.Content,
                PostId = createCommentDto.PostId,
                AuthorId = authorId,
                ParentCommentId = createCommentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.CreateAsync(comment);
            return MapToDto(createdComment);
        }

        public async Task<CommentDto> UpdateCommentAsync(Guid id, Guid authorId, UpdateCommentDto updateCommentDto)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new InvalidOperationException("Комментарий не найден");

            if (comment.AuthorId != authorId)
                throw new InvalidOperationException("Вы не можете редактировать чужой комментарий");

            comment.Content = updateCommentDto.Content;

            var updatedComment = await _commentRepository.UpdateAsync(comment);
            return MapToDto(updatedComment);
        }

        public async Task DeleteCommentAsync(Guid id, Guid authorId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new InvalidOperationException("Комментарий не найден");

            if (comment.AuthorId != authorId)
                throw new InvalidOperationException("Вы не можете удалить чужой комментарий");

            await _commentRepository.DeleteAsync(id);
        }

        private static CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                AuthorUsername = comment.Author?.Username ?? "Неизвестный пользователь",
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
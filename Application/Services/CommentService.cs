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
        private const int MaxNestingLevel = 3; // Максимальная вложенность комментариев

        public CommentService(ICommentRepository commentRepository, IUserRepository userRepository, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync(CancellationToken cancellationToken)
        {
            var comments = await _commentRepository.GetAllAsync(cancellationToken);
            return comments.Select(MapToDto);
        }

        public async Task<CommentDto?> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            return comment != null ? MapToDto(comment) : null;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(Guid postId, CancellationToken cancellationToken)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken);
            return comments.Select(MapToDto);
        }

        public async Task<CommentDto> CreateCommentAsync(Guid authorId, CreateCommentDto createCommentDto, CancellationToken cancellationToken)
        {
            var author = await _userRepository.GetByIdAsync(authorId, cancellationToken);
            if (author == null)
                throw new InvalidOperationException("Пользователь не найден");

            var post = await _postRepository.GetByIdAsync(createCommentDto.PostId, cancellationToken);
            if (post == null)
                throw new InvalidOperationException("Пост не найден");

            // Проверяем вложенность комментариев
            int nestingLevel = 0;
            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(createCommentDto.ParentCommentId.Value, cancellationToken);
                if (parentComment == null)
                    throw new InvalidOperationException("Родительский комментарий не найден");

                nestingLevel = parentComment.NestingLevel + 1;
                if (nestingLevel > MaxNestingLevel)
                    throw new InvalidOperationException($"Максимальная вложенность комментариев составляет {MaxNestingLevel} уровней");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = createCommentDto.Content,
                PostId = createCommentDto.PostId,
                AuthorId = authorId,
                ParentCommentId = createCommentDto.ParentCommentId,
                NestingLevel = nestingLevel,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.CreateAsync(comment, cancellationToken);
            return MapToDto(createdComment);
        }

        public async Task<CommentDto> UpdateCommentAsync(Guid id, Guid authorId, UpdateCommentDto updateCommentDto, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
                throw new InvalidOperationException("Комментарий не найден");

            if (comment.AuthorId != authorId)
                throw new InvalidOperationException("Вы не можете редактировать чужой комментарий");

            comment.Content = updateCommentDto.Content;

            var updatedComment = await _commentRepository.UpdateAsync(comment, cancellationToken);
            return MapToDto(updatedComment);
        }

        public async Task DeleteCommentAsync(Guid id, Guid authorId, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
                throw new InvalidOperationException("Комментарий не найден");

            if (comment.AuthorId != authorId)
                throw new InvalidOperationException("Вы не можете удалить чужой комментарий");

            await _commentRepository.DeleteAsync(id, cancellationToken);
        }

        private static CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                AuthorUserName = comment.Author?.UserName ?? "Неизвестный пользователь",
                ParentCommentId = comment.ParentCommentId,
                NestingLevel = comment.NestingLevel,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
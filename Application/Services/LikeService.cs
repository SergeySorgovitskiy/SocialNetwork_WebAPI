using Application.Common.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Application.Services
{
    public interface ILikeService
    {
        Task<bool> AddLikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
        Task<bool> RemoveLikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
        Task<int> GetLikesCountAsync(Guid postId, CancellationToken cancellationToken);
        Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
        Task<List<LikeDto>> GetLikesForPostAsync(Guid postId, CancellationToken cancellationToken);
    }

    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        public LikeService(ILikeRepository likeRepository, IUserRepository userRepository, IPostRepository postRepository)
        {
            _likeRepository = likeRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
        }
        public async Task<bool> AddLikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
            if (post == null) throw new InvalidOperationException("Пост не найден");

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new InvalidOperationException("Пользователь не найден");

            if (await _likeRepository.ExistsAsync(postId, userId, cancellationToken)) return false;

            var like = new Like
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
            await _likeRepository.AddAsync(like, cancellationToken);
            return true;
        }
        public async Task<bool> RemoveLikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
        {
            if (!await _likeRepository.ExistsAsync(postId, userId, cancellationToken))
                return false;

            await _likeRepository.RemoveAsync(postId, userId, cancellationToken);
            return true;
        }
        public async Task<int> GetLikesCountAsync(Guid postId, CancellationToken cancellationToken)
        {
            return await _likeRepository.GetLikesCountAsync(postId, cancellationToken);
        }

        public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
        {
            return await _likeRepository.ExistsAsync(postId, userId, cancellationToken);
        }

        public async Task<List<LikeDto>> GetLikesForPostAsync(Guid postId, CancellationToken cancellationToken)
        {
            var likes = await _likeRepository.GetLikesForPostAsync(postId, cancellationToken);
            return likes.Select(MapToDto).ToList();
        }

        private static LikeDto MapToDto(Like like)
        {
            return new LikeDto
            {
                Id = like.Id,
                PostId = like.PostId,
                UserId = like.UserId,
                CreatedAt = like.CreatedAt
            };
        }
    }
}

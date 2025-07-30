using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public interface IRepostService
{
    Task<Repost> CreateRepostAsync(Guid userId, Guid postId, string? comment, CancellationToken cancellationToken = default);
    Task DeleteRepostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Repost>> GetUserRepostsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Repost>> GetPostRepostsAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<bool> HasUserRepostedAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<int> GetRepostCountAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Repost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class RepostService : IRepostService
{
    private readonly IRepostRepository _repostRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public RepostService(IRepostRepository repostRepository, IPostRepository postRepository, IUserRepository userRepository)
    {
        _repostRepository = repostRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<Repost> CreateRepostAsync(Guid userId, Guid postId, string? comment, CancellationToken cancellationToken = default)
    {

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new ArgumentException("Пользователь не найден");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post == null)
            throw new ArgumentException("Пост не найден");

        var existingRepost = await _repostRepository.GetByUserAndPostAsync(userId, postId, cancellationToken);
        if (existingRepost != null)
            throw new InvalidOperationException("Пользователь уже репостил этот пост");

        var repost = new Repost
        {
            UserId = userId,
            OriginalPostId = postId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        var createdRepost = await _repostRepository.AddAsync(repost, cancellationToken);

        post.RepostCount = await _repostRepository.GetRepostCountAsync(postId, cancellationToken);
        await _postRepository.UpdateAsync(post, cancellationToken);

        return createdRepost;
    }

    public async Task DeleteRepostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var repost = await _repostRepository.GetByUserAndPostAsync(userId, postId, cancellationToken);
        if (repost == null)
            throw new ArgumentException("Репост не найден");

        await _repostRepository.DeleteAsync(repost.Id, cancellationToken);

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post != null)
        {
            post.RepostCount = await _repostRepository.GetRepostCountAsync(postId, cancellationToken);
            await _postRepository.UpdateAsync(post, cancellationToken);
        }
    }

    public async Task<IEnumerable<Repost>> GetUserRepostsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _repostRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Repost>> GetPostRepostsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _repostRepository.GetByPostIdAsync(postId, cancellationToken);
    }

    public async Task<bool> HasUserRepostedAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _repostRepository.ExistsAsync(userId, postId, cancellationToken);
    }

    public async Task<int> GetRepostCountAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _repostRepository.GetRepostCountAsync(postId, cancellationToken);
    }

    public async Task<Repost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repostRepository.GetByIdAsync(id, cancellationToken);
    }
}
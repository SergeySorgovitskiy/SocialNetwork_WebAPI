using Domain.Entities;
using Domain.Interfaces;
using Domain.Common;

namespace Application.Services;

public class NewsFeedService : INewsFeedService
{
    private readonly IPostRepository _postRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;

    public NewsFeedService(IPostRepository postRepository, ISubscriptionRepository subscriptionRepository, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
    }

    public async Task<NewsFeedResult> GetPersonalFeedAsync(Guid userId, NewsFeedFilter filter, CancellationToken cancellationToken = default)
    {

        var subscriptions = await _subscriptionRepository.GetUserSubscriptionsAsync(userId, cancellationToken);
        var followingIds = subscriptions.Select(s => s.FollowingId).ToList();


        followingIds.Add(userId);


        var posts = await _postRepository.GetPostsByUserIdsAsync(followingIds, filter.Page, filter.PageSize, cancellationToken);
        var totalCount = await _postRepository.GetPostsCountByUserIdsAsync(followingIds, cancellationToken);


        var filteredPosts = ApplyFilters(posts, filter);

        return new NewsFeedResult
        {
            Posts = filteredPosts,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    public async Task<NewsFeedResult> GetGlobalFeedAsync(NewsFeedFilter filter, CancellationToken cancellationToken = default)
    {

        var posts = await _postRepository.GetAllAsync(filter.Page, filter.PageSize, cancellationToken);
        var totalCount = await _postRepository.GetTotalCountAsync(cancellationToken);


        var filteredPosts = ApplyFilters(posts, filter);

        return new NewsFeedResult
        {
            Posts = filteredPosts,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    public async Task<NewsFeedResult> GetUserFeedAsync(Guid userId, Guid targetUserId, NewsFeedFilter filter, CancellationToken cancellationToken = default)
    {

        var targetUser = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (targetUser == null)
            throw new ArgumentException("Пользователь не найден");


        if (targetUser.IsPrivate)
        {
            var isFollowing = await _subscriptionRepository.IsSubscribedAsync(userId, targetUserId, cancellationToken);
            if (!isFollowing && userId != targetUserId)
                throw new UnauthorizedAccessException("Профиль пользователя является приватным");
        }


        var posts = await _postRepository.GetByUserIdAsync(targetUserId, filter.Page, filter.PageSize, cancellationToken);
        var totalCount = await _postRepository.GetCountByUserIdAsync(targetUserId, cancellationToken);


        var filteredPosts = ApplyFilters(posts, filter);

        return new NewsFeedResult
        {
            Posts = filteredPosts,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    private IEnumerable<Post> ApplyFilters(IEnumerable<Post> posts, NewsFeedFilter filter)
    {
        var filteredPosts = posts.AsEnumerable();


        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            filteredPosts = filteredPosts.Where(p =>
                p.Content.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Hashtags.Any(h => h.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase)));
        }


        if (!string.IsNullOrWhiteSpace(filter.Hashtag))
        {
            filteredPosts = filteredPosts.Where(p =>
                p.Hashtags.Any(h => h.Equals(filter.Hashtag, StringComparison.OrdinalIgnoreCase)));
        }


        if (filter.FromDate.HasValue)
        {
            filteredPosts = filteredPosts.Where(p => p.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            filteredPosts = filteredPosts.Where(p => p.CreatedAt <= filter.ToDate.Value);
        }


        if (!filter.IncludeReposts)
        {
            filteredPosts = filteredPosts.Where(p => p.RepostCount == 0);
        }


        return filteredPosts.OrderByDescending(p => p.CreatedAt);
    }
}
using Domain.Common;

namespace Domain.Interfaces;

public interface INewsFeedService
{
    Task<NewsFeedResult> GetPersonalFeedAsync(Guid userId, NewsFeedFilter filter, CancellationToken cancellationToken = default);
    Task<NewsFeedResult> GetGlobalFeedAsync(NewsFeedFilter filter, CancellationToken cancellationToken = default);
    Task<NewsFeedResult> GetUserFeedAsync(Guid userId, Guid targetUserId, NewsFeedFilter filter, CancellationToken cancellationToken = default);
}
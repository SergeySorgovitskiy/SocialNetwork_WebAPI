using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public interface IBookmarkService
{
    Task<Bookmark> AddBookmarkAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task RemoveBookmarkAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bookmark>> GetUserBookmarksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsBookmarkedAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public BookmarkService(IBookmarkRepository bookmarkRepository, IPostRepository postRepository, IUserRepository userRepository)
    {
        _bookmarkRepository = bookmarkRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<Bookmark> AddBookmarkAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new ArgumentException("Пользователь не найден");


        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post == null)
            throw new ArgumentException("Пост не найден");


        var existingBookmark = await _bookmarkRepository.GetByUserAndPostAsync(userId, postId, cancellationToken);
        if (existingBookmark != null)
            throw new InvalidOperationException("Пост уже добавлен в закладки");


        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };

        return await _bookmarkRepository.AddAsync(bookmark, cancellationToken);
    }

    public async Task RemoveBookmarkAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var bookmark = await _bookmarkRepository.GetByUserAndPostAsync(userId, postId, cancellationToken);
        if (bookmark == null)
            throw new ArgumentException("Закладка не найдена");

        await _bookmarkRepository.DeleteByUserAndPostAsync(userId, postId, cancellationToken);
    }

    public async Task<IEnumerable<Bookmark>> GetUserBookmarksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<bool> IsBookmarkedAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepository.ExistsAsync(userId, postId, cancellationToken);
    }

    public async Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepository.GetByIdAsync(id, cancellationToken);
    }
}
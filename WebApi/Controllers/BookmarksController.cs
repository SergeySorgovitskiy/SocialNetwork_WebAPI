using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;

    public BookmarksController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    [HttpPost]
    public async Task<IActionResult> AddBookmark([FromBody] AddBookmarkDto addBookmarkDto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var bookmark = await _bookmarkService.AddBookmarkAsync(userId, addBookmarkDto.PostId, cancellationToken);

            return CreatedAtAction(nameof(GetBookmark), new { id = bookmark.Id }, bookmark);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> RemoveBookmark(Guid postId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _bookmarkService.RemoveBookmarkAsync(userId, postId, cancellationToken);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserBookmarks(Guid userId, CancellationToken cancellationToken)
    {
        var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId, cancellationToken);
        return Ok(bookmarks);
    }

    [HttpGet("check/{postId}")]
    public async Task<IActionResult> CheckBookmarked(Guid postId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, postId, cancellationToken);

        return Ok(new { IsBookmarked = isBookmarked });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookmark(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var bookmark = await _bookmarkService.GetByIdAsync(id, cancellationToken);
            if (bookmark == null)
            {
                return NotFound("Закладка не найдена");
            }

            return Ok(bookmark);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при получении закладки", error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Недействительный токен пользователя");
        }
        return userId;
    }
}

public class AddBookmarkDto
{
    public Guid PostId { get; set; }
}
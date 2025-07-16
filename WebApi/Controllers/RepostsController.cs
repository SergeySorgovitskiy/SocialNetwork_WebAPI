using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepostsController : ControllerBase
{
    private readonly IRepostService _repostService;

    public RepostsController(IRepostService repostService)
    {
        _repostService = repostService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRepost([FromBody] CreateRepostDto createRepostDto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var repost = await _repostService.CreateRepostAsync(userId, createRepostDto.PostId, createRepostDto.Comment, cancellationToken);

            return CreatedAtAction(nameof(GetRepost), new { id = repost.Id }, repost);
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
    public async Task<IActionResult> DeleteRepost(Guid postId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _repostService.DeleteRepostAsync(userId, postId, cancellationToken);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReposts(Guid userId, CancellationToken cancellationToken)
    {
        var reposts = await _repostService.GetUserRepostsAsync(userId, cancellationToken);
        return Ok(reposts);
    }

    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetPostReposts(Guid postId, CancellationToken cancellationToken)
    {
        var reposts = await _repostService.GetPostRepostsAsync(postId, cancellationToken);
        return Ok(reposts);
    }

    [HttpGet("check/{postId}")]
    public async Task<IActionResult> CheckUserReposted(Guid postId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var hasReposted = await _repostService.HasUserRepostedAsync(userId, postId, cancellationToken);

        return Ok(new { HasReposted = hasReposted });
    }

    [HttpGet("count/{postId}")]
    public async Task<IActionResult> GetRepostCount(Guid postId, CancellationToken cancellationToken)
    {
        var count = await _repostService.GetRepostCountAsync(postId, cancellationToken);
        return Ok(new { RepostCount = count });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRepost(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var repost = await _repostService.GetByIdAsync(id, cancellationToken);
            if (repost == null)
            {
                return NotFound("Репост не найден");
            }

            return Ok(repost);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при получении репоста", error = ex.Message });
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

public class CreateRepostDto
{
    public Guid PostId { get; set; }
    public string? Comment { get; set; }
}
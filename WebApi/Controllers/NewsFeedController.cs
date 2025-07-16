using Domain.Interfaces;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewsFeedController : ControllerBase
{
    private readonly INewsFeedService _newsFeedService;

    public NewsFeedController(INewsFeedService newsFeedService)
    {
        _newsFeedService = newsFeedService;
    }

    [HttpGet("personal")]
    public async Task<IActionResult> GetPersonalFeed([FromQuery] NewsFeedFilterDto filter, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var newsFeedFilter = MapToNewsFeedFilter(filter);
            var result = await _newsFeedService.GetPersonalFeedAsync(userId, newsFeedFilter, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("global")]
    public async Task<IActionResult> GetGlobalFeed([FromQuery] NewsFeedFilterDto filter, CancellationToken cancellationToken)
    {
        try
        {
            var newsFeedFilter = MapToNewsFeedFilter(filter);
            var result = await _newsFeedService.GetGlobalFeedAsync(newsFeedFilter, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{targetUserId}")]
    public async Task<IActionResult> GetUserFeed(Guid targetUserId, [FromQuery] NewsFeedFilterDto filter, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var newsFeedFilter = MapToNewsFeedFilter(filter);
            var result = await _newsFeedService.GetUserFeedAsync(userId, targetUserId, newsFeedFilter, cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
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

    private static NewsFeedFilter MapToNewsFeedFilter(NewsFeedFilterDto dto)
    {
        return new NewsFeedFilter
        {
            Page = dto.Page,
            PageSize = dto.PageSize,
            SearchQuery = dto.SearchQuery,
            Hashtag = dto.Hashtag,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            IncludeReposts = dto.IncludeReposts,
            IncludeComments = dto.IncludeComments
        };
    }
}

public class NewsFeedFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? Hashtag { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeReposts { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.DTOs;
using Application.Services;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost]
        public async Task<IActionResult> LikePost([FromBody] LikeCreateDto dto, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _likeService.AddLikeAsync(dto.PostId, userId, cancellationToken);
            if (!result) return BadRequest("Лайк уже существует");
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> UnlikePost([FromBody] LikeDeleteDto dto, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _likeService.RemoveLikeAsync(dto.PostId, userId, cancellationToken);
            if (!result) return NotFound("Лайк не найден");
            return Ok();
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetLikesForPost(Guid postId, CancellationToken cancellationToken)
        {
            var likes = await _likeService.GetLikesForPostAsync(postId, cancellationToken);
            return Ok(likes);
        }

        [HttpGet("post/{postId}/count")]
        public async Task<IActionResult> GetLikesCount(Guid postId, CancellationToken cancellationToken)
        {
            var count = await _likeService.GetLikesCountAsync(postId, cancellationToken);
            return Ok(count);
        }
    }
}

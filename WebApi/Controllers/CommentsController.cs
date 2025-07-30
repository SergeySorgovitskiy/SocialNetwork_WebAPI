using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments(CancellationToken cancellationToken)
        {
            try
            {
                var comments = await _commentService.GetAllCommentsAsync(cancellationToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении комментариев", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetCommentById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id, cancellationToken);
                if (comment == null)
                {
                    return NotFound(new { message = "Комментарий не найден" });
                }
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении комментария", error = ex.Message });
            }
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPost(Guid postId, CancellationToken cancellationToken)
        {
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId, cancellationToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении комментариев поста", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto createCommentDto, [FromQuery] Guid authorId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(createCommentDto.Content))
                {
                    return BadRequest(new { message = "Контент комментария обязателен" });
                }

                if (createCommentDto.Content.Length > 200)
                {
                    return BadRequest(new { message = "Контент комментария не может превышать 200 символов" });
                }

                var comment = await _commentService.CreateCommentAsync(authorId, createCommentDto, cancellationToken);
                return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при создании комментария", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, [FromBody] UpdateCommentDto updateCommentDto, [FromQuery] Guid authorId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(updateCommentDto.Content))
                {
                    return BadRequest(new { message = "Контент комментария обязателен" });
                }

                if (updateCommentDto.Content.Length > 200)
                {
                    return BadRequest(new { message = "Контент комментария не может превышать 200 символов" });
                }

                var comment = await _commentService.UpdateCommentAsync(id, authorId, updateCommentDto, cancellationToken);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении комментария", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComment(Guid id, [FromQuery] Guid authorId, CancellationToken cancellationToken)
        {
            try
            {
                await _commentService.DeleteCommentAsync(id, authorId, cancellationToken);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при удалении комментария", error = ex.Message });
            }
        }
    }
}
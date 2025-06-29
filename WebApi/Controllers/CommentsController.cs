using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetCommentById(Guid id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPost(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto createCommentDto, [FromQuery] Guid authorId)
        {
            var comment = await _commentService.CreateCommentAsync(authorId, createCommentDto);
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, UpdateCommentDto updateCommentDto, [FromQuery] Guid authorId)
        {
            var comment = await _commentService.UpdateCommentAsync(id, authorId, updateCommentDto);
            return Ok(comment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComment(Guid id, [FromQuery] Guid authorId)
        {
            await _commentService.DeleteCommentAsync(id, authorId);
            return NoContent();
        }
    }
}
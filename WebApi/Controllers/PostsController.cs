using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;

        public PostsController(PostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPosts()
        {
            try
            {
                var posts = await _postService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении постов", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPostById(Guid id)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "Пост не найден" });
                }
                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении поста", error = ex.Message });
            }
        }

        [HttpGet("user/{authorId}")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByAuthor(Guid authorId)
        {
            try
            {
                var posts = await _postService.GetPostsByAuthorIdAsync(authorId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении постов пользователя", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto, [FromQuery] Guid authorId)
        {
            try
            {
                if (string.IsNullOrEmpty(createPostDto.Content))
                {
                    return BadRequest(new { message = "Контент поста обязателен" });
                }

                if (createPostDto.Content.Length > 280)
                {
                    return BadRequest(new { message = "Контент поста не может превышать 280 символов" });
                }

                var post = await _postService.CreatePostAsync(authorId, createPostDto);
                return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при создании поста", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> UpdatePost(Guid id, [FromBody] UpdatePostDto updatePostDto, [FromQuery] Guid authorId)
        {
            try
            {
                if (string.IsNullOrEmpty(updatePostDto.Content))
                {
                    return BadRequest(new { message = "Контент поста обязателен" });
                }

                if (updatePostDto.Content.Length > 280)
                {
                    return BadRequest(new { message = "Контент поста не может превышать 280 символов" });
                }

                var post = await _postService.UpdatePostAsync(id, authorId, updatePostDto);
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении поста", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePost(Guid id, [FromQuery] Guid authorId)
        {
            try
            {
                await _postService.DeletePostAsync(id, authorId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при удалении поста", error = ex.Message });
            }
        }
    }
}
using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionsController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAllSubscriptions(CancellationToken cancellationToken)
        {
            try
            {
                var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync(cancellationToken);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении подписок", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscriptionById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionByIdAsync(id, cancellationToken);
                if (subscription == null)
                {
                    return NotFound(new { message = "Подписка не найдена" });
                }
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении подписки", error = ex.Message });
            }
        }

        [HttpGet("followers/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetFollowers(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var followers = await _subscriptionService.GetFollowersAsync(userId, cancellationToken);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении подписчиков", error = ex.Message });
            }
        }

        [HttpGet("following/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetFollowing(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var following = await _subscriptionService.GetFollowingAsync(userId, cancellationToken);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении подписок пользователя", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto createSubscriptionDto, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDto, cancellationToken);
                return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id }, subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при создании подписки", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionDto>> UpdateSubscription(Guid id, UpdateSubscriptionDto updateSubscriptionDto, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(updateSubscriptionDto.Status))
                {
                    return BadRequest(new { message = "Статус подписки обязателен" });
                }

                var subscription = await _subscriptionService.UpdateSubscriptionAsync(id, updateSubscriptionDto, cancellationToken);
                return Ok(subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении подписки", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubscription(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _subscriptionService.DeleteSubscriptionAsync(id, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при удалении подписки", error = ex.Message });
            }
        }
    }
}
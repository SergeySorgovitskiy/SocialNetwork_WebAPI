using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionsController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAllSubscriptions()
        {
            var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
            return Ok(subscriptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscriptionById(Guid id)
        {
            var subscription = await _subscriptionService.GetSubscriptionByIdAsync(id);
            if (subscription == null)
                return NotFound();
            return Ok(subscription);
        }

        [HttpGet("followers/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetFollowers(Guid userId)
        {
            var followers = await _subscriptionService.GetFollowersAsync(userId);
            return Ok(followers);
        }

        [HttpGet("following/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetFollowing(Guid userId)
        {
            var following = await _subscriptionService.GetFollowingAsync(userId);
            return Ok(following);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto createSubscriptionDto)
        {
            var subscription = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDto);
            return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id }, subscription);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionDto>> UpdateSubscription(Guid id, UpdateSubscriptionDto updateSubscriptionDto)
        {
            var subscription = await _subscriptionService.UpdateSubscriptionAsync(id, updateSubscriptionDto);
            return Ok(subscription);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubscription(Guid id)
        {
            await _subscriptionService.DeleteSubscriptionAsync(id);
            return NoContent();
        }
    }
}
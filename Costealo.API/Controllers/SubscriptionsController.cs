using Costealo.API.Data;
using Costealo.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubscriptionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Subscriptions/me
    [HttpGet("me")]
    public async Task<ActionResult<Subscription>> GetMySubscription()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (subscription == null)
        {
            // Return default Free subscription if none exists
            return Ok(new Subscription
            {
                UserId = userId,
                PlanType = SubscriptionPlan.Free,
                IsActive = true,
                StartDate = DateTime.UtcNow
            });
        }

        return Ok(subscription);
    }

    // POST: api/Subscriptions
    [HttpPost]
    public async Task<ActionResult<Subscription>> CreateSubscription(CreateSubscriptionDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if user already has an active subscription
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (existingSubscription != null)
        {
            return BadRequest("User already has an active subscription. Use PUT to update it.");
        }

        var subscription = new Subscription
        {
            UserId = userId,
            PlanType = request.PlanType,
            StartDate = DateTime.UtcNow,
            IsActive = true,
            CardLastFourDigits = request.CardLastFourDigits,
            CardHolderName = request.CardHolderName,
            ExpirationDate = request.ExpirationDate,
            PaymentMethodType = request.PaymentMethodType
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMySubscription), subscription);
    }

    // PUT: api/Subscriptions/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(int id, UpdateSubscriptionDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null || subscription.UserId != userId)
        {
            return NotFound();
        }

        subscription.PlanType = request.PlanType;
        subscription.IsActive = request.IsActive;

        if (request.EndDate.HasValue)
        {
            subscription.EndDate = request.EndDate.Value;
        }

        // Update payment info if provided
        if (!string.IsNullOrWhiteSpace(request.CardLastFourDigits))
            subscription.CardLastFourDigits = request.CardLastFourDigits;
        if (!string.IsNullOrWhiteSpace(request.CardHolderName))
            subscription.CardHolderName = request.CardHolderName;
        if (!string.IsNullOrWhiteSpace(request.ExpirationDate))
            subscription.ExpirationDate = request.ExpirationDate;
        if (!string.IsNullOrWhiteSpace(request.PaymentMethodType))
            subscription.PaymentMethodType = request.PaymentMethodType;

        _context.Entry(subscription).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SubscriptionExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Subscriptions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelSubscription(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null || subscription.UserId != userId)
        {
            return NotFound();
        }

        // Don't delete, just deactivate
        subscription.IsActive = false;
        subscription.EndDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SubscriptionExists(int id)
    {
        return _context.Subscriptions.Any(e => e.Id == id);
    }
}

public class CreateSubscriptionDto
{
    public SubscriptionPlan PlanType { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? PaymentMethodType { get; set; }
}

public class UpdateSubscriptionDto
{
    public SubscriptionPlan PlanType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? PaymentMethodType { get; set; }
}

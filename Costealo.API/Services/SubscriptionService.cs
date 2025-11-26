using Costealo.API.Data;
using Costealo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Costealo.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;

    public SubscriptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription> GetUserSubscription(int userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (subscription == null)
        {
            // Create default Free subscription if none exists
            subscription = new Subscription
            {
                UserId = userId,
                PlanType = SubscriptionPlan.Free,
                StartDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        return subscription;
    }

    public async Task<bool> CanCreateWorkbook(int userId)
    {
        var subscription = await GetUserSubscription(userId);
        if (subscription.PlanType == SubscriptionPlan.Premium) return true;

        var count = await _context.Workbooks.CountAsync(w => w.UserId == userId);
        return count < subscription.MaxWorkbooks;
    }

    public async Task<bool> CanCreateDatabase(int userId)
    {
        var subscription = await GetUserSubscription(userId);
        if (subscription.PlanType == SubscriptionPlan.Premium) return true;

        var count = await _context.PriceDatabases.CountAsync(d => d.UserId == userId);
        return count < subscription.MaxDatabases;
    }
}

using Costealo.API.Data;
using Costealo.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Costealo.API.Services;

public interface ISubscriptionService
{
    Task<bool> CanCreateDatabase(int userId);
    Task<Subscription> GetUserSubscription(int userId);
}

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

        // Return default Free plan if no subscription exists
        if (subscription == null)
        {
            return new Subscription
            {
                UserId = userId,
                PlanType = SubscriptionPlan.Free,
                IsActive = true,
                StartDate = DateTime.UtcNow
            };
        }

        return subscription;
    }

    public async Task<bool> CanCreateDatabase(int userId)
    {
        var subscription = await GetUserSubscription(userId);
        
        // Count user's existing databases
        var currentDatabaseCount = await _context.PriceDatabases
            .CountAsync(db => db.Items.Any(item => item.PriceDatabase.Items.Any()));
        
        // For simplicity, we're checking total databases
        // In a real app, you might want to track per-user database ownership
        var userDatabaseCount = currentDatabaseCount; // Simplified for now
        
        return userDatabaseCount < subscription.MaxDatabases;
    }
}

using Costealo.API.Models;

namespace Costealo.API.Services;

public interface ISubscriptionService
{
    Task<bool> CanCreateWorkbook(int userId);
    Task<bool> CanCreateDatabase(int userId);
    Task<Subscription> GetUserSubscription(int userId);
}

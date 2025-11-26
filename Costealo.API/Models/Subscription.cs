using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costealo.API.Models;

public class Subscription
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    public SubscriptionPlan PlanType { get; set; } = SubscriptionPlan.Free;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Calculated properties based on plan
    [NotMapped]
    public int MaxWorkbooks => PlanType switch
    {
        SubscriptionPlan.Free => 5,
        SubscriptionPlan.Basico => 10,
        SubscriptionPlan.Estandar => 25,
        SubscriptionPlan.Premium => int.MaxValue,
        _ => 5
    };

    [NotMapped]
    public int MaxDatabases => PlanType switch
    {
        SubscriptionPlan.Free => 1,
        SubscriptionPlan.Basico => 1,
        SubscriptionPlan.Estandar => 2,
        SubscriptionPlan.Premium => int.MaxValue,
        _ => 1
    };
}

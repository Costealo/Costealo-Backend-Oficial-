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

    // Payment information (for demo purposes only - not validated)
    [MaxLength(4)]
    public string? CardLastFourDigits { get; set; }

    [MaxLength(100)]
    public string? CardHolderName { get; set; }

    [MaxLength(7)] // Format: MM/YYYY
    public string? ExpirationDate { get; set; }

    [MaxLength(50)]
    public string? PaymentMethodType { get; set; } // "Tarjeta de débito" or "Tarjeta de crédito"

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

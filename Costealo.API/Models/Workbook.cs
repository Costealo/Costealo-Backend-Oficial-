using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Costealo.API.Models;

public class Workbook
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // Inputs defined by User
    [Column(TypeName = "decimal(18,4)")]
    public decimal ProductionUnits { get; set; } = 1; // "Cantidad de ración"

    [Column(TypeName = "decimal(18,4)")]
    public decimal TaxPercentage { get; set; } = 0.16m; // Default 16%

    [Column(TypeName = "decimal(18,4)")]
    public decimal ProfitMarginPercentage { get; set; } = 0.20m; // Default 20%

    // Bidirectional Fields
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TargetSalePrice { get; set; } // Optional target price

    // Overhead
    [Column(TypeName = "decimal(18,4)")]
    public decimal OperationalCostPercentage { get; set; } = 0.20m; // Default 20%

    [Column(TypeName = "decimal(18,2)")]
    public decimal OperationalCostFixed { get; set; } = 0; // NOT USED - Reserved for future use

    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    [JsonIgnore] // Prevent exposing user data (email, passwordHash) in responses
    public User User { get; set; } = null!;

    public EntityStatus Status { get; set; } = EntityStatus.Draft;

    public List<WorkbookItem> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

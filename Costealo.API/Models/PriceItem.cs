using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costealo.API.Models;

public class PriceItem
{
    [Key]
    public int Id { get; set; }

    public int PriceDatabaseId { get; set; }

    [ForeignKey("PriceDatabaseId")]
    public PriceDatabase PriceDatabase { get; set; } = null!;

    [MaxLength(100)]
    public string ExternalId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Product { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
}

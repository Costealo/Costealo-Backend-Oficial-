using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costealo.API.Models;

public class PriceDatabase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int ItemCount { get; set; }
    
    // Add UserId to track ownership
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public EntityStatus Status { get; set; } = EntityStatus.Draft;

    public List<PriceItem> Items { get; set; } = new();
}

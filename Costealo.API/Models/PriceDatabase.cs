using System.ComponentModel.DataAnnotations;

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

    public List<PriceItem> Items { get; set; } = new();
}

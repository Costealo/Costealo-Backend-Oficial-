using System.ComponentModel.DataAnnotations;

namespace Costealo.API.DTOs;

public class CreatePriceDatabaseDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? SourceUrl { get; set; }
}

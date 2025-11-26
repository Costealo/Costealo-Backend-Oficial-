using System.ComponentModel.DataAnnotations;

namespace Costealo.API.DTOs;

public class AddWorkbookItemDto
{
    [Required]
    public int PriceItemId { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public string Unit { get; set; } = string.Empty;
    
    public decimal AdditionalCost { get; set; }
}

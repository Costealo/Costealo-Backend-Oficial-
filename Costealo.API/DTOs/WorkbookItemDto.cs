namespace Costealo.API.DTOs;

public class WorkbookItemDto
{
    public int Id { get; set; }
    public int PriceItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; } // Price from PriceItem
    public string OriginalUnit { get; set; } = string.Empty; // Unit from PriceItem
    
    public decimal QuantityUsed { get; set; } // Quantity in recipe
    public string UnitUsed { get; set; } = string.Empty; // Unit in recipe
    public decimal AdditionalCost { get; set; }
    
    public decimal CalculatedCost { get; set; } // The final cost for this item
    public string ConversionMessage { get; set; } = string.Empty; // e.g. "Converted 500g to 0.5kg"
}

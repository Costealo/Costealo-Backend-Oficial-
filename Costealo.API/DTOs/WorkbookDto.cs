namespace Costealo.API.DTOs;

public class WorkbookDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ProductionUnits { get; set; }
    
    // --- Weight Calculations ---
    public decimal TotalWeight { get; set; } // Sum of all ingredients in Kg/g
    public string WeightUnit { get; set; } = "g"; // Default to grams
    public decimal UnitWeight { get; set; } // TotalWeight / ProductionUnits
    
    // --- Cost Breakdown ---
    public decimal ProductionCost { get; set; } // Cost of Ingredients
    public decimal AdditionalCost { get; set; } // Sum of manual additional costs
    public decimal OperationalCost { get; set; } // (Prod + Add) * % + Fixed
    
    public decimal SubtotalCost { get; set; } // Prod + Add + Op
    public decimal TaxAmount { get; set; } // Subtotal * 16%
    public decimal TotalCost { get; set; } // Subtotal + Tax
    
    public decimal UnitCost { get; set; } // TotalCost / ProductionUnits
    
    // --- Price & Utility ---
    public decimal ProfitMarginPercentage { get; set; } // 20%
    public decimal SuggestedPrice { get; set; } // UnitCost / (1 - Margin) OR UnitCost * (1 + Margin)
    public decimal? TargetSalePrice { get; set; }
    public decimal ActualProfitMargin { get; set; } // If TargetPrice set
    
    public List<WorkbookItemDto> Items { get; set; } = new();
}

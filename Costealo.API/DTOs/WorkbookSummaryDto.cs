namespace Costealo.API.DTOs;

public class WorkbookSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal ProfitMargin { get; set; } // Percentage
    public decimal ProductionUnits { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

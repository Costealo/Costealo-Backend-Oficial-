using System.ComponentModel.DataAnnotations;

namespace Costealo.API.DTOs;

public class CreateWorkbookDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public decimal ProductionUnits { get; set; } = 1;
    public decimal TaxPercentage { get; set; } = 0.16m;
    public decimal ProfitMarginPercentage { get; set; } = 0.20m;
    
    public decimal? TargetSalePrice { get; set; }
    
    public decimal OperationalCostPercentage { get; set; }
    public decimal OperationalCostFixed { get; set; }
}

public class UpdateWorkbookDto : CreateWorkbookDto
{
}

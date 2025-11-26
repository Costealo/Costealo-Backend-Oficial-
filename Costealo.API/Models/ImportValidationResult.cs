namespace Costealo.API.Models;

/// <summary>
/// Resultado de la validación de importación de Excel
/// </summary>
public class ImportValidationResult
{
    public bool IsValid { get; set; }
    public int TotalRows { get; set; }
    public List<ImportValidationError> Errors { get; set; } = new();
    public List<PriceItem> ValidItems { get; set; } = new();
    
    public int ErrorCount => Errors.Count;
}

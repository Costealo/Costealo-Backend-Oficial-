using Costealo.API.Models;

namespace Costealo.API.Services;

public interface IExcelService
{
    List<PriceItem> ParsePriceItems(IFormFile file);
    List<PriceItem> ParsePriceItems(Stream stream);
    
    /// <summary>
    /// Parsea y valida items de un archivo Excel.
    /// Retorna resultado con errores detallados si la validación falla.
    /// </summary>
    ImportValidationResult ParseAndValidate(IFormFile file);
    
    /// <summary>
    /// Parsea y valida items de un stream de Excel.
    /// Retorna resultado con errores detallados si la validación falla.
    /// </summary>
    ImportValidationResult ParseAndValidate(Stream stream);
}

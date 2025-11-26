using Costealo.API.Models;

namespace Costealo.API.Services;

public interface IExcelService
{
    List<PriceItem> ParsePriceItems(IFormFile file);
    List<PriceItem> ParsePriceItems(Stream stream);
}

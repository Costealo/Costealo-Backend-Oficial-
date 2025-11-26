using Costealo.API.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Costealo.API.Services;

public class ExcelService : IExcelService
{
    public List<PriceItem> ParsePriceItems(IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            return ParsePriceItems(stream);
        }
    }

    public List<PriceItem> ParsePriceItems(Stream stream)
    {
        var priceItems = new List<PriceItem>();

        // Ensure stream is at the beginning
        if (stream.CanSeek && stream.Position != 0)
        {
            stream.Position = 0;
        }

        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);

        // Assuming first row is header, start from row 1
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            // Simple mapping based on column index:
            // 0: ID (ExternalId)
            // 1: PRODUCTO
            // 2: PRECIO
            // 3: UNIDAD

            var item = new PriceItem
            {
                ExternalId = GetCellValue(row.GetCell(0)),
                Product = GetCellValue(row.GetCell(1)),
                Price = ParseDecimal(GetCellValue(row.GetCell(2))),
                Unit = GetCellValue(row.GetCell(3))
            };

            if (!string.IsNullOrWhiteSpace(item.Product))
            {
                priceItems.Add(item);
            }
        }

        return priceItems;
    }

    private string GetCellValue(ICell? cell)
    {
        if (cell == null) return string.Empty;

        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            _ => cell.ToString() ?? string.Empty
        };
    }

    private decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, out var result))
        {
            return result;
        }
        return 0;
    }
}

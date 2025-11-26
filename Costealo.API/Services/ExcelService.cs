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

    public ImportValidationResult ParseAndValidate(IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            return ParseAndValidate(stream);
        }
    }

    public ImportValidationResult ParseAndValidate(Stream stream)
    {
        var result = new ImportValidationResult();
        var errors = new List<ImportValidationError>();
        var validItems = new List<PriceItem>();
        var externalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var externalIdRows = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Ensure stream is at the beginning
        if (stream.CanSeek && stream.Position != 0)
        {
            stream.Position = 0;
        }

        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);

        // Start from row 1 (assuming row 0 is header)
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            int excelRowNumber = i + 1; // +1 because Excel is 1-indexed and we skip header

            // Parse values
            string externalId = GetCellValue(row.GetCell(0));
            string product = GetCellValue(row.GetCell(1));
            string priceStr = GetCellValue(row.GetCell(2));
            string unit = GetCellValue(row.GetCell(3));

            bool hasErrors = false;

            // Validate Product
            if (string.IsNullOrWhiteSpace(product))
            {
                errors.Add(new ImportValidationError(
                    excelRowNumber,
                    "Producto",
                    product,
                    "El nombre del producto es requerido"
                ));
                hasErrors = true;
            }

            // Validate Price
            if (!decimal.TryParse(priceStr, out var price))
            {
                errors.Add(new ImportValidationError(
                    excelRowNumber,
                    "Precio",
                    priceStr,
                    "El precio debe ser un número válido"
                ));
                hasErrors = true;
            }
            else if (price <= 0)
            {
                errors.Add(new ImportValidationError(
                    excelRowNumber,
                    "Precio",
                    priceStr,
                    "El precio debe ser mayor a cero"
                ));
                hasErrors = true;
            }

            // Validate Unit
            if (!UnitCatalog.IsValidUnit(unit))
            {
                errors.Add(new ImportValidationError(
                    excelRowNumber,
                    "Unidad",
                    unit,
                    "Unidad inválida. Use una unidad del catálogo (ej: kilogram, gram, liter)"
                ));
                hasErrors = true;
            }

            // Validate ExternalId uniqueness
            if (!string.IsNullOrWhiteSpace(externalId))
            {
                if (externalIds.Contains(externalId))
                {
                    errors.Add(new ImportValidationError(
                        excelRowNumber,
                        "ID",
                        externalId,
                        $"El ID '{externalId}' está duplicado en la fila {externalIdRows[externalId]}"
                    ));
                    hasErrors = true;
                }
                else
                {
                    externalIds.Add(externalId);
                    externalIdRows[externalId] = excelRowNumber;
                }
            }

            // Only add to valid items if no errors
            if (!hasErrors)
            {
                validItems.Add(new PriceItem
                {
                    ExternalId = externalId,
                    Product = product,
                    Price = price,
                    Unit = unit
                });
            }
        }

        result.TotalRows = sheet.LastRowNum; // Excluding header
        result.Errors = errors;
        result.ValidItems = validItems;
        result.IsValid = errors.Count == 0 && validItems.Count > 0;

        return result;
    }
}

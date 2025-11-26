using Costealo.API.Data;
using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PriceDatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IExcelService _excelService;

    public PriceDatabaseController(ApplicationDbContext context, IExcelService excelService)
    {
        _context = context;
        _excelService = excelService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<PriceDatabase>> UploadDatabase(IFormFile file, [FromForm] string databaseName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a valid Excel file.");
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return BadRequest("Database name is required.");
        }

        try
        {
            var items = _excelService.ParsePriceItems(file);

            if (items.Count == 0)
            {
                return BadRequest("No valid items found in the Excel file.");
            }

            var priceDatabase = new PriceDatabase
            {
                Name = databaseName,
                UploadDate = DateTime.UtcNow,
                ItemCount = items.Count,
                Items = items
            };

            _context.PriceDatabases.Add(priceDatabase);
            await _context.SaveChangesAsync();

            return Ok(priceDatabase);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

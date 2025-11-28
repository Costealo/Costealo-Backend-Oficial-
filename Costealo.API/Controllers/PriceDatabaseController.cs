using Costealo.API.Data;
using Costealo.API.DTOs;
using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PriceDatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IExcelService _excelService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly HttpClient _httpClient;

    public PriceDatabaseController(ApplicationDbContext context, IExcelService excelService, ISubscriptionService subscriptionService)
    {
        _context = context;
        _excelService = excelService;
        _subscriptionService = subscriptionService;
        _httpClient = new HttpClient();
    }

    // --- Database Management ---

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceDatabase>>> GetDatabases()
    {
        return await _context.PriceDatabases.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PriceDatabase>> GetDatabase(int id)
    {
        var database = await _context.PriceDatabases.FindAsync(id);

        if (database == null)
        {
            return NotFound();
        }

        return database;
    }

    [HttpPost]
    public async Task<ActionResult<PriceDatabase>> CreateDatabase(CreatePriceDatabaseDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check subscription limits
        if (!await _subscriptionService.CanCreateDatabase(userId))
        {
            var subscription = await _subscriptionService.GetUserSubscription(userId);
            return BadRequest($"Database limit reached for your {subscription.PlanType} plan. Upgrade to create more databases.");
        }

        var database = new PriceDatabase
        {
            Name = dto.Name,
            SourceUrl = dto.SourceUrl,
            UploadDate = DateTime.UtcNow,
            ItemCount = 0,
            UserId = userId
        };

        _context.PriceDatabases.Add(database);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDatabase), new { id = database.Id }, database);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDatabase(int id, PriceDatabase database)
    {
        if (id != database.Id)
        {
            return BadRequest();
        }

        _context.Entry(database).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PriceDatabaseExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDatabase(int id)
    {
        var database = await _context.PriceDatabases.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
        if (database == null)
        {
            return NotFound();
        }

        _context.PriceDatabases.Remove(database);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Import & Upload ---

    [HttpPost("upload")]
    public async Task<ActionResult<PriceDatabase>> UploadDatabase(IFormFile file, [FromForm] string databaseName)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid Excel file.");
        if (string.IsNullOrWhiteSpace(databaseName))
            return BadRequest("Database name is required.");

        // Check subscription limits
        var userId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
        if (!await _subscriptionService.CanCreateDatabase(userId))
        {
            var subscription = await _subscriptionService.GetUserSubscription(userId);
            return BadRequest($"Database limit reached for your {subscription.PlanType} plan. Upgrade to create more databases.");
        }

        try
        {
            var items = _excelService.ParsePriceItems(file);
            if (items.Count == 0) return BadRequest("No valid items found.");

            var priceDatabase = new PriceDatabase
            {
                Name = databaseName,
                UploadDate = DateTime.UtcNow,
                ItemCount = items.Count,
                Items = items,
                UserId = userId
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

    [HttpPost("import-url")]
    public async Task<ActionResult<PriceDatabase>> ImportFromUrl([FromBody] ImportUrlDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("URL and Name are required.");

        // Check subscription limits
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!await _subscriptionService.CanCreateDatabase(userId))
        {
            var subscription = await _subscriptionService.GetUserSubscription(userId);
            return BadRequest($"Database limit reached for your {subscription.PlanType} plan. Upgrade to create more databases.");
        }

        try
        {
            var stream = await _httpClient.GetStreamAsync(request.Url);
            var items = _excelService.ParsePriceItems(stream);

            if (items.Count == 0) return BadRequest("No valid items found in the file from URL.");

            var priceDatabase = new PriceDatabase
            {
                Name = request.Name,
                SourceUrl = request.Url,
                UploadDate = DateTime.UtcNow,
                ItemCount = items.Count,
                Items = items,
                UserId = userId
            };

            _context.PriceDatabases.Add(priceDatabase);
            await _context.SaveChangesAsync();

            return Ok(priceDatabase);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error importing from URL: {ex.Message}");
        }
    }

    [HttpPut("{id}/refresh")]
    public async Task<IActionResult> RefreshDatabase(int id)
    {
        var database = await _context.PriceDatabases.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
        if (database == null) return NotFound();
        if (string.IsNullOrWhiteSpace(database.SourceUrl)) return BadRequest("This database does not have a source URL.");

        try
        {
            var stream = await _httpClient.GetStreamAsync(database.SourceUrl);
            var newItems = _excelService.ParsePriceItems(stream);

            if (newItems.Count == 0) return BadRequest("No valid items found in the refreshed file.");

            // Remove old items
            _context.PriceItems.RemoveRange(database.Items);

            // Add new items
            database.Items = newItems;
            database.ItemCount = newItems.Count;
            database.UploadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(database);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error refreshing database: {ex.Message}");
        }
    }

    // --- Item Management ---

    [HttpGet("{id}/items")]
    public async Task<ActionResult<IEnumerable<PriceItem>>> GetDatabaseItems(int id)
    {
        return await _context.PriceItems.Where(i => i.PriceDatabaseId == id).ToListAsync();
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<PriceItem>> AddItem(int id, PriceItem item)
    {
        if (id != item.PriceDatabaseId) return BadRequest("Database ID mismatch.");
        
        if (!PriceDatabaseExists(id)) return NotFound("Database not found.");

        // Validate product name
        if (string.IsNullOrWhiteSpace(item.Product))
            return BadRequest("Product name is required and cannot be empty.");

        // Validate price
        if (item.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        // Validate unit
        if (!UnitCatalog.IsValidUnit(item.Unit))
            return BadRequest($"Invalid unit '{item.Unit}'. Please use a valid unit from the catalog.");

        // Validate ExternalId uniqueness within the database
        if (!string.IsNullOrWhiteSpace(item.ExternalId))
        {
            var duplicateExists = await _context.PriceItems
                .AnyAsync(i => i.PriceDatabaseId == id && i.ExternalId == item.ExternalId);
            
            if (duplicateExists)
                return BadRequest($"An item with ExternalId '{item.ExternalId}' already exists in this database.");
        }

        _context.PriceItems.Add(item);
        
        // Update item count
        var db = await _context.PriceDatabases.FindAsync(id);
        if (db != null) db.ItemCount++;

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDatabaseItems), new { id = id }, item);
    }

    [HttpPut("{id}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int id, int itemId, PriceItem item)
    {
        if (id != item.PriceDatabaseId || itemId != item.Id) return BadRequest();

        // Validate product name
        if (string.IsNullOrWhiteSpace(item.Product))
            return BadRequest("Product name is required and cannot be empty.");

        // Validate price
        if (item.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        // Validate unit
        if (!UnitCatalog.IsValidUnit(item.Unit))
            return BadRequest($"Invalid unit '{item.Unit}'. Please use a valid unit from the catalog.");

        // Validate ExternalId uniqueness within the database (excluding current item)
        if (!string.IsNullOrWhiteSpace(item.ExternalId))
        {
            var duplicateExists = await _context.PriceItems
                .AnyAsync(i => i.PriceDatabaseId == id && i.ExternalId == item.ExternalId && i.Id != itemId);
            
            if (duplicateExists)
                return BadRequest($"An item with ExternalId '{item.ExternalId}' already exists in this database.");
        }

        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.PriceItems.Any(e => e.Id == itemId)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(int id, int itemId)
    {
        var item = await _context.PriceItems.FindAsync(itemId);
        if (item == null) return NotFound();
        if (item.PriceDatabaseId != id) return BadRequest("Item does not belong to this database.");

        _context.PriceItems.Remove(item);

        // Update item count
        var db = await _context.PriceDatabases.FindAsync(id);
        if (db != null) db.ItemCount--;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PriceDatabaseExists(int id)
    {
        return _context.PriceDatabases.Any(e => e.Id == id);
    }
}

public class ImportUrlDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

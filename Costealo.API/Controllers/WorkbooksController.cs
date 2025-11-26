using System.Security.Claims;
using Costealo.API.Data;
using Costealo.API.DTOs;
using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costealo.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkbooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitConversionService _conversionService;
    private readonly ISubscriptionService _subscriptionService;

    public WorkbooksController(ApplicationDbContext context, IUnitConversionService conversionService, ISubscriptionService subscriptionService)
    {
        _context = context;
        _conversionService = conversionService;
        _subscriptionService = subscriptionService;
    }

    // GET: api/workbooks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkbookSummaryDto>>> GetWorkbooks()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbooks = await _context.Workbooks
            .Where(w => w.UserId == userId)
            .Include(w => w.Items)
            .ThenInclude(i => i.PriceItem)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        var summaryList = new List<WorkbookSummaryDto>();

        foreach (var workbook in workbooks)
        {
            // Calculate details to get fresh totals
            var details = await CalculateWorkbook(workbook);
            
            summaryList.Add(new WorkbookSummaryDto
            {
                Id = workbook.Id,
                Name = workbook.Name,
                ProductionUnits = workbook.ProductionUnits,
                CreatedAt = workbook.CreatedAt,
                SellingPrice = details.SuggestedPrice,
                ProfitMargin = details.ActualProfitMargin != 0 ? details.ActualProfitMargin : workbook.ProfitMarginPercentage,
                Status = workbook.Status.ToString()
            });
        }

        return summaryList;
    }

    // GET: api/workbooks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkbookDto>> GetWorkbook(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks
            .Include(w => w.Items)
            .ThenInclude(i => i.PriceItem)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        return await CalculateWorkbook(workbook);
    }

    // POST: api/workbooks
    [HttpPost]
    public async Task<ActionResult<Workbook>> CreateWorkbook(CreateWorkbookDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check subscription limits
        if (!await _subscriptionService.CanCreateWorkbook(userId))
        {
            var subscription = await _subscriptionService.GetUserSubscription(userId);
            return BadRequest($"Workbook limit reached for your {subscription.PlanType} plan. Upgrade to create more workbooks.");
        }

        var workbook = new Workbook
        {
            Name = dto.Name,
            ProductionUnits = dto.ProductionUnits,
            TaxPercentage = dto.TaxPercentage,
            ProfitMarginPercentage = dto.ProfitMarginPercentage,
            TargetSalePrice = dto.TargetSalePrice,
            OperationalCostPercentage = dto.OperationalCostPercentage,
            OperationalCostFixed = dto.OperationalCostFixed,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Draft // Default to Draft
        };

        _context.Workbooks.Add(workbook);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkbook), new { id = workbook.Id }, workbook);
    }

    // PUT: api/workbooks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkbook(int id, UpdateWorkbookDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        workbook.Name = dto.Name;
        workbook.ProductionUnits = dto.ProductionUnits;
        workbook.TaxPercentage = dto.TaxPercentage;
        workbook.ProfitMarginPercentage = dto.ProfitMarginPercentage;
        workbook.TargetSalePrice = dto.TargetSalePrice;
        workbook.OperationalCostPercentage = dto.OperationalCostPercentage;
        workbook.OperationalCostFixed = dto.OperationalCostFixed;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/workbooks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkbook(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        _context.Workbooks.Remove(workbook);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/workbooks/5/publish
    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishWorkbook(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        workbook.Status = EntityStatus.Published;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/workbooks/5/items
    [HttpPost("{id}/items")]
    public async Task<ActionResult<WorkbookItem>> AddItem(int id, AddWorkbookItemDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        // Verify PriceItem belongs to a database owned by user
        var priceItem = await _context.PriceItems
            .Include(p => p.PriceDatabase)
            .FirstOrDefaultAsync(p => p.Id == dto.PriceItemId);

        if (priceItem == null) return BadRequest("Price item not found.");
        
        // Security Check: Ensure the user owns the database this item belongs to
        if (priceItem.PriceDatabase.UserId != userId)
            return Forbid("You do not have access to this price item.");

        var item = new WorkbookItem
        {
            WorkbookId = id,
            PriceItemId = dto.PriceItemId,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            AdditionalCost = dto.AdditionalCost
        };

        _context.WorkbookItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkbook), new { id = id }, item);
    }

    // DELETE: api/workbooks/5/items/10
    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(int id, int itemId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workbook = await _context.Workbooks.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workbook == null) return NotFound();

        var item = await _context.WorkbookItems.FirstOrDefaultAsync(i => i.Id == itemId && i.WorkbookId == id);
        if (item == null) return NotFound();

        _context.WorkbookItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Calculation Logic ---

    private async Task<WorkbookDto> CalculateWorkbook(Workbook workbook)
    {
        var dto = new WorkbookDto
        {
            Id = workbook.Id,
            Name = workbook.Name,
            ProductionUnits = workbook.ProductionUnits,
            ProfitMarginPercentage = workbook.ProfitMarginPercentage,
            TargetSalePrice = workbook.TargetSalePrice
        };

        decimal totalProductionCost = 0;
        decimal totalAdditionalCost = 0;
        decimal totalWeight = 0;

        foreach (var item in workbook.Items)
        {
            var itemDto = new WorkbookItemDto
            {
                Id = item.Id,
                PriceItemId = item.PriceItemId,
                ProductName = item.PriceItem.Product,
                OriginalPrice = item.PriceItem.Price,
                OriginalUnit = item.PriceItem.Unit,
                QuantityUsed = item.Quantity,
                UnitUsed = item.Unit,
                AdditionalCost = item.AdditionalCost
            };

            // 1. Calculate Cost with Unit Conversion
            string conversionMsg = "";

            if (!string.Equals(item.Unit, item.PriceItem.Unit, StringComparison.OrdinalIgnoreCase))
            {
                // Convert 1 unit of PriceItem.Unit to Item.Unit
                // Example: PriceItem is $/kg. Item is g.
                // We need price per g.
                // Convert(1, "kg", "g") = 1000.
                // Price per g = Price / 1000.
                
                // Wait! The service converts Quantity from -> to.
                // Convert(1, "kg", "g") = 1000.
                // So 1 kg = 1000 g.
                // Price is for 1 kg.
                // So Price is for 1000 g.
                // Price per g = Price / 1000.
                
                var factor = await _conversionService.ConvertAsync(1, item.PriceItem.Unit, item.Unit);
                if (factor.HasValue && factor.Value != 0)
                {
                    // Price per Item.Unit = OriginalPrice / Factor
                    // Example: $10/kg. Convert(1, kg, g) = 1000. Price/g = 10/1000 = 0.01.
                    decimal pricePerItemUnit = item.PriceItem.Price / factor.Value;
                    itemDto.CalculatedCost = (pricePerItemUnit * item.Quantity) + item.AdditionalCost;
                    conversionMsg = $"Converted 1 {item.PriceItem.Unit} to {factor.Value} {item.Unit}";
                }
                else
                {
                    // Fallback or Error: Assume 1:1 if conversion fails (should handle better in prod)
                    itemDto.CalculatedCost = (item.PriceItem.Price * item.Quantity) + item.AdditionalCost;
                    conversionMsg = "Conversion failed, assumed 1:1";
                }
            }
            else
            {
                itemDto.CalculatedCost = (item.PriceItem.Price * item.Quantity) + item.AdditionalCost;
            }

            itemDto.ConversionMessage = conversionMsg;
            dto.Items.Add(itemDto);

            totalProductionCost += (itemDto.CalculatedCost - item.AdditionalCost);
            totalAdditionalCost += item.AdditionalCost;

            // 2. Calculate Weight (Normalize to grams for total)
            // We need to convert Item.Quantity + Item.Unit -> grams
            var weightInGrams = await _conversionService.ConvertAsync(item.Quantity, item.Unit, "gram");
            if (weightInGrams.HasValue)
            {
                totalWeight += weightInGrams.Value;
            }
        }

        // --- Totals ---
        dto.TotalWeight = totalWeight;
        dto.UnitWeight = workbook.ProductionUnits > 0 ? totalWeight / workbook.ProductionUnits : 0;

        dto.ProductionCost = totalProductionCost;
        dto.AdditionalCost = totalAdditionalCost;
        
        // Operational Cost
        dto.OperationalCost = ((totalProductionCost + totalAdditionalCost) * workbook.OperationalCostPercentage) + workbook.OperationalCostFixed;

        dto.SubtotalCost = totalProductionCost + totalAdditionalCost + dto.OperationalCost;
        dto.TaxAmount = dto.SubtotalCost * workbook.TaxPercentage;
        dto.TotalCost = dto.SubtotalCost + dto.TaxAmount;

        dto.UnitCost = workbook.ProductionUnits > 0 ? dto.TotalCost / workbook.ProductionUnits : 0;

        // --- Price & Margin ---
        // Suggested Price = UnitCost + (UnitCost * Margin)  [Markup Logic]
        dto.SuggestedPrice = dto.UnitCost * (1 + workbook.ProfitMarginPercentage);

        if (workbook.TargetSalePrice.HasValue && workbook.TargetSalePrice.Value > 0)
        {
            // Actual Margin = (Price - Cost) / Cost  [Markup Logic]
            // Or (Price - Cost) / Price [Margin Logic]
            // Let's use Markup to be consistent with the Suggested Price formula above.
            // Margin = (Price / Cost) - 1
            if (dto.UnitCost > 0)
            {
                dto.ActualProfitMargin = (workbook.TargetSalePrice.Value / dto.UnitCost) - 1;
            }
            else
            {
                dto.ActualProfitMargin = 1; // 100% if cost is 0
            }
        }

        return dto;
    }
}

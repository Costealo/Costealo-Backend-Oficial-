using Costealo.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UnitsController : ControllerBase
{
    /// <summary>
    /// Obtiene el catálogo completo de unidades organizadas por categoría
    /// </summary>
    [HttpGet("catalog")]
    public ActionResult<object> GetCatalog()
    {
        return Ok(UnitCatalog.Categories);
    }

    /// <summary>
    /// Obtiene una lista plana de todas las unidades válidas
    /// </summary>
    [HttpGet("valid")]
    public ActionResult<IEnumerable<string>> GetValidUnits()
    {
        return Ok(UnitCatalog.ValidUnits.OrderBy(u => u));
    }

    /// <summary>
    /// Verifica si una unidad es válida
    /// </summary>
    [HttpGet("validate/{unit}")]
    public ActionResult<object> ValidateUnit(string unit)
    {
        var isValid = UnitCatalog.IsValidUnit(unit);
        var unitInfo = isValid ? UnitCatalog.GetUnitInfo(unit) : null;

        return Ok(new
        {
            unit,
            isValid,
            info = unitInfo
        });
    }
}

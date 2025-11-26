namespace Costealo.API.Models;

/// <summary>
/// Catálogo completo de unidades de medida soportadas por la API de conversión
/// </summary>
public static class UnitCatalog
{
    public static Dictionary<string, List<UnitInfo>> Categories { get; } = new()
    {
        {
            "Peso", new List<UnitInfo>
            {
                new("kilogram", "Kilogramo", "kg"),
                new("gram", "Gramo", "g"),
                new("pound", "Libra", "lb"),
                new("ounce", "Onza", "oz"),
                new("ounce (gold)", "Onza (oro)", "oz"),
                new("tonne", "Tonelada", "ton"),
                new("ton (UK)", "Tonelada (UK)", "ton"),
                new("ton (US)", "Tonelada (US)", "ton"),
                new("carat", "Quilate", "ct"),
                new("carat (UK)", "Quilate (UK)", "ct"),
                new("stone", "Stone", "st"),
                new("mace (China)", "Mace (China)", ""),
                new("tael (China)", "Tael (China)", ""),
                new("catty (China)", "Catty (China)", ""),
                new("mace (HK)", "Mace (HK)", ""),
                new("tael (HK)", "Tael (HK)", ""),
                new("catty (HK)", "Catty (HK)", ""),
                new("tam (HK)", "Tam (HK)", ""),
                new("tael (Taiwan)", "Tael (Taiwan)", ""),
                new("catty (Taiwan)", "Catty (Taiwan)", ""),
                new("catty (Japan)", "Catty (Japan)", ""),
                new("dan (Japan)", "Dan (Japan)", ""),
                new("artel (Arab)", "Artel (Árabe)", ""),
                new("baht (Thai)", "Baht (Tailandés)", ""),
                new("bale (UK)", "Bale (UK)", ""),
                new("bale (US)", "Bale (US)", ""),
                new("denier (France)", "Denier (Francia)", ""),
                new("centner (Germany)", "Centner (Alemania)", ""),
                new("gran (Germany)", "Gran (Alemania)", ""),
                new("lot (Germany)", "Lot (Alemania)", ""),
                new("centner (Russia)", "Centner (Rusia)", ""),
                new("danaro (Italy)", "Danaro (Italia)", ""),
                new("etto (Italy)", "Etto (Italia)", ""),
                new("grano (Italy)", "Grano (Italia)", ""),
                new("Arroba (Portugal)", "Arroba (Portugal)", ""),
                new("funt (Russia)", "Funt (Rusia)", "")
            }
        },
        {
            "Longitud", new List<UnitInfo>
            {
                new("kilometer", "Kilómetro", "km"),
                new("meter", "Metro", "m"),
                new("centimeter", "Centímetro", "cm"),
                new("millimeter", "Milímetro", "mm"),
                new("inch", "Pulgada", "inch"),
                new("feet", "Pie", "ft"),
                new("yard", "Yarda", "yd"),
                new("mile", "Milla", "mi"),
                new("fen (Taiwan)", "Fen (Taiwan)", ""),
                new("inch (Taiwan)", "Pulgada (Taiwan)", ""),
                new("feet (Taiwan)", "Pie (Taiwan)", ""),
                new("jian (Taiwan)", "Jian (Taiwan)", ""),
                new("inch (Japan)", "Pulgada (Japón)", ""),
                new("feet (Japan)", "Pie (Japón)", ""),
                new("jian (Japan)", "Jian (Japón)", ""),
                new("zhang (Japan)", "Zhang (Japón)", ""),
                new("ding (Japan)", "Ding (Japón)", ""),
                new("li (Japan)", "Li (Japón)", "")
            }
        },
        {
            "Área", new List<UnitInfo>
            {
                new("sq meter", "Metro cuadrado", "m²"),
                new("sq inch", "Pulgada cuadrada", "inch²"),
                new("sq feet", "Pie cuadrado", "ft²"),
                new("sq yard", "Yarda cuadrada", "yd²"),
                new("sq kilometer", "Kilómetro cuadrado", "km²"),
                new("acre", "Acre", "ac"),
                new("hectare", "Hectárea", "ha"),
                new("ping (Taiwan)", "Ping (Taiwan)", ""),
                new("fen (Taiwan)", "Fen (Taiwan)", ""),
                new("jia (Taiwan)", "Jia (Taiwan)", ""),
                new("mu (China)", "Mu (China)", "")
            }
        },
        {
            "Volumen", new List<UnitInfo>
            {
                new("liter", "Litro", "l"),
                new("cu mm", "Milímetro cúbico", "mm³"),
                new("milliliter", "Mililitro", "ml"),
                new("cu cm", "Centímetro cúbico", "cc"),
                new("cu meter", "Metro cúbico", "m³"),
                new("cu inch", "Pulgada cúbica", "inch³"),
                new("cu feet", "Pie cúbico", "ft³"),
                new("ounce (US)", "Onza (US)", "oz"),
                new("pint (US)", "Pinta (US)", "pt"),
                new("ounce (UK)", "Onza (UK)", "oz"),
                new("pint (UK)", "Pinta (UK)", "pt"),
                new("gallon", "Galón", "gal")
            }
        },
        {
            "Temperatura", new List<UnitInfo>
            {
                new("Celsius", "Celsius", "°C"),
                new("Fahrenheit", "Fahrenheit", "°F"),
                new("Kelvin", "Kelvin", "K")
            }
        },
        {
            "Velocidad", new List<UnitInfo>
            {
                new("meter/s", "Metro por segundo", "m/s"),
                new("foot/s", "Pie por segundo", "ft/s"),
                new("inch/s", "Pulgada por segundo", "in/s"),
                new("km/s", "Kilómetro por segundo", "km/s"),
                new("km/min", "Kilómetro por minuto", "km/min"),
                new("km/hour", "Kilómetro por hora", "km/h"),
                new("meter/min", "Metro por minuto", "m/min"),
                new("mile/s", "Milla por segundo", "ml/s"),
                new("mile/min", "Milla por minuto", "ml/min"),
                new("mile/hour", "Milla por hora", "mph")
            }
        },
        {
            "Combustible", new List<UnitInfo>
            {
                new("km/gal", "Kilómetro por galón", "km/gal"),
                new("km/l", "Kilómetro por litro", "km/l"),
                new("ml/l (US)", "Milla por litro (US)", "ml/l"),
                new("mi/gal (US)", "Milla por galón (US)", "mi/gal"),
                new("mi/gal (UK)", "Milla por galón (UK)", "mi/gal")
            }
        },
        {
            "Presión", new List<UnitInfo>
            {
                new("atm pressure", "Atmósfera", "atm"),
                new("Pascal", "Pascal", "Pa"),
                new("psi", "PSI", "psi"),
                new("psf", "PSF", "psf")
            }
        }
    };

    /// <summary>
    /// Obtiene todas las unidades válidas (códigos API)
    /// </summary>
    public static HashSet<string> ValidUnits { get; } = Categories
        .SelectMany(c => c.Value)
        .Select(u => u.ApiCode)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Verifica si una unidad es válida
    /// </summary>
    public static bool IsValidUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return false;
        return ValidUnits.Contains(unit.Trim());
    }

    /// <summary>
    /// Obtiene la información de una unidad por su código
    /// </summary>
    public static UnitInfo? GetUnitInfo(string apiCode)
    {
        return Categories
            .SelectMany(c => c.Value)
            .FirstOrDefault(u => u.ApiCode.Equals(apiCode, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Información de una unidad de medida
/// </summary>
public record UnitInfo(
    string ApiCode,      // Código que usa la API (ej: "kilogram")
    string DisplayName,  // Nombre para mostrar (ej: "Kilogramo")
    string Symbol        // Símbolo (ej: "kg")
);

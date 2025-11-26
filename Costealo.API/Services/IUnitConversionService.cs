namespace Costealo.API.Services;

public interface IUnitConversionService
{
    /// <summary>
    /// Converts a quantity from one unit to another.
    /// </summary>
    /// <param name="quantity">The amount to convert</param>
    /// <param name="fromUnit">Source unit (e.g., "kilogram", "meter")</param>
    /// <param name="toUnit">Target unit (e.g., "gram", "centimeter")</param>
    /// <returns>The converted value, or null if conversion fails</returns>
    Task<decimal?> ConvertAsync(decimal quantity, string fromUnit, string toUnit);
}

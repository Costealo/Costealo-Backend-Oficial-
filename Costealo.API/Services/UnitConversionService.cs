using System.Text.Json;

namespace Costealo.API.Services;

public class UnitConversionService : IUnitConversionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UnitConversionService> _logger;

    // Fallback conversion table for common conversions
    private static readonly Dictionary<(string, string), decimal> _fallbackConversions = new()
    {
        // Mass conversions
        { ("kilogram", "gram"), 1000m },
        { ("gram", "kilogram"), 0.001m },
        { ("kilogram", "milligram"), 1_000_000m },
        { ("milligram", "kilogram"), 0.000001m },
        { ("gram", "milligram"), 1000m },
        { ("milligram", "gram"), 0.001m },
        
        // Length conversions
        { ("meter", "centimeter"), 100m },
        { ("centimeter", "meter"), 0.01m },
        { ("meter", "millimeter"), 1000m },
        { ("millimeter", "meter"), 0.001m },
        { ("centimeter", "millimeter"), 10m },
        { ("millimeter", "centimeter"), 0.1m },
        
        // Volume conversions
        { ("liter", "milliliter"), 1000m },
        { ("milliliter", "liter"), 0.001m },
    };

    public UnitConversionService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<UnitConversionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<decimal?> ConvertAsync(decimal quantity, string fromUnit, string toUnit)
    {
        // Normalize unit names to lowercase
        var fromUnitNormalized = fromUnit.ToLowerInvariant().Trim();
        var toUnitNormalized = toUnit.ToLowerInvariant().Trim();

        // If units are the same, return the original quantity
        if (fromUnitNormalized == toUnitNormalized)
        {
            return quantity;
        }

        try
        {
            // Try RapidAPI conversion first
            var result = await ConvertViaApiAsync(quantity, fromUnitNormalized, toUnitNormalized);
            if (result.HasValue)
            {
                _logger.LogInformation(
                    "Successfully converted {Quantity} {From} to {To} via API: {Result}",
                    quantity, fromUnit, toUnit, result.Value);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "API conversion failed for {Quantity} {From} to {To}. Trying fallback.",
                quantity, fromUnit, toUnit);
        }

        // Fallback to local conversion
        var fallbackResult = ConvertViaFallback(quantity, fromUnitNormalized, toUnitNormalized);
        if (fallbackResult.HasValue)
        {
            _logger.LogInformation(
                "Converted {Quantity} {From} to {To} via fallback: {Result}",
                quantity, fromUnit, toUnit, fallbackResult.Value);
        }
        else
        {
            _logger.LogWarning(
                "Unable to convert {Quantity} {From} to {To}",
                quantity, fromUnit, toUnit);
        }

        return fallbackResult;
    }

    private async Task<decimal?> ConvertViaApiAsync(decimal quantity, string fromUnit, string toUnit)
    {
        try
        {
            var baseUrl = _configuration["RapidApi:BaseUrl"];
            var host = _configuration["RapidApi:Host"];
            var apiKey = _configuration["RapidApi:Key"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("RapidAPI configuration is missing or incomplete");
                return null;
            }

            // Build the request URL based on RapidAPI Unit Measurement Conversion API
            var requestUrl = $"{baseUrl}/convert?value={quantity}&from={fromUnit}&to={toUnit}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-RapidAPI-Host", host);
            request.Headers.Add("X-RapidAPI-Key", apiKey);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "API returned status code {StatusCode} for conversion {From} to {To}",
                    response.StatusCode, fromUnit, toUnit);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Parse the JSON response
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // Try to extract the converted value from the response
            // The structure may vary, so we try different possible paths
            if (root.TryGetProperty("result", out var resultProp) && 
                resultProp.TryGetDecimal(out var resultValue))
            {
                return resultValue;
            }
            
            if (root.TryGetProperty("value", out var valueProp) && 
                valueProp.TryGetDecimal(out var value))
            {
                return value;
            }

            if (root.TryGetProperty("convertedValue", out var convertedProp) && 
                convertedProp.TryGetDecimal(out var convertedValue))
            {
                return convertedValue;
            }

            _logger.LogWarning("Could not parse API response for conversion: {Content}", content);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during API conversion");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from API");
            return null;
        }
    }

    private decimal? ConvertViaFallback(decimal quantity, string fromUnit, string toUnit)
    {
        var key = (fromUnit, toUnit);
        
        if (_fallbackConversions.TryGetValue(key, out var factor))
        {
            return quantity * factor;
        }

        return null;
    }
}

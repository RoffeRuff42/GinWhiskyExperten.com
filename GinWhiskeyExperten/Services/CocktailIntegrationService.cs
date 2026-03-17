using GinWhiskeyExperten.DTOs;
using System.Text.Json;

namespace GinWhiskeyExperten.Services
{
    public class CocktailIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CocktailIntegrationService> _logger;

        public CocktailIntegrationService(HttpClient httpClient, ILogger<CocktailIntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/1/");
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", "1"); // TheCocktailDB provides a free API key "1" for testing purposes. For production, consider getting your own API key.
        }

        public async Task<List<CocktailDto>> GetSmartCocktailSuggestionsAsync(string brandName, string spiritType)
        {
            try
            {
                _logger.LogInformation("Searching cocktails for brand: {Brand}", brandName);
                var cocktails = await FetchFromApiAsync($"search.php?s={brandName}"); // Attempt to find cocktails by brand name first

                if (cocktails == null || !cocktails.Any()) // If no cocktails found by brand, fallback to searching by spirit type
                {
                    _logger.LogInformation("No brand match for {Brand}. Falling back to spirit type: {Type}", brandName, spiritType); // Log the fallback scenario
                    cocktails = await FetchFromApiAsync($"filter.php?i={spiritType}"); // Filter cocktails by spirit type if brand search yields no results
                }

                return cocktails?.Take(5).ToList() ?? new List<CocktailDto>(); // Return top 5 suggestions or an empty list if no cocktails found
            }
            catch (Exception ex) // Catch any unexpected exceptions to prevent application crashes and log the error for debugging
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching cocktails.");
                return new List<CocktailDto>();
            }
        }

        private async Task<List<CocktailDto>> FetchFromApiAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CocktailDB API returned error: {StatusCode}", response.StatusCode); // Log the non-success status code for debugging purposes
                    return new List<CocktailDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                using var json = JsonDocument.Parse(content);

                if (!json.RootElement.TryGetProperty("drinks", out var drinksElement) || drinksElement.ValueKind == JsonValueKind.Null) // Handle the case where "drinks" property is missing or null in the API response, which can happen if no cocktails are found for the given search criteria
                {
                    return new List<CocktailDto>();
                }

                return drinksElement.EnumerateArray().Select(d => new CocktailDto(  // Map the API response to our CocktailDto, ensuring we handle potential null values gracefully
                    d.GetProperty("strDrink").GetString() ?? "Unknown Drink", // Provide a default name if the API response is missing the "strDrink" property
                    d.GetProperty("strDrinkThumb").GetString() ?? "", // Provide an empty string for the thumbnail URL if it's missing in the API response
                    "Mix and enjoy!" // Simple instructions placeholder since the API may not provide detailed instructions
                )).ToList();
            }
            catch (HttpRequestException ex) // Catch network-related exceptions to handle scenarios where the API is unreachable or there are connectivity issues
            {
                _logger.LogError(ex, "Network error while calling CocktailDB at endpoint: {Endpoint}", endpoint); 
                return new List<CocktailDto>();
            }
        }
    }
}


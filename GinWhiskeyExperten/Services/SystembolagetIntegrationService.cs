using System.Net.Http.Json;

namespace GinWhiskeyExperten.Services
{
    public class SystembolagetIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SystembolagetIntegrationService> _logger;

        public SystembolagetIntegrationService(HttpClient httpClient, ILogger<SystembolagetIntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://api-extern.systembolaget.se/");
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "TOP_SECRET_KEY"); // Fake API key for demonstration. Replace with actual key in production and consider using secure storage for sensitive information.
        }

        public async Task<string> GetStockStatusAsync(string articleNumber)
        {
            try
            {
                _logger.LogInformation("Fetching stock status for article: {ArticleNumber}", articleNumber); // Log the article number being queried for better traceability in logs

                var response = await _httpClient.GetAsync($"product/v1/product/search?SearchQuery={articleNumber}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Systembolaget API unreachable. Status: {Code}", response.StatusCode); // Log the non-success status code to help identify issues with the API connectivity
                    return "Availability information currently unavailable.";
                }

                return "Item found. Check local stores for exact stock levels."; // Since the API may not provide real-time stock levels, we return a generic message indicating that the item exists
            }
            catch (HttpRequestException ex) // Catch network-related exceptions to handle scenarios where the API is unreachable or there are connectivity issues
            {
                _logger.LogError(ex, "Network failure while connecting to Systembolaget.");
                return "Connection to Systembolaget failed. Please try again later.";
            }
        }
    }
}

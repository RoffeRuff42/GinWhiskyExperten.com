using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Tests.TestDoubles;

namespace GinWhiskeyExperten.Tests.Controllers
{
    // Covers the gap found while reviewing the app: the README documents POST/PUT/DELETE on
    // Spirits as Admin-only, but no [Authorize] attribute or authentication scheme existed at all.
    public class SpiritsAuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SpiritsAuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateSpirit_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/spirits", new SpiritCreateDto("Test Gin", "Gin", 40, "desc", 1));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateSpirit_WithAdminToken_Succeeds()
        {
            var client = _factory.CreateClient();
            var token = await LoginAsAdminAsync(client);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsJsonAsync("/api/spirits", new SpiritCreateDto("Test Gin", "Gin", 40, "desc", 1));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetSpirits_WithoutToken_StaysPublic()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/spirits");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<string> LoginAsAdminAsync(HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto(CustomWebApplicationFactory.AdminEmail, CustomWebApplicationFactory.AdminPassword));

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return body!.Token;
        }
    }
}

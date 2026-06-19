using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Services;
using GinWhiskeyExperten.Tests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;

namespace GinWhiskeyExperten.Tests.Services
{
    // Regression coverage for the cache-invalidation bug found while reviewing SpiritService:
    // ClearSpiritCache() built a cache key without the underscore the write path used, so
    // Update/Delete never invalidated the cached first page. These tests assert the repository is
    // actually re-queried after a write, not just that no exception was thrown.
    public class SpiritServiceCacheTests
    {
        private static SpiritService CreateService(FakeSpiritRepository repository)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new SpiritService(repository, new FakeFlavorRepository(), cache);
        }

        [Fact]
        public async Task GetSpiritsAsync_SecondCallWithSameParams_IsServedFromCache()
        {
            var repository = new FakeSpiritRepository(new[] { MakeSpirit(1) });
            var service = CreateService(repository);

            await service.GetSpiritsAsync(1, 10, null);
            await service.GetSpiritsAsync(1, 10, null);

            Assert.Equal(1, repository.GetAllAsyncCallCount);
        }

        [Fact]
        public async Task UpdateSpiritAsync_InvalidatesCache_SoNextReadHitsRepository()
        {
            var repository = new FakeSpiritRepository(new[] { MakeSpirit(1) });
            var service = CreateService(repository);

            await service.GetSpiritsAsync(1, 10, null); // populates the cache
            await service.UpdateSpiritAsync(1, new SpiritUpdateDto("Updated", "Gin", 40, "desc", 1));
            await service.GetSpiritsAsync(1, 10, null); // must not be served from a stale entry

            Assert.Equal(2, repository.GetAllAsyncCallCount);
        }

        [Fact]
        public async Task DeleteSpiritAsync_InvalidatesCache_SoNextReadHitsRepository()
        {
            var repository = new FakeSpiritRepository(new[] { MakeSpirit(1) });
            var service = CreateService(repository);

            await service.GetSpiritsAsync(1, 10, null); // populates the cache
            await service.DeleteSpiritAsync(1);
            await service.GetSpiritsAsync(1, 10, null); // must not be served from a stale entry

            Assert.Equal(2, repository.GetAllAsyncCallCount);
        }

        [Fact]
        public async Task UpdateSpiritAsync_InvalidatesNonDefaultPagedAndFilteredCacheEntriesToo()
        {
            // The original bug only ever cleared the page=1/size=10/type=all key. This asserts the
            // fixed tracked-key-set strategy invalidates every variant actually served, not just the default.
            var repository = new FakeSpiritRepository(new[] { MakeSpirit(1), MakeSpirit(2) });
            var service = CreateService(repository);

            await service.GetSpiritsAsync(2, 5, null);   // a non-default page
            await service.GetSpiritsAsync(1, 10, "Gin"); // a filtered query
            Assert.Equal(2, repository.GetAllAsyncCallCount);

            await service.UpdateSpiritAsync(1, new SpiritUpdateDto("Updated", "Gin", 40, "desc", 1));

            await service.GetSpiritsAsync(2, 5, null);
            await service.GetSpiritsAsync(1, 10, "Gin");

            Assert.Equal(4, repository.GetAllAsyncCallCount);
        }

        private static Spirit MakeSpirit(int id) => new()
        {
            Id = id,
            Name = $"Spirit {id}",
            Type = "Gin",
            Abv = 40,
            BrandId = 1
        };
    }
}

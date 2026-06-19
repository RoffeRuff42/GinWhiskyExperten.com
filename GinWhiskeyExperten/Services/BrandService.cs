using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace GinWhiskeyExperten.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repository;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "brands_list"; // Brand list isn't paginated/filtered, so one key covers it

        // See CacheKeyTracking - static + keyed by the cache instance because BrandService is
        // registered Scoped (one instance per request) while IMemoryCache is a singleton.
        private static readonly ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> _trackedKeys = new();

        public BrandService(IBrandRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<IEnumerable<BrandReadDto>> GetBrandsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<BrandReadDto>? cached))
            {
                var brands = await _repository.GetAllAsync();
                cached = brands.Select(ToDto).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(CacheKey, cached, cacheOptions);
                CacheKeyTracking.Track(_trackedKeys, _cache, CacheKey);
            }

            return cached!;
        }

        public async Task<BrandReadDto?> GetBrandByIdAsync(int id)
        {
            var brand = await _repository.GetByIdAsync(id);
            return brand == null ? null : ToDto(brand);
        }

        public async Task<BrandReadDto> CreateBrandAsync(BrandCreateDto createDto)
        {
            var brand = new Brand
            {
                Name = createDto.Name,
                Website = createDto.Website,
                Country = createDto.Country,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(brand);
            ClearCache();

            return ToDto(brand);
        }

        public async Task<bool> UpdateBrandAsync(int id, BrandUpdateDto updateDto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) return false;

            brand.Name = updateDto.Name;
            brand.Website = updateDto.Website;
            brand.Country = updateDto.Country;
            brand.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(brand);
            ClearCache();

            return true;
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) return false;

            await _repository.DeleteAsync(id);
            ClearCache();

            return true;
        }

        private void ClearCache() => CacheKeyTracking.InvalidateAll(_trackedKeys, _cache);

        private static BrandReadDto ToDto(Brand b) => new(b.Id, b.Name, b.Website, b.Country, b.CreatedAt);
    }
}

using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace GinWhiskeyExperten.Services
{
    public class FlavorService : IFlavorService
    {
        private readonly IFlavorRepository _repository;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "flavors_list"; // Flavor catalog isn't paginated/filtered, so one key covers it

        // See CacheKeyTracking - static + keyed by the cache instance because FlavorService is
        // registered Scoped (one instance per request) while IMemoryCache is a singleton.
        private static readonly ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> _trackedKeys = new();

        public FlavorService(IFlavorRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<IEnumerable<FlavorReadDto>> GetFlavorsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<FlavorReadDto>? cached))
            {
                var flavors = await _repository.GetAllAsync();
                cached = flavors.Select(ToDto).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(CacheKey, cached, cacheOptions);
                CacheKeyTracking.Track(_trackedKeys, _cache, CacheKey);
            }

            return cached!;
        }

        public async Task<FlavorReadDto?> GetFlavorByIdAsync(int id)
        {
            var flavor = await _repository.GetByIdAsync(id);
            return flavor == null ? null : ToDto(flavor);
        }

        public async Task<FlavorReadDto> CreateFlavorAsync(FlavorCreateDto createDto)
        {
            var flavor = new Flavor
            {
                Name = createDto.Name,
                Description = createDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(flavor);
            ClearCache();

            return ToDto(flavor);
        }

        public async Task<bool> UpdateFlavorAsync(int id, FlavorUpdateDto updateDto)
        {
            var flavor = await _repository.GetByIdAsync(id);
            if (flavor == null) return false;

            flavor.Name = updateDto.Name;
            flavor.Description = updateDto.Description;
            flavor.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(flavor);
            ClearCache();

            return true;
        }

        public async Task<bool> DeleteFlavorAsync(int id)
        {
            var flavor = await _repository.GetByIdAsync(id);
            if (flavor == null) return false;

            await _repository.DeleteAsync(id);
            ClearCache();

            return true;
        }

        private void ClearCache() => CacheKeyTracking.InvalidateAll(_trackedKeys, _cache);

        private static FlavorReadDto ToDto(Flavor f) => new(f.Id, f.Name, f.Description, f.CreatedAt);
    }
}

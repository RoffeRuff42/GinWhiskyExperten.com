using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Repositories;
using GinWhiskeyExperten.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GinWhiskeyExperten.Services
{
    public class SpiritService : ISpiritService
    {
        private readonly ISpiritRepository _repository;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "spirits_list"; // Prefix for cache keys to group related cache entries

        public SpiritService(ISpiritRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<PagedResponse<SpiritReadDto>> GetSpiritsAsync(int page, int pageSize, string? type)
        {
             
            string cacheKey = $"spirits_list_p{page}_s{pageSize}_t{type ?? "all"}"; // Create a unique cache key based on the query parameters (page, pageSize, type)

            
            if (!_cache.TryGetValue(cacheKey, out PagedResponse<SpiritReadDto>? cachedResponse)) // Check if the result is already in the cache
            {
                
                var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, type); // If not in cache, go to database

                var dtos = items.Select(s => new SpiritReadDto(
                    s.Id, s.Name, s.Type, s.Abv, s.Description,
                    s.Brand?.Name ?? "Unknown brand", s.CreatedAt
                ));

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                cachedResponse = new PagedResponse<SpiritReadDto>(dtos, page, pageSize, totalCount, totalPages);

                
                var cacheOptions = new MemoryCacheEntryOptions() // Save to cache with an expiration time
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Expire after 5 minutes
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2)); // Reset time if accessed frequently

                _cache.Set(cacheKey, cachedResponse, cacheOptions);
            }

            return cachedResponse!;
        }

        public async Task<SpiritReadDto?> GetSpiritByIdAsync(int id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return null;

            return new SpiritReadDto(s.Id, s.Name, s.Type, s.Abv, s.Description, s.Brand?.Name ?? "Unknown brand", s.CreatedAt);
        }

        public async Task<SpiritReadDto> CreateSpiritAsync(SpiritCreateDto createDto)
        {
            var newSpirit = new Spirit
            {
                Name = createDto.Name,
                Type = createDto.Type,
                Abv = createDto.Abv,
                Description = createDto.Description,
                BrandId = createDto.BrandId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newSpirit);
            _cache.Remove($"{CacheKeyPrefix}_p1_s10_tall"); // Clear the cache for the first page of all spirits to ensure new item appears in subsequent queries

            return new SpiritReadDto(
                newSpirit.Id,
                newSpirit.Name,
                newSpirit.Type,
                newSpirit.Abv,
                newSpirit.Description, "New item", // Since we don't have the Brand name immediately, we can set it to a placeholder or fetch it if needed
                newSpirit.CreatedAt
            );


        }
        public async Task<bool> UpdateSpiritAsync(int id, SpiritUpdateDto updateDto)
        {
            var spirit = await _repository.GetByIdAsync(id);
            if (spirit == null) return false;

            spirit.Name = updateDto.Name;
            spirit.Type = updateDto.Type;
            spirit.Abv = updateDto.Abv;
            spirit.Description = updateDto.Description;
            spirit.BrandId = updateDto.BrandId;

            await _repository.UpdateAsync(spirit);

            ClearSpiritCache(); // Important: Clear the cache to ensure that updated data is reflected in subsequent queries

            return true;
        }

        public async Task<bool> DeleteSpiritAsync(int id)
        {
            var spirit = await _repository.GetByIdAsync(id);
            if (spirit == null) return false;

            await _repository.DeleteAsync(spirit.Id);

            ClearSpiritCache(); // Important: Clear the cache to ensure that deleted data is not shown in subsequent queries

            return true;
        }

        private void ClearSpiritCache()
        {
            _cache.Remove($"{CacheKeyPrefix}p1_s10_tall"); // ImemoryCache does not support "Clear all" so we removethe the first page to ensure the default view stays fresh
        }
    }
}

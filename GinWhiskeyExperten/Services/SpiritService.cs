using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Repositories;
using GinWhiskeyExperten.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace GinWhiskeyExperten.Services
{
    public class SpiritService : ISpiritService
    {
        private readonly ISpiritRepository _repository;
        private readonly IFlavorRepository _flavorRepository;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "spirits_list"; // Prefix for cache keys to group related cache entries
        private const string TopRatedCacheKeyPrefix = "spirits_top";

        // See CacheKeyTracking - static + keyed by the cache instance because SpiritService is
        // registered Scoped (one instance per request) while IMemoryCache is a singleton in
        // production, so a plain instance field would reset every request.
        private static readonly ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> _trackedKeys = new();

        public SpiritService(ISpiritRepository repository, IFlavorRepository flavorRepository, IMemoryCache cache)
        {
            _repository = repository;
            _flavorRepository = flavorRepository;
            _cache = cache;
        }

        public async Task<PagedResponse<SpiritReadDto>> GetSpiritsAsync(int page, int pageSize, string? type)
        {
            string cacheKey = BuildCacheKey(page, pageSize, type);

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
                CacheKeyTracking.Track(_trackedKeys, _cache, cacheKey);
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
            ClearSpiritCache(); // Ensure new item appears in subsequent queries

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

        // "Smart Match": ranks other spirits by Euclidean distance over flavor intensities shared
        // with the target. A flavor a spirit doesn't have is "unknown", not "0" - encoding absence
        // as zero would unfairly penalize sparsely-tagged spirits, so distance is computed only
        // over the intersection of assigned flavors. Candidates with zero shared flavors are
        // excluded rather than scored, since a manufactured worst-case distance would produce a
        // confidently-wrong "top N" on a small catalog. Ties break on shared-flavor-count (more
        // overlap wins), then Id, so results are deterministic.
        public async Task<List<SpiritReadDto>> GetRecommendationsAsync(int spiritId, int count = 3)
        {
            var target = await _repository.GetByIdWithFlavorsAsync(spiritId);
            if (target == null || target.SpiritFlavors.Count == 0) return new List<SpiritReadDto>();

            var targetIntensities = target.SpiritFlavors.ToDictionary(sf => sf.FlavorId, sf => sf.Intensity);
            var candidates = await _repository.GetAllWithFlavorsExceptAsync(spiritId);

            return candidates
                .Select(c =>
                {
                    var shared = c.SpiritFlavors.Where(sf => targetIntensities.ContainsKey(sf.FlavorId)).ToList();
                    var sumSquares = shared.Sum(sf => Math.Pow(targetIntensities[sf.FlavorId] - sf.Intensity, 2));
                    return (Spirit: c, Distance: Math.Sqrt(sumSquares), SharedCount: shared.Count);
                })
                .Where(x => x.SharedCount > 0)
                .OrderBy(x => x.Distance)
                .ThenByDescending(x => x.SharedCount)
                .ThenBy(x => x.Spirit.Id)
                .Take(count)
                .Select(x => new SpiritReadDto(x.Spirit.Id, x.Spirit.Name, x.Spirit.Type, x.Spirit.Abv,
                    x.Spirit.Description, x.Spirit.Brand?.Name ?? "Unknown brand", x.Spirit.CreatedAt))
                .ToList();
        }

        public async Task<bool> AssignFlavorAsync(int spiritId, int flavorId, int intensity)
        {
            var flavor = await _flavorRepository.GetByIdAsync(flavorId);
            if (flavor == null) return false;

            return await _repository.UpsertSpiritFlavorAsync(spiritId, flavorId, intensity);
        }

        public async Task<bool> RemoveFlavorAsync(int spiritId, int flavorId) =>
            await _repository.RemoveSpiritFlavorAsync(spiritId, flavorId);

        public async Task<bool> CastVoteAsync(int spiritId, int stars)
        {
            var success = await _repository.AddVoteAsync(spiritId, stars);
            if (success) ClearSpiritCache(); // a new vote changes the ranking
            return success;
        }

        public async Task<List<SpiritRankingDto>> GetTopRatedAsync(int count = 50)
        {
            string cacheKey = $"{TopRatedCacheKeyPrefix}_{count}";

            if (!_cache.TryGetValue(cacheKey, out List<SpiritRankingDto>? cached))
            {
                var spirits = await _repository.GetAllWithVotesAsync();

                cached = spirits
                    .Where(s => s.SpiritVotes.Count > 0) // exclude spirits with no votes yet, rather than showing a misleading 0/5
                    .Select(s => new SpiritRankingDto(
                        s.Id, s.Name, s.Type, s.Abv, s.Brand?.Name ?? "Unknown brand",
                        s.SpiritVotes.Average(v => v.Stars),
                        s.SpiritVotes.Count))
                    .OrderByDescending(r => r.AverageRating)
                    .ThenByDescending(r => r.VoteCount)
                    .ThenBy(r => r.Id)
                    .Take(count)
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, cached, cacheOptions);
                CacheKeyTracking.Track(_trackedKeys, _cache, cacheKey);
            }

            return cached!;
        }

        private static string BuildCacheKey(int page, int pageSize, string? type) =>
            $"{CacheKeyPrefix}_p{page}_s{pageSize}_t{type ?? "all"}";

        private void ClearSpiritCache() => CacheKeyTracking.InvalidateAll(_trackedKeys, _cache);
    }
}

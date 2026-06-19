using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace GinWhiskeyExperten.Services
{
    // Shared "tracked-key-set" cache invalidation logic used by SpiritService/BrandService/
    // FlavorService: each list-read records the key it issued, and a write clears every key
    // actually issued instead of guessing one key shape (the bug class found in the original
    // SpiritService.ClearSpiritCache, which built a key that silently didn't match the write path).
    // Each service owns its own ConditionalWeakTable so a write to one entity doesn't invalidate
    // another entity's unrelated cache entries.
    internal static class CacheKeyTracking
    {
        public static void Track(
            ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> table,
            IMemoryCache cache,
            string key) =>
            table.GetOrCreateValue(cache).TryAdd(key, 0);

        public static void InvalidateAll(
            ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> table,
            IMemoryCache cache)
        {
            var keys = table.GetOrCreateValue(cache);
            foreach (var key in keys.Keys.ToList())
            {
                cache.Remove(key);
            }
            keys.Clear();
        }
    }
}

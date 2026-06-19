using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Services;
using GinWhiskeyExperten.Tests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;

namespace GinWhiskeyExperten.Tests.Services
{
    // Coverage for the "Smart Match" recommendation algorithm: Euclidean distance over shared
    // flavor intensities, excluding non-overlapping candidates, with deterministic tie-breaking.
    public class SpiritServiceRecommendationsTests
    {
        [Fact]
        public async Task GetRecommendationsAsync_OrdersByDistance_AndExcludesNonOverlappingCandidates()
        {
            var target = MakeSpirit(1, (1, 3), (2, 4));
            var identical = MakeSpirit(2, (1, 3), (2, 4));     // distance 0 - closest
            var farther = MakeSpirit(3, (1, 5), (2, 2));       // distance sqrt(8)
            var noOverlap = MakeSpirit(4, (3, 5));              // shares nothing with target - excluded
            var noFlavorsAtAll = MakeSpirit(5);                 // excluded

            var repository = new FakeSpiritRepository(new[] { target, identical, farther, noOverlap, noFlavorsAtAll });
            var service = CreateService(repository);

            var results = await service.GetRecommendationsAsync(1);

            Assert.Equal(new[] { 2, 3 }, results.Select(r => r.Id));
        }

        [Fact]
        public async Task GetRecommendationsAsync_TargetHasNoFlavorsAssigned_ReturnsEmpty()
        {
            var target = MakeSpirit(1); // no flavors assigned yet
            var other = MakeSpirit(2, (1, 3));

            var repository = new FakeSpiritRepository(new[] { target, other });
            var service = CreateService(repository);

            var results = await service.GetRecommendationsAsync(1);

            Assert.Empty(results);
        }

        [Fact]
        public async Task GetRecommendationsAsync_TiesBreakBySharedFlavorCountThenById()
        {
            var target = MakeSpirit(1, (1, 3), (2, 3));
            // Both candidates end up at exactly distance 1 from the target:
            var fewerSharedFlavors = MakeSpirit(31, (1, 4));            // 1 shared flavor, diff^2 = 1 -> distance 1
            var moreSharedFlavors = MakeSpirit(32, (1, 4), (2, 3));     // 2 shared flavors, diff^2 sum = 1 -> distance 1

            var repository = new FakeSpiritRepository(new[] { target, fewerSharedFlavors, moreSharedFlavors });
            var service = CreateService(repository);

            var results = await service.GetRecommendationsAsync(1);

            // Equal distance -> more shared flavors wins, even though its Id (32) is higher.
            Assert.Equal(new[] { 32, 31 }, results.Select(r => r.Id));
        }

        private static SpiritService CreateService(FakeSpiritRepository repository) =>
            new(repository, new FakeFlavorRepository(), new MemoryCache(new MemoryCacheOptions()));

        private static Spirit MakeSpirit(int id, params (int flavorId, int intensity)[] flavors)
        {
            var spirit = new Spirit { Id = id, Name = $"Spirit {id}", Type = "Gin", Abv = 40, BrandId = 1 };
            foreach (var (flavorId, intensity) in flavors)
            {
                spirit.SpiritFlavors.Add(new SpiritFlavor { SpiritId = id, FlavorId = flavorId, Intensity = intensity });
            }
            return spirit;
        }
    }
}

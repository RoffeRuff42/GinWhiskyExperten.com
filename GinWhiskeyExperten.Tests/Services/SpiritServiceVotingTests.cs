using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Services;
using GinWhiskeyExperten.Tests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;

namespace GinWhiskeyExperten.Tests.Services
{
    public class SpiritServiceVotingTests
    {
        [Fact]
        public async Task GetTopRatedAsync_OrdersByAverageRating_ExcludesUnvotedSpirits()
        {
            var highRated = MakeSpirit(1, 5, 5, 5);
            var lowRated = MakeSpirit(2, 2, 3);
            var unvoted = MakeSpirit(3);

            var repository = new FakeSpiritRepository(new[] { highRated, lowRated, unvoted });
            var service = CreateService(repository);

            var ranking = await service.GetTopRatedAsync();

            Assert.Equal(new[] { 1, 2 }, ranking.Select(r => r.Id));
            Assert.Equal(5.0, ranking[0].AverageRating);
            Assert.Equal(2.5, ranking[1].AverageRating);
        }

        [Fact]
        public async Task CastVoteAsync_OnUnknownSpirit_ReturnsFalse()
        {
            var repository = new FakeSpiritRepository();
            var service = CreateService(repository);

            var success = await service.CastVoteAsync(spiritId: 999, stars: 5);

            Assert.False(success);
        }

        [Fact]
        public async Task CastVoteAsync_InvalidatesTopRatedCache()
        {
            var spirit = MakeSpirit(1, 3);
            var repository = new FakeSpiritRepository(new[] { spirit });
            var service = CreateService(repository);

            var before = await service.GetTopRatedAsync();
            Assert.Equal(3.0, before[0].AverageRating);

            await service.CastVoteAsync(1, 5); // average should move from 3 to 4

            var after = await service.GetTopRatedAsync();
            Assert.Equal(4.0, after[0].AverageRating);
        }

        private static SpiritService CreateService(FakeSpiritRepository repository) =>
            new(repository, new FakeFlavorRepository(), new MemoryCache(new MemoryCacheOptions()));

        private static Spirit MakeSpirit(int id, params int[] votes)
        {
            var spirit = new Spirit { Id = id, Name = $"Spirit {id}", Type = "Gin", Abv = 40, BrandId = 1 };
            foreach (var stars in votes)
            {
                spirit.SpiritVotes.Add(new SpiritVote { SpiritId = id, Stars = stars });
            }
            return spirit;
        }
    }
}

using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Repositories;

namespace GinWhiskeyExperten.Tests.TestDoubles
{
    public class FakeSpiritRepository : ISpiritRepository
    {
        private readonly List<Spirit> _spirits;

        public int GetAllAsyncCallCount { get; private set; }

        public FakeSpiritRepository(IEnumerable<Spirit>? seed = null)
        {
            _spirits = seed?.ToList() ?? new List<Spirit>();
        }

        public Task<(IEnumerable<Spirit> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? type)
        {
            GetAllAsyncCallCount++;

            var filtered = string.IsNullOrWhiteSpace(type)
                ? _spirits.AsEnumerable()
                : _spirits.Where(s => s.Type == type);

            var all = filtered.ToList();
            var pageItems = all.Skip((page - 1) * pageSize).Take(pageSize);

            return Task.FromResult(((IEnumerable<Spirit>)pageItems, all.Count));
        }

        public Task<Spirit?> GetByIdAsync(int id) =>
            Task.FromResult(_spirits.FirstOrDefault(s => s.Id == id));

        public Task AddAsync(Spirit spirit)
        {
            if (spirit.Id == 0) spirit.Id = _spirits.Count == 0 ? 1 : _spirits.Max(s => s.Id) + 1;
            _spirits.Add(spirit);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Spirit spirit) => Task.CompletedTask; // already the tracked in-memory instance

        public Task DeleteAsync(int id)
        {
            var spirit = _spirits.FirstOrDefault(s => s.Id == id);
            if (spirit != null) _spirits.Remove(spirit);
            return Task.CompletedTask;
        }

        public Task<Spirit?> GetByIdWithFlavorsAsync(int id) => GetByIdAsync(id);

        public Task<List<Spirit>> GetAllWithFlavorsExceptAsync(int excludeId) =>
            Task.FromResult(_spirits.Where(s => s.Id != excludeId).ToList());

        public Task<bool> UpsertSpiritFlavorAsync(int spiritId, int flavorId, int intensity)
        {
            var spirit = _spirits.FirstOrDefault(s => s.Id == spiritId);
            if (spirit == null) return Task.FromResult(false);

            var existing = spirit.SpiritFlavors.FirstOrDefault(sf => sf.FlavorId == flavorId);
            if (existing != null)
            {
                existing.Intensity = intensity;
            }
            else
            {
                spirit.SpiritFlavors.Add(new SpiritFlavor { SpiritId = spiritId, FlavorId = flavorId, Intensity = intensity });
            }

            return Task.FromResult(true);
        }

        public Task<bool> RemoveSpiritFlavorAsync(int spiritId, int flavorId)
        {
            var spirit = _spirits.FirstOrDefault(s => s.Id == spiritId);
            var existing = spirit?.SpiritFlavors.FirstOrDefault(sf => sf.FlavorId == flavorId);
            if (existing == null) return Task.FromResult(false);

            spirit!.SpiritFlavors.Remove(existing);
            return Task.FromResult(true);
        }

        public Task<bool> AddVoteAsync(int spiritId, int stars)
        {
            var spirit = _spirits.FirstOrDefault(s => s.Id == spiritId);
            if (spirit == null) return Task.FromResult(false);

            spirit.SpiritVotes.Add(new SpiritVote { SpiritId = spiritId, Stars = stars });
            return Task.FromResult(true);
        }

        public Task<List<Spirit>> GetAllWithVotesAsync() => Task.FromResult(_spirits.ToList());
    }
}

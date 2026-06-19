using GinWhiskeyExperten.Models;
using GinWhiskeyExperten.Repositories;

namespace GinWhiskeyExperten.Tests.TestDoubles
{
    public class FakeFlavorRepository : IFlavorRepository
    {
        private readonly List<Flavor> _flavors;

        public FakeFlavorRepository(IEnumerable<Flavor>? seed = null)
        {
            _flavors = seed?.ToList() ?? new List<Flavor>();
        }

        public Task<IEnumerable<Flavor>> GetAllAsync() =>
            Task.FromResult(_flavors.AsEnumerable());

        public Task<Flavor?> GetByIdAsync(int id) =>
            Task.FromResult(_flavors.FirstOrDefault(f => f.Id == id));

        public Task AddAsync(Flavor flavor)
        {
            if (flavor.Id == 0) flavor.Id = _flavors.Count == 0 ? 1 : _flavors.Max(f => f.Id) + 1;
            _flavors.Add(flavor);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Flavor flavor) => Task.CompletedTask;

        public Task DeleteAsync(int id)
        {
            var flavor = _flavors.FirstOrDefault(f => f.Id == id);
            if (flavor != null) _flavors.Remove(flavor);
            return Task.CompletedTask;
        }
    }
}

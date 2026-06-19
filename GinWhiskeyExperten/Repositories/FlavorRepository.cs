using GinWhiskeyExperten.Data;
using GinWhiskeyExperten.Models;
using Microsoft.EntityFrameworkCore;

namespace GinWhiskeyExperten.Repositories
{
    public class FlavorRepository : IFlavorRepository
    {
        private readonly ApplicationDbContext _context;

        public FlavorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flavor>> GetAllAsync() =>
            await _context.Flavors.ToListAsync();

        public async Task<Flavor?> GetByIdAsync(int id) =>
            await _context.Flavors.FirstOrDefaultAsync(f => f.Id == id);

        public async Task AddAsync(Flavor flavor)
        {
            await _context.Flavors.AddAsync(flavor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Flavor flavor)
        {
            _context.Flavors.Update(flavor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var flavor = await GetByIdAsync(id);
            if (flavor != null)
            {
                _context.Flavors.Remove(flavor);
                await _context.SaveChangesAsync();
            }
        }
    }
}

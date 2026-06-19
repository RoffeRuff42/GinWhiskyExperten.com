using GinWhiskeyExperten.Data;
using GinWhiskeyExperten.Models;
using Microsoft.EntityFrameworkCore;

namespace GinWhiskeyExperten.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllAsync() =>
            await _context.Brands.ToListAsync();

        public async Task<Brand?> GetByIdAsync(int id) =>
            await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

        public async Task AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        // Deleting a Brand cascades to delete its Spirits (FK_Spirits_Brands_BrandId is configured
        // ON DELETE CASCADE in the original migration) - pre-existing behavior, now reachable here.
        public async Task DeleteAsync(int id)
        {
            var brand = await GetByIdAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }
    }
}

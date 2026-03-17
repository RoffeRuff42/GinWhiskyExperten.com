using GinWhiskeyExperten.Data;
using GinWhiskeyExperten.Models;
using Microsoft.EntityFrameworkCore;

namespace GinWhiskeyExperten.Repositories
{
    public class SpiritRepository : ISpiritRepository
    {
        private readonly ApplicationDbContext _context;

        public SpiritRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Spirit> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? type)
        {
            var query = _context.Spirits.Include(s => s.Brand).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(s => s.Type == type);
            }

            var totalCount = await query.CountAsync();

            var items = await query 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Spirit?> GetByIdAsync(int id) =>
            await _context.Spirits.Include(s => s.Brand).FirstOrDefaultAsync(s => s.Id == id);

        public async Task AddAsync(Spirit spirit)
        {
            await _context.Spirits.AddAsync(spirit);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Spirit spirit)
        {
            _context.Spirits.Update(spirit);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var spirit = await GetByIdAsync(id);
            if (spirit != null)
            {
                _context.Spirits.Remove(spirit);
                await _context.SaveChangesAsync();
            }
        }
    }
}

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

        public async Task<Spirit?> GetByIdWithFlavorsAsync(int id) =>
            await _context.Spirits.Include(s => s.Brand).Include(s => s.SpiritFlavors)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<Spirit>> GetAllWithFlavorsExceptAsync(int excludeId) =>
            await _context.Spirits.Include(s => s.Brand).Include(s => s.SpiritFlavors)
                .Where(s => s.Id != excludeId).ToListAsync();

        public async Task<bool> UpsertSpiritFlavorAsync(int spiritId, int flavorId, int intensity)
        {
            var spiritExists = await _context.Spirits.AnyAsync(s => s.Id == spiritId);
            if (!spiritExists) return false;

            var existing = await _context.SpiritFlavors
                .FirstOrDefaultAsync(sf => sf.SpiritId == spiritId && sf.FlavorId == flavorId);

            if (existing != null)
            {
                existing.Intensity = intensity;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                await _context.SpiritFlavors.AddAsync(new SpiritFlavor
                {
                    SpiritId = spiritId,
                    FlavorId = flavorId,
                    Intensity = intensity,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSpiritFlavorAsync(int spiritId, int flavorId)
        {
            var existing = await _context.SpiritFlavors
                .FirstOrDefaultAsync(sf => sf.SpiritId == spiritId && sf.FlavorId == flavorId);
            if (existing == null) return false;

            _context.SpiritFlavors.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

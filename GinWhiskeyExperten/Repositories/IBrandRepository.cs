using GinWhiskeyExperten.Models;

namespace GinWhiskeyExperten.Repositories
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(int id);
        Task AddAsync(Brand brand);
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(int id);
    }
}

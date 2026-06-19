using GinWhiskeyExperten.Models;

namespace GinWhiskeyExperten.Repositories
{
    public interface IFlavorRepository
    {
        Task<IEnumerable<Flavor>> GetAllAsync();
        Task<Flavor?> GetByIdAsync(int id);
        Task AddAsync(Flavor flavor);
        Task UpdateAsync(Flavor flavor);
        Task DeleteAsync(int id);
    }
}

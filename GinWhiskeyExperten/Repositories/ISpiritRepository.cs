using GinWhiskeyExperten.Models;

namespace GinWhiskeyExperten.Repositories
{
    public interface ISpiritRepository
    {
        Task<(IEnumerable<Spirit> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? type); // <()> = ValueTuple with named elements, allowing us to return both the list of spirits and the total count in one method. The 'type' parameter is optional and can be used to filter spirits by their type (e.g., Gin, Whiskey).
        Task<Spirit?> GetByIdAsync(int id);
        Task AddAsync(Spirit spirit);
        Task UpdateAsync(Spirit spirit);
        Task DeleteAsync(int id);
        
    }
}

using GinWhiskeyExperten.DTOs;

namespace GinWhiskeyExperten.Services
{
    public interface IFlavorService
    {
        Task<IEnumerable<FlavorReadDto>> GetFlavorsAsync();
        Task<FlavorReadDto?> GetFlavorByIdAsync(int id);
        Task<FlavorReadDto> CreateFlavorAsync(FlavorCreateDto createDto);
        Task<bool> UpdateFlavorAsync(int id, FlavorUpdateDto updateDto);
        Task<bool> DeleteFlavorAsync(int id);
    }
}

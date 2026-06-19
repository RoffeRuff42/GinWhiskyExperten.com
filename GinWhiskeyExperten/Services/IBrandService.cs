using GinWhiskeyExperten.DTOs;

namespace GinWhiskeyExperten.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandReadDto>> GetBrandsAsync();
        Task<BrandReadDto?> GetBrandByIdAsync(int id);
        Task<BrandReadDto> CreateBrandAsync(BrandCreateDto createDto);
        Task<bool> UpdateBrandAsync(int id, BrandUpdateDto updateDto);
        Task<bool> DeleteBrandAsync(int id);
    }
}

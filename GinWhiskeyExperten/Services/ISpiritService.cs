using GinWhiskeyExperten.DTOs;

namespace GinWhiskeyExperten.Services
{
    public interface ISpiritService
    {
        Task<PagedResponse<SpiritReadDto>> GetSpiritsAsync(int page, int pageSize, string? type);
        Task<SpiritReadDto?> GetSpiritByIdAsync(int id);
        Task<SpiritReadDto> CreateSpiritAsync(SpiritCreateDto createDto);
        Task<bool> UpdateSpiritAsync(int id, SpiritUpdateDto updateDto);
        Task<bool> DeleteSpiritAsync(int id);

        Task<List<SpiritReadDto>> GetRecommendationsAsync(int spiritId, int count = 3);
        Task<bool> AssignFlavorAsync(int spiritId, int flavorId, int intensity);
        Task<bool> RemoveFlavorAsync(int spiritId, int flavorId);
    }
}

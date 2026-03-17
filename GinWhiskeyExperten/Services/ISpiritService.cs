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
    }
}

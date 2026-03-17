using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.DTOs
{
    // First to define the DTOs for Spirits
    public record SpiritReadDto(
        int Id,
        string Name,
        string Type,
        decimal Abv,
        string? Description,
        string BrandName,
        DateTime CreatedAt
    );

    // Second to define the DTO for creating a new Spirit
    public record SpiritCreateDto(
        [Required(ErrorMessage = "Name is needed")]
        [StringLength(100, MinimumLength = 2)]
        string Name,

        [Required]
        string Type,

        [Range(0, 100)]
        decimal Abv,

        [StringLength(500)]
        string? Description,

        [Required]
        int BrandId
    );

    // Third to define the DTO for updating an existing Spirit
    public record SpiritUpdateDto(
        [Required]
        [StringLength(100)]
        string Name,

        [Required]
        string Type,

        [Range(0, 100)]
        decimal Abv,

        [StringLength(500)]
        string? Description,

        [Required]
        int BrandId
    );

    // For pagination response
    public record PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}

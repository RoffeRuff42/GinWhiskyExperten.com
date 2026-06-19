using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.DTOs
{
    public record BrandReadDto(int Id, string Name, string? Website, string Country, DateTime CreatedAt);

    public record BrandCreateDto(
        [Required, StringLength(100, MinimumLength = 2)] string Name,
        [Url] string? Website,
        [Required] string Country
    );

    public record BrandUpdateDto(
        [Required, StringLength(100, MinimumLength = 2)] string Name,
        [Url] string? Website,
        [Required] string Country
    );
}

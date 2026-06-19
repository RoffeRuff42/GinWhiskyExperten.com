using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.DTOs
{
    public record FlavorReadDto(int Id, string Name, string? Description, DateTime CreatedAt);

    public record FlavorCreateDto(
        [Required, StringLength(50, MinimumLength = 2)] string Name,
        [StringLength(200)] string? Description
    );

    public record FlavorUpdateDto(
        [Required, StringLength(50, MinimumLength = 2)] string Name,
        [StringLength(200)] string? Description
    );
}

using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.DTOs
{
    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponseDto(string Token, DateTime ExpiresAtUtc);
}

using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GinWhiskeyExperten.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthPolicy")] // Stricter than the default policy - login is a brute-force target
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new { Message = "Invalid credentials." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!result.Succeeded) return Unauthorized(new { Message = "Invalid credentials." });

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAtUtc) = GenerateJwtToken(user, roles);

            return Ok(new AuthResponseDto(token, expiresAtUtc));
        }

        private (string Token, DateTime ExpiresAtUtc) GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }
}

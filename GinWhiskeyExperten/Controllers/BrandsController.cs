using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GinWhiskeyExperten.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("FixedWindowPolicy")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        // GET: api/brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandReadDto>>> GetBrands()
        {
            var brands = await _brandService.GetBrandsAsync();
            return Ok(brands);
        }

        // GET: api/brands/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandReadDto>> GetBrand(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound(new { Message = $"Couldn't find brand with matching id {id}" });
            return Ok(brand);
        }

        // POST: api/brands
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BrandReadDto>> CreateBrand(BrandCreateDto createDto)
        {
            var created = await _brandService.CreateBrandAsync(createDto);
            return CreatedAtAction(nameof(GetBrand), new { id = created.Id }, created);
        }

        // PUT: api/brands/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBrand(int id, BrandUpdateDto updateDto)
        {
            var success = await _brandService.UpdateBrandAsync(id, updateDto);
            if (!success) return NotFound(new { Message = $"Update failed. Brand with ID {id} not found." });
            return NoContent();
        }

        // DELETE: api/brands/5
        // Note: deleting a brand cascades to delete its Spirits (pre-existing FK behavior).
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var success = await _brandService.DeleteBrandAsync(id);
            if (!success) return NotFound(new { Message = $"Delete failed. Brand with ID {id} not found." });
            return NoContent();
        }
    }
}

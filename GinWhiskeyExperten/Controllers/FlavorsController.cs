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
    public class FlavorsController : ControllerBase
    {
        private readonly IFlavorService _flavorService;

        public FlavorsController(IFlavorService flavorService)
        {
            _flavorService = flavorService;
        }

        // GET: api/flavors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlavorReadDto>>> GetFlavors()
        {
            var flavors = await _flavorService.GetFlavorsAsync();
            return Ok(flavors);
        }

        // GET: api/flavors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlavorReadDto>> GetFlavor(int id)
        {
            var flavor = await _flavorService.GetFlavorByIdAsync(id);
            if (flavor == null) return NotFound(new { Message = $"Couldn't find flavor with matching id {id}" });
            return Ok(flavor);
        }

        // POST: api/flavors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FlavorReadDto>> CreateFlavor(FlavorCreateDto createDto)
        {
            var created = await _flavorService.CreateFlavorAsync(createDto);
            return CreatedAtAction(nameof(GetFlavor), new { id = created.Id }, created);
        }

        // PUT: api/flavors/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFlavor(int id, FlavorUpdateDto updateDto)
        {
            var success = await _flavorService.UpdateFlavorAsync(id, updateDto);
            if (!success) return NotFound(new { Message = $"Update failed. Flavor with ID {id} not found." });
            return NoContent();
        }

        // DELETE: api/flavors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFlavor(int id)
        {
            var success = await _flavorService.DeleteFlavorAsync(id);
            if (!success) return NotFound(new { Message = $"Delete failed. Flavor with ID {id} not found." });
            return NoContent();
        }
    }
}

using GinWhiskeyExperten.DTOs;
using GinWhiskeyExperten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GinWhiskeyExperten.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Becomes "api/spirits"
    [EnableRateLimiting("FixedWindowPolicy")] // Apply rate limiting to all endpoints in this controller
    public class SpiritsController : ControllerBase
    {
        private readonly ISpiritService _spiritService;

        public SpiritsController(ISpiritService spiritService)
        {
            _spiritService = spiritService;
        }

        // GET: api/spirits?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedResponse<SpiritReadDto>>> GetSpirits(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null) //Optional type parameter
        {
            // Validation of query parameters with default values
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 50) pageSize = 10;

            var response = await _spiritService.GetSpiritsAsync(page, pageSize, type);
            return Ok(response);
        }

        // GET: api/spirits/top?count=50 - must be declared with a literal segment ahead of {id:int}
        // below, and {id} must be constrained to :int, or a request for "top" would otherwise try
        // to bind "top" to the int id parameter on GetSpirit instead of reaching this action.
        [HttpGet("top")]
        public async Task<ActionResult<List<SpiritRankingDto>>> GetTopRated([FromQuery] int count = 50)
        {
            if (count <= 0 || count > 100) count = 50;

            var ranking = await _spiritService.GetTopRatedAsync(count);
            return Ok(ranking);
        }

        // GET: api/spirits/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SpiritReadDto>> GetSpirit(int id, [FromServices] SystembolagetIntegrationService systemService)
        {
            var spirit = await _spiritService.GetSpiritByIdAsync(id);

            if (spirit == null)
            {
                return NotFound(new { Message = $"Couldnt find spirit with matching id {id}" });
            }

            var stockStatus = await systemService.GetStockStatusAsync("12345"); // Replace "12345" with the actual article number associated with the spirit

            return Ok(new
            {
                Data = spirit,
                StoreInformation = stockStatus
            });
        }

        [HttpGet("{id}/cocktails")]
        public async Task<ActionResult<List<CocktailDto>>> GetCocktailsForSpirit(int id, [FromServices] CocktailIntegrationService cocktailService)
        {
            var spirit = await _spiritService.GetSpiritByIdAsync(id); // Fetch the spirit from the database
            if (spirit == null) return NotFound("Spirit not found.");

            var suggestions = await cocktailService.GetSmartCocktailSuggestionsAsync(spirit.Name, spirit.Type); // Get cocktail suggestions based on the spirit's name and type

            return Ok(suggestions);
        }

        // POST: api/spirits
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SpiritReadDto>> CreateSpirit(SpiritCreateDto createDto)
        {
            var createdSpirit = await _spiritService.CreateSpiritAsync(createDto); // Create the spirit and get the created entity with its new ID
            return CreatedAtAction(nameof(GetSpirit), new { id = createdSpirit.Id }, createdSpirit); // Return 201 Created with the location of the new resource and the created entity in the response body
        }

        // PUT: api/spirits/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpirit(int id, SpiritUpdateDto updateDto)
        {
            var success = await _spiritService.UpdateSpiritAsync(id, updateDto);

            if (!success)
            {
                return NotFound(new { Message = $"Update failed. Spirit with ID {id} not found." });
            }

            return NoContent();
        }

        // DELETE: api/spirits/{id}
        // REST Standard: Use 204 No Content on success
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpirit(int id)
        {
            var success = await _spiritService.DeleteSpiritAsync(id);

            if (!success)
            {
                return NotFound(new { Message = $"Delete failed. Spirit with ID {id} not found." });
            }

            return NoContent();
        }

        // GET: api/spirits/5/recommendations - "Smart Match": top 3 spirits by flavor-profile similarity.
        // Returns an empty list (not an error) if the spirit has no flavors assigned yet.
        [HttpGet("{id}/recommendations")]
        public async Task<ActionResult<List<SpiritReadDto>>> GetRecommendations(int id)
        {
            var spirit = await _spiritService.GetSpiritByIdAsync(id);
            if (spirit == null) return NotFound(new { Message = $"Spirit {id} not found." });

            var recommendations = await _spiritService.GetRecommendationsAsync(id);
            return Ok(recommendations);
        }

        // PUT: api/spirits/5/flavors - assign or update a flavor's intensity on this spirit (upsert)
        [HttpPut("{id}/flavors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignFlavor(int id, AssignFlavorDto assignDto)
        {
            var success = await _spiritService.AssignFlavorAsync(id, assignDto.FlavorId, assignDto.Intensity);
            if (!success) return NotFound(new { Message = $"Spirit {id} or Flavor {assignDto.FlavorId} not found." });

            return NoContent();
        }

        // DELETE: api/spirits/5/flavors/3 - remove a flavor assignment from this spirit
        [HttpDelete("{id}/flavors/{flavorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFlavor(int id, int flavorId)
        {
            var success = await _spiritService.RemoveFlavorAsync(id, flavorId);
            if (!success) return NotFound(new { Message = $"Spirit {id} has no flavor {flavorId} assigned." });

            return NoContent();
        }

        // POST: api/spirits/5/votes - cast an anonymous 1-5 star vote. No auth, no dedup on the
        // server (see SpiritVote model) - the frontend prevents repeat votes via localStorage.
        [HttpPost("{id}/votes")]
        public async Task<IActionResult> CastVote(int id, CastVoteDto voteDto)
        {
            var success = await _spiritService.CastVoteAsync(id, voteDto.Stars);
            if (!success) return NotFound(new { Message = $"Spirit {id} not found." });

            return NoContent();
        }
    }
}

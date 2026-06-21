using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.Models
{
    // Anonymous - no user identity. Abuse prevention (one vote per spirit per visitor) is handled
    // client-side via localStorage, not enforced here - acceptable tradeoff for a passion project,
    // see conversation: building real per-account vote enforcement was explicitly deferred.
    public class SpiritVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SpiritId { get; set; }
        public Spirit Spirit { get; set; } = null!;

        [Range(1, 5)]
        public int Stars { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        public string InternalNotes { get; set; } = " Review pending"; // Not exposed to customers

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int SpiritId { get; set; }
        public Spirit Spirit { get; set; } = null!; // null! = null-forgiving operator, tells the compiler that this will be set later and won't be null at runtime
    }
}

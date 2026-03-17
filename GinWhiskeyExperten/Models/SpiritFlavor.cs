using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.Models
{
    public class SpiritFlavor
    {
        [Required]
        public int SpiritId { get; set; } // Foreign key to Spirit
        public Spirit Spirit { get; set; } = null!; // null! = null-forgiving operator, tells the compiler that this will be set later and won't be null at runtime

        [Required]
        public int FlavorId { get; set; } // Foreign key to Flavor
        public Flavor Flavor { get; set; } = null!; // null! = null-forgiving operator, tells the compiler that this will be set later and won't be null at runtime

        [Range(1, 5)]
        public int Intensity { get; set; } // Intensity of the flavor in the spirit (1-5)

        public string InternalNotes { get; set; } = "Internal notes for staff only"; // Not exposed to customers

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}

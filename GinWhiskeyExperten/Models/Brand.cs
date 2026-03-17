using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? Website { get; set; } // Optional URL to the brand's website

        [Required]
        public string Country { get; set; } = string.Empty;

        public string InternalNotes { get; set; } = "Internal notes for staff only"; // Not exposed to customers

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // ? = nullable, can be null until updated

        public List<Spirit> Spirits { get; set; } = new();
    }
}

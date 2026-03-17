using System.ComponentModel.DataAnnotations;

namespace GinWhiskeyExperten.Models
{
    public class Flavor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; } // Optional description of the flavor

        public string InternalNotes { get; set; } = "Internal notes for staff only"; // Not exposed to customers

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<SpiritFlavor> SpiritFlavors { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GinWhiskeyExperten.Models
{
    public class Spirit
    {
        [Key]
        public int Id { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // E.g Gin or Whiskey

        [Range(0,100)]
        [Column(TypeName = "decimal(5, 2)")] // Precision for decimal in SQL Server, allows values like 40.00 for ABV
        public decimal Abv { get; set; } // Alcohol by volume

        [StringLength(500)]
        public string? Description { get; set; } = string.Empty; // ? = nullable, can be empty string
        
        public string InternalNotes { get; set; } = "Internal notes for staff only"; // Not exposed to customers

        [Required]
        public int BrandId { get; set; } // Foreign key to Brand
        public Brand Brand { get; set; } = null!; // null! = null-forgiving operator, tells the compiler that this will be set later and won't be null at runtime
        public List<SpiritFlavor> SpiritFlavors { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

    }
}

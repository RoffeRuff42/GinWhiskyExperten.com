using Microsoft.EntityFrameworkCore;
using GinWhiskeyExperten.Models;

namespace GinWhiskeyExperten.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Spirit> Spirits { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Flavor> Flavors { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<SpiritFlavor> SpiritFlavors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Configuring the many-to-many relationship between Spirit and Flavor
        {
            modelBuilder.Entity<SpiritFlavor>()
                .HasKey(sf => new { sf.SpiritId, sf.FlavorId });

            modelBuilder.Entity<SpiritFlavor>()
                .HasOne(sf => sf.Spirit)
                .WithMany(s => s.SpiritFlavors)
                .HasForeignKey(sf => sf.SpiritId);

            modelBuilder.Entity<SpiritFlavor>()
                .HasOne(sf => sf.Flavor)
                .WithMany(f => f.SpiritFlavors)
                .HasForeignKey(sf => sf.FlavorId);

            base.OnModelCreating(modelBuilder);
        }
    }
}

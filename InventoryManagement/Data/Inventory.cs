using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

        public DbSet<Material> Materials { get; set; }
        public DbSet<InventoryMaterial> InventoryMaterials { get; set; }
        public DbSet<History> Histories { get; set; }  // Add this line
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>().ToTable("Materials");
            modelBuilder.Entity<InventoryMaterial>().ToTable("InventoryMaterials");
            modelBuilder.Entity<History>().ToTable("Histories");  // Add this line
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=inventory.db")
                              .LogTo(Console.WriteLine, LogLevel.Information);
            }
        }
    }
}

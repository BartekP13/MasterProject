using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MasterProject.Models;

namespace MasterProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MasterProject.Models.Recipe> Recipe { get; set; }
        public DbSet<MasterProject.Models.Ingredient> Ingredient { get; set; }
        public DbSet<MasterProject.Models.Tag> Tag { get; set; }
        public DbSet<MasterProject.Models.Recipe_Tag> Recipe_Tag { get; set;}
        public DbSet<MasterProject.Models.Ratings> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe_Tag>()
                .HasKey(rt => new { rt.RecipeId, rt.TagId });

            modelBuilder.Entity<Recipe_Tag>()
                .HasOne(rt => rt.Recipe)
                .WithMany(r => r.Recipe_Tag)
                .HasForeignKey(rt => rt.RecipeId);

            modelBuilder.Entity<Recipe_Tag>()
                .HasOne(rt => rt.Tag)
                .WithMany(t => t.Recipe_Tag)
                .HasForeignKey(rt => rt.TagId);
        }

    }
}

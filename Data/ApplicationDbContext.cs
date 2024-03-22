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
    }
}

using Microsoft.EntityFrameworkCore;
using SPS_Code.Data.Models;

namespace SPS_Code.Data
{
    public class CodeDbContext : DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public DbSet<User>? Users { get; set; }
        public DbSet<Models.Task>? Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Task>().HasMany(x => x.User).WithMany(x => x.Tasks);
        }
    }
}

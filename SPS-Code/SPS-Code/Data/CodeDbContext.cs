using Microsoft.EntityFrameworkCore;
using SPS_Code.Data.Models;

namespace SPS_Code.Data
{
    public class CodeDbContext : DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public DbSet<UserModel>? Users { get; set; }
        public DbSet<TaskModel>? Tasks { get; set; }
        public DbSet<UserTaskResult>? UserTaskResult { get; set; }
        public DbSet<UserCategory>? UserCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserModel>().HasMany(u => u.Tasks).WithOne(t => t.User);
            builder.Entity<UserModel>().HasOne(u => u.UserCategory).WithMany();

            builder.Entity<TaskModel>().HasMany(t => t.ViewUserCategories).WithMany();
        }
    }
}

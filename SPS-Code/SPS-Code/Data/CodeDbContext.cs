using Microsoft.EntityFrameworkCore;
using SPS_Code.Data.Models;

namespace SPS_Code.Data
{
    public class CodeDbContext : DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public DbSet<UserModel>? Users { get; set; }
        public DbSet<TaskModel>? Tasks { get; set; }

    }
}

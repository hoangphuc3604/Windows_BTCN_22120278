using Microsoft.EntityFrameworkCore;
using Windows_22120278_Data.models;

namespace Windows_22120278_Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; } = null!;

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=designTime.db");
            }
        }
    }
}


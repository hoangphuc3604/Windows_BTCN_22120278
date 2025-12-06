using Microsoft.EntityFrameworkCore;
using Windows_22120278_Data.models;

namespace Windows_22120278_Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<DrawingBoard> DrawingBoards { get; set; } = null!;
        public DbSet<ShapeEntity> Shapes { get; set; } = null!;
        public DbSet<ShapeTemplate> ShapeTemplates { get; set; } = null!;

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=designTime.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DrawingBoard>()
                .HasMany(d => d.Shapes)
                .WithOne(s => s.DrawingBoard)
                .HasForeignKey(s => s.DrawingBoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany(p => p.DrawingBoards)
                .WithOne(d => d.Profile)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany(p => p.ShapeTemplates)
                .WithOne(t => t.Profile)
                .HasForeignKey(t => t.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


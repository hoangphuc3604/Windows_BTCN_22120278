using Microsoft.EntityFrameworkCore;
using System.IO;
using Windows.Storage;
using Models;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use the application's local folder path and combine it safely to avoid path issues
            string folderPath = ApplicationData.Current.LocalFolder.Path;
            string dbPath = Path.Combine(folderPath, "paint.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}

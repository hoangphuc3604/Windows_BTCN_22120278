using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Windows_22120278_Data;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;

        public ProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Profile>> GetProfilesAsync()
        {
            return await _context.Profiles.ToListAsync();
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<Profile> UpdateProfileAsync(Profile profile)
        {
            var existingProfile = await _context.Profiles.FindAsync(profile.Id);
            if (existingProfile != null)
            {
                existingProfile.Name = profile.Name;
                existingProfile.IsDefaultThemeDark = profile.IsDefaultThemeDark;
                existingProfile.DefaultBoardWidth = profile.DefaultBoardWidth;
                existingProfile.DefaultBoardHeight = profile.DefaultBoardHeight;
                await _context.SaveChangesAsync();
                return existingProfile;
            }
            return profile;
        }

        public async Task DeleteProfileAsync(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile != null)
            {
                _context.Profiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }
    }
}



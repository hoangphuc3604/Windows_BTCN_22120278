using System.Collections.Generic;
using System.Threading.Tasks;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface IProfileService
    {
        Task<List<Profile>> GetProfilesAsync();
        Task<Profile> AddProfileAsync(Profile profile);
        Task DeleteProfileAsync(int id);
    }
}


using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows_22120278.Services;
using Windows_22120278_Data.models;

namespace Windows_22120278.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IProfileService _profileService;

        [ObservableProperty]
        private ObservableCollection<Profile> profiles = new();

        public ProfileViewModel(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [RelayCommand]
        private async Task LoadProfilesAsync()
        {
            var profilesList = await _profileService.GetProfilesAsync();
            Profiles.Clear();
            foreach (var profile in profilesList)
            {
                Profiles.Add(profile);
            }
        }

        [RelayCommand]
        private async Task AddDefaultProfileAsync()
        {
            var newProfile = new Profile
            {
                Name = "New Profile",
                IsDefaultThemeDark = false,
                DefaultBoardWidth = 800,
                DefaultBoardHeight = 600
            };

            var addedProfile = await _profileService.AddProfileAsync(newProfile);
            Profiles.Add(addedProfile);
        }
    }
}


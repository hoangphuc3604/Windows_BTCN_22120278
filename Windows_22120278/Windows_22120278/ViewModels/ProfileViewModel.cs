using System;
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
        private readonly ISelectedProfileService _selectedProfileService;

        [ObservableProperty]
        private ObservableCollection<Profile> profiles = new();

        public ProfileViewModel(IProfileService profileService, ISelectedProfileService selectedProfileService)
        {
            _profileService = profileService;
            _selectedProfileService = selectedProfileService;
        }

        [RelayCommand]
        private async Task LoadProfilesAsync()
        {
            try
            {
            var profilesList = await _profileService.GetProfilesAsync();
            Profiles.Clear();
            foreach (var profile in profilesList)
            {
                Profiles.Add(profile);
                }
                
                // If there are profiles and no profile is currently selected, 
                // automatically select the first profile to enable Dashboard
                if (profilesList.Count > 0 && _selectedProfileService.SelectedProfile == null)
                {
                    _selectedProfileService.SelectedProfile = profilesList[0];
                }
            }
            catch (Exception ex)
            {
                // Log error - in production, use proper logging
                System.Diagnostics.Debug.WriteLine($"Error loading profiles: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddDefaultProfileAsync()
        {
            try
            {
            var newProfile = new Profile
            {
                Name = "New Profile",
                IsDefaultThemeDark = false,
                DefaultBoardWidth = 800,
                DefaultBoardHeight = 600
            };

            var addedProfile = await _profileService.AddProfileAsync(newProfile);
                if (addedProfile != null)
                {
            Profiles.Add(addedProfile);
                    // Set the newly created profile as selected to enable navigation buttons
                    _selectedProfileService.SelectedProfile = addedProfile;
                }
            }
            catch (Exception ex)
            {
                // Log error - in production, use proper logging
                System.Diagnostics.Debug.WriteLine($"Error adding profile: {ex.Message}");
                throw; // Re-throw to let UI handle it
            }
        }

        [RelayCommand]
        private async Task EditProfileAsync(Profile profile)
        {
            if (profile == null)
                return;

            await Task.CompletedTask;
        }

        public async Task<bool> UpdateProfileAsync(Profile originalProfile, Profile updatedProfile)
        {
            try
            {
                var savedProfile = await _profileService.UpdateProfileAsync(updatedProfile);
                var index = Profiles.IndexOf(originalProfile);
                if (index >= 0)
                {
                    Profiles[index] = savedProfile;
                }
                
                if (_selectedProfileService.SelectedProfile?.Id == savedProfile.Id)
                {
                    _selectedProfileService.SelectedProfile = savedProfile;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        [RelayCommand]
        private async Task DeleteProfileAsync(Profile profile)
        {
            if (profile != null)
            {
                await _profileService.DeleteProfileAsync(profile.Id);
                Profiles.Remove(profile);
                
                // If the deleted profile was the selected one, select another profile or clear selection
                if (_selectedProfileService.SelectedProfile?.Id == profile.Id)
                {
                    if (Profiles.Count > 0)
                    {
                        // Select the first remaining profile
                        _selectedProfileService.SelectedProfile = Profiles[0];
                    }
                    else
                    {
                        // No profiles left, clear selection
                        _selectedProfileService.SelectedProfile = null;
                    }
                }
            }
        }
    }
}


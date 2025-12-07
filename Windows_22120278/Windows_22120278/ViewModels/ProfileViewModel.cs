using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows_22120278.Services;
using Windows_22120278_Data.models;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.Views;
using Microsoft.UI.Xaml; // add for FrameworkElement

namespace Windows_22120278.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IProfileService _profileService;
        private readonly ISelectedProfileService _selectedProfileService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<Profile> profiles = new();

        [ObservableProperty]
        private Profile? selectedProfile;

        public ProfileViewModel(IProfileService profileService, ISelectedProfileService selectedProfileService, INavigationService navigationService)
        {
            _profileService = profileService;
            _selectedProfileService = selectedProfileService;
            _navigationService = navigationService;
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
        private async Task CreateProfileAsync()
        {
            try
            {
                var newProfile = new Profile
                {
                    Name = $"Profile {Profiles.Count + 1}",
                    IsDefaultThemeDark = false,
                    DefaultBoardWidth = 800,
                    DefaultBoardHeight = 600
                };

                var addedProfile = await _profileService.AddProfileAsync(newProfile);
                if (addedProfile != null)
                {
                    Profiles.Add(addedProfile);
                    SelectedProfile = addedProfile;
                    _selectedProfileService.SelectedProfile = addedProfile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding profile: {ex.Message}");
                throw;
            }
        }

        [RelayCommand]
        private async Task EditProfileAsync(Profile? profile)
        {
            if (profile == null)
                return;

            try
            {
                var dialog = new EditProfileDialog(profile);
                if (Windows_22120278.App.MainWindowInstance is MainWindow mainWindow && mainWindow.Content is FrameworkElement rootElement)
                {
                    dialog.XamlRoot = rootElement.XamlRoot;
                }

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var updatedProfile = dialog.GetUpdatedProfile();
                    if (updatedProfile != null)
                    {
                        await UpdateProfileAsync(profile, updatedProfile);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing profile: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void StartDrawing()
        {
            if (SelectedProfile != null)
            {
                _selectedProfileService.SelectedProfile = SelectedProfile;
                _navigationService.NavigateTo("DrawingPage");
            }
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
        private async Task DeleteProfileAsync(Profile? profile)
        {
            if (profile != null)
            {
                await _profileService.DeleteProfileAsync(profile.Id);
                Profiles.Remove(profile);
                
                if (SelectedProfile?.Id == profile.Id)
                {
                    SelectedProfile = null;
                }
                
                if (_selectedProfileService.SelectedProfile?.Id == profile.Id)
                {
                    if (Profiles.Count > 0)
                    {
                        _selectedProfileService.SelectedProfile = Profiles[0];
                        SelectedProfile = Profiles[0];
                    }
                    else
                    {
                        _selectedProfileService.SelectedProfile = null;
                        SelectedProfile = null;
                    }
                }
            }
        }
    }
}


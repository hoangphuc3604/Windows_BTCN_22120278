using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows_22120278.ViewModels;
using Windows_22120278_Data.models;

namespace Windows_22120278.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ViewModel { get; }

        public ProfilePage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<ProfileViewModel>();
            DataContext = ViewModel;
            Loaded += ProfilePage_Loaded;
        }

        private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadProfilesCommand.ExecuteAsync(null);
        }

        private async void ProfilePage_EditProfile(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProfile != null)
            {
                var editDialog = new EditProfileDialog(ViewModel.SelectedProfile);
                editDialog.XamlRoot = this.XamlRoot;
                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var updatedProfile = editDialog.GetUpdatedProfile();
                    if (updatedProfile != null)
                    {
                        await ViewModel.UpdateProfileAsync(ViewModel.SelectedProfile, updatedProfile);
                    }
                }
            }
        }
    }
}


using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Profile profile)
            {
                await ViewModel.DeleteProfileCommand.ExecuteAsync(profile);
            }
        }
    }
}


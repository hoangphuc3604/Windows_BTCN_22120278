using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.Services;
using Windows_22120278.ViewModels;

namespace Windows_22120278.Views
{
    public sealed partial class SavedCanvasesPage : Page
    {
        public BoardsViewModel ViewModel { get; }
        public List<string> BreadcrumbItems { get; } = new() { "Home", "Management", "Saved Canvases" };

        public SavedCanvasesPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<BoardsViewModel>();
            var selectedProfileService = App.Services.GetRequiredService<ISelectedProfileService>();
            ViewModel.Profile = selectedProfileService.SelectedProfile;
            DataContext = ViewModel;
            Loaded += SavedCanvasesPage_Loaded;
        }

        private async void SavedCanvasesPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.LoadBoardsCommand.ExecuteAsync(null);
        }

        private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is string item)
            {
                var navigationService = App.Services.GetRequiredService<INavigationService>();
                if (item == "Home")
                {
                    navigationService.NavigateTo("Home");
                }
                else if (item == "Management")
                {
                    navigationService.NavigateTo("Management");
                }
            }
        }
    }
}


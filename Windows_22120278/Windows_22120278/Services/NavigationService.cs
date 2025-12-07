using System;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.Views;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? _frame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(string pageName, object? parameter = null)
        {
            if (_frame == null)
                return;

            Type? pageType = pageName switch
            {
                "ProfilePage" or "Profiles" or "Home" => typeof(ProfilePage),
                "DashboardPage" or "Dashboard" or "Management" => typeof(DashboardPage),
                "BoardsPage" or "Boards" => typeof(BoardsPage),
                "DrawingPage" or "Drawing" => typeof(DrawingPage),
                "SettingsPage" or "Settings" => typeof(SettingsPage),
                "SavedCanvases" => typeof(SavedCanvasesPage),
                "Templates" => typeof(TemplatesPage),
                _ => null
            };

            if (pageType != null)
            {
                _frame.Navigate(pageType, parameter);
            }
        }

        public void NavigateToDrawing(DrawingBoard board)
        {
            // Navigate to drawing page with the provided board as parameter
            NavigateTo("DrawingPage", board);
        }

        public void NavigateToBoards(Profile profile)
        {
            // Navigate to boards page with the provided profile as parameter
            NavigateTo("BoardsPage", profile);
        }

        public void GoBack()
        {
            if (_frame != null && _frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}


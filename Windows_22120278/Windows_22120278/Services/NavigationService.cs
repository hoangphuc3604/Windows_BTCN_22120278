using System;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.Views;

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
                "DashboardPage" or "Dashboard" => typeof(DashboardPage),
                "DrawingPage" or "Drawing" => typeof(DrawingPage),
                "SettingsPage" or "Settings" => typeof(SettingsPage),
                _ => null
            };

            if (pageType != null)
            {
                _frame.Navigate(pageType, parameter);
            }
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


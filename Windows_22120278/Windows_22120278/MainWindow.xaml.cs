using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinRT.Interop;
using Windows_22120278.Services;
using Windows_22120278.Views;
using Windows_22120278_Data.models;

namespace Windows_22120278
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow? _appWindow;
        private readonly ISelectedProfileService _selectedProfileService;

        public MainWindow()
        {
            InitializeComponent();
            
            _selectedProfileService = App.Services.GetRequiredService<ISelectedProfileService>();
            
            _appWindow = GetAppWindowForCurrentWindow();
            if (_appWindow != null)
            {
                _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                _appWindow.TitleBar.ButtonForegroundColor = Colors.Black;
                _appWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
                _appWindow.TitleBar.BackgroundColor = Colors.Transparent;
                _appWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
                
                NavView.PaneTitle = "Windows_22120278";
            }

            // Initialize NavigationService with Frame
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.SetFrame(ContentFrame);

            NavView.SelectionChanged += NavView_SelectionChanged;
            ContentFrame.Navigated += ContentFrame_Navigated;
            _selectedProfileService.SelectedProfileChanged += SelectedProfileService_SelectedProfileChanged;
            
            ContentFrame.Navigate(typeof(ProfilePage));
            
            // Initially disable Dashboard since no profile is selected
            UpdateNavigationItemsEnabledState();
        }

        private void SelectedProfileService_SelectedProfileChanged(object? sender, Profile? profile)
        {
            UpdateNavigationItemsEnabledState();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(ProfilePage))
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Profiles");
            }
            else if (e.SourcePageType == typeof(DashboardPage))
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Dashboard");
            }
            else if (e.SourcePageType == typeof(DrawingPage))
            {
                // When navigating to DrawingPage, a profile should be passed
                if (e.Parameter is Profile profile)
                {
                    _selectedProfileService.SelectedProfile = profile;
                }
            }
        }

        private void UpdateNavigationItemsEnabledState()
        {
            // Enable/Disable Dashboard based on whether a profile is selected
            var dashboardItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Dashboard");
            if (dashboardItem != null)
            {
                dashboardItem.IsEnabled = _selectedProfileService.SelectedProfile != null;
            }
        }

        private AppWindow? GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Profiles":
                        ContentFrame.Navigate(typeof(ProfilePage));
                        break;
                    case "Dashboard":
                        if (_selectedProfileService.SelectedProfile != null)
                        {
                            ContentFrame.Navigate(typeof(DashboardPage));
                        }
                        break;
                }
            }
        }
    }
}

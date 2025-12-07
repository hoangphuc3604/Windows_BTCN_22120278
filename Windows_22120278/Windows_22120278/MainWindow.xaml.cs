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
using Windows.Storage;
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
        private const string ThemeSettingKey = "AppTheme";

        public MainWindow()
        {
            InitializeComponent();
            
            _selectedProfileService = App.Services.GetRequiredService<ISelectedProfileService>();
            
            _appWindow = GetAppWindowForCurrentWindow();
            if (_appWindow != null)
            {
                _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                UpdateTitleBarColors();
                
                NavView.PaneTitle = "Drawing App";
            }

            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.SetFrame(ContentFrame);

            NavView.SelectionChanged += NavView_SelectionChanged;
            NavView.BackRequested += NavView_BackRequested;
            ContentFrame.Navigated += ContentFrame_Navigated;
            _selectedProfileService.SelectedProfileChanged += SelectedProfileService_SelectedProfileChanged;
            RootGrid.ActualThemeChanged += RootGrid_ActualThemeChanged;
            
            LoadTheme();
            
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(CustomTitleBar);
            
            ContentFrame.Navigate(typeof(ProfilePage));
            
            UpdateNavigationItemsEnabledState();
            UpdateBackButtonState();
            
            this.SizeChanged += MainWindow_SizeChanged;
            
            this.Activated += MainWindow_Activated;
        }

        private void SelectedProfileService_SelectedProfileChanged(object? sender, Profile? profile)
        {
            UpdateNavigationItemsEnabledState();
        }

        private void UpdateNavigationItemsEnabledState()
        {
            var drawingItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Drawing");
            var managementItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Management");
            
            if (drawingItem != null)
            {
                drawingItem.IsEnabled = _selectedProfileService.SelectedProfile != null;
            }
            
            if (managementItem != null)
            {
                managementItem.IsEnabled = _selectedProfileService.SelectedProfile != null;
            }
        }

        private void UpdateBackButtonState()
        {
            if (BackButton != null)
            {
                BackButton.IsEnabled = ContentFrame.CanGoBack;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.GoBack();
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
                    case "Home":
                        ContentFrame.Navigate(typeof(ProfilePage));
                        break;
                    case "Drawing":
                        if (_selectedProfileService.SelectedProfile != null)
                        {
                            var navigationService = App.Services.GetRequiredService<INavigationService>();
                            navigationService.NavigateTo("DrawingPage");
                        }
                        break;
                    case "Management":
                        if (_selectedProfileService.SelectedProfile != null)
                        {
                            ContentFrame.Navigate(typeof(DashboardPage));
                        }
                        break;
                    case "Settings":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                }
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdateBackButtonState();

            if (e.SourcePageType == typeof(ProfilePage))
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Home");
            }
            else if (e.SourcePageType == typeof(DashboardPage))
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Management");
            }
            else if (e.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else if (e.SourcePageType == typeof(DrawingPage))
            {
                if (e.Parameter is Profile profile)
                {
                    _selectedProfileService.SelectedProfile = profile;
                }
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => item.Tag?.ToString() == "Drawing");
            }
            else
            {
                NavView.SelectedItem = null;
            }
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            double windowWidth = args.Size.Width;
            
            UpdateNavigationViewPaneDisplayMode(windowWidth);
            
            UpdateTitleBarLayout(windowWidth);
        }

        private void UpdateNavigationViewPaneDisplayMode(double windowWidth)
        {
            if (windowWidth < 640)
            {
                if (NavView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top)
                {
                    NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                }
            }
            else if (windowWidth < 1008)
            {
                if (NavView.PaneDisplayMode != NavigationViewPaneDisplayMode.LeftCompact)
                {
                    NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                }
            }
            else
            {
                if (NavView.PaneDisplayMode != NavigationViewPaneDisplayMode.Left)
                {
                    NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                }
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_appWindow != null)
            {
                var windowSize = _appWindow.Size;
                UpdateNavigationViewPaneDisplayMode(windowSize.Width);
                UpdateTitleBarLayout(windowSize.Width);
            }
        }

        private void UpdateTitleBarLayout(double windowWidth)
        {
            if (windowWidth < 600)
            {
                if (AppTitle != null)
                {
                    AppTitle.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (AppTitle != null)
                {
                    AppTitle.Visibility = Visibility.Visible;
                }
            }
        }

        private void RootGrid_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTitleBarColors();
        }

        private void UpdateTitleBarColors()
        {
            if (_appWindow == null) return;

            var isDark = RootGrid.ActualTheme == ElementTheme.Dark;
            
            _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.BackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            
            if (isDark)
            {
                _appWindow.TitleBar.ButtonForegroundColor = Colors.White;
                _appWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            }
            else
            {
                _appWindow.TitleBar.ButtonForegroundColor = Colors.Black;
                _appWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            }
        }

        private void LoadTheme()
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values.TryGetValue(ThemeSettingKey, out var themeValue))
                {
                    var theme = themeValue?.ToString();
                    switch (theme)
                    {
                        case "Light":
                            RootGrid.RequestedTheme = ElementTheme.Light;
                            break;
                        case "Dark":
                            RootGrid.RequestedTheme = ElementTheme.Dark;
                            break;
                        case "System":
                        default:
                            RootGrid.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme: {ex.Message}");
            }
        }

        public ElementTheme GetCurrentTheme()
        {
            return RootGrid.ActualTheme;
        }

        public void SetTheme(ElementTheme theme)
        {
            RootGrid.RequestedTheme = theme;
            UpdateTitleBarColors();
            
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                var themeString = theme switch
                {
                    ElementTheme.Light => "Light",
                    ElementTheme.Dark => "Dark",
                    _ => "System"
                };
                localSettings.Values[ThemeSettingKey] = themeString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme: {ex.Message}");
            }
        }
    }
}

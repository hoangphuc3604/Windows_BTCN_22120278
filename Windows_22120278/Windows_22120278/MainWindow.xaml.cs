using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows_22120278.Views;

namespace Windows_22120278
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow? _appWindow;

        public MainWindow()
        {
            InitializeComponent();
            
            _appWindow = GetAppWindowForCurrentWindow();
            if (_appWindow != null)
            {
                _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                
                NavView.PaneTitle = "Windows_22120278";
            }

            NavView.SelectionChanged += NavView_SelectionChanged;
            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigate(typeof(ProfilePage));
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
                        ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                }
            }
        }
    }
}

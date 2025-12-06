using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278;

namespace Windows_22120278.Views
{
    public sealed partial class SettingsPage : Page
    {
        private MainWindow? _mainWindow;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow = App.MainWindowInstance;
            LoadCurrentTheme();
        }

        private void LoadCurrentTheme()
        {
            if (_mainWindow == null) return;

            var currentTheme = _mainWindow.GetCurrentTheme();
            
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    LightThemeRadio.IsChecked = true;
                    break;
                case ElementTheme.Dark:
                    DarkThemeRadio.IsChecked = true;
                    break;
                case ElementTheme.Default:
                default:
                    SystemThemeRadio.IsChecked = true;
                    break;
            }
        }

        private void LightThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null && LightThemeRadio.IsChecked == true)
            {
                _mainWindow.SetTheme(ElementTheme.Light);
            }
        }

        private void DarkThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null && DarkThemeRadio.IsChecked == true)
            {
                _mainWindow.SetTheme(ElementTheme.Dark);
            }
        }

        private void SystemThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null && SystemThemeRadio.IsChecked == true)
            {
                _mainWindow.SetTheme(ElementTheme.Default);
            }
        }
    }
}


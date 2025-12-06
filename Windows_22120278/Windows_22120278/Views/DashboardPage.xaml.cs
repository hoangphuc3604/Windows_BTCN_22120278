using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.ViewModels;

namespace Windows_22120278.Views
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; }
        public List<string> BreadcrumbItems { get; } = new() { "Home", "Dashboard" };

        public DashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
            DataContext = ViewModel;
            Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadStatsCommand.ExecuteAsync(null);
        }
    }
}

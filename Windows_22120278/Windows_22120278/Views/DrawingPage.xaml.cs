using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.ViewModels;

namespace Windows_22120278.Views
{
    public sealed partial class DrawingPage : Page
    {
        public DrawingViewModel ViewModel { get; }

        public DrawingPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<DrawingViewModel>();
            DataContext = ViewModel;
        }
    }
}

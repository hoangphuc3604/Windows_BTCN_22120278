using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Windows_22120278.Models
{
    public abstract partial class DrawingShape : ObservableObject
    {
        [ObservableProperty]
        private double x;

        [ObservableProperty]
        private double y;

        [ObservableProperty]
        private double width;

        [ObservableProperty]
        private double height;

        [ObservableProperty]
        private SolidColorBrush color = new SolidColorBrush(Microsoft.UI.Colors.Black);

        [ObservableProperty]
        private double strokeThickness = 1.0;
    }
}



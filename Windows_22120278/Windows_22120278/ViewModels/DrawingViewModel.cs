using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Windows.UI;
using Windows_22120278.Models;
using Windows_22120278.Services;
using Windows_22120278_Data.models;

namespace Windows_22120278.ViewModels
{
    public partial class DrawingViewModel : ObservableObject
    {
        private readonly IDrawingService _drawingService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Color currentColor = Microsoft.UI.Colors.Black;

        [ObservableProperty]
        private double currentStrokeSize = 1.0;

        [ObservableProperty]
        private ShapeType currentShapeType = ShapeType.Line;

        [ObservableProperty]
        private ObservableCollection<DrawingShape> shapes = new();

        public DrawingViewModel(IDrawingService drawingService, INavigationService navigationService)
        {
            _drawingService = drawingService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void SelectTool(ShapeType shapeType)
        {
            CurrentShapeType = shapeType;
        }

        [RelayCommand]
        private async Task SaveDrawing()
        {
            // Implementation will be added later
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void ClearCanvas()
        {
            Shapes.Clear();
        }
    }
}



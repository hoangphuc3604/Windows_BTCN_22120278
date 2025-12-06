using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private Profile? _currentProfile;
        private DrawingBoard? _currentDrawingBoard;

        public DrawingViewModel(IDrawingService drawingService, INavigationService navigationService)
        {
            _drawingService = drawingService;
            _navigationService = navigationService;
        }

        public void SetProfile(Profile profile)
        {
            _currentProfile = profile;
        }

        [RelayCommand]
        private async Task LoadDrawingAsync()
        {
            if (_currentProfile == null)
                return;

            var latestBoard = await _drawingService.GetLatestDrawingBoardAsync(_currentProfile.Id);
            if (latestBoard != null)
            {
                _currentDrawingBoard = latestBoard;
                var (board, shapes) = await _drawingService.GetDrawingAsync(latestBoard.Id);
                
                Shapes.Clear();
                foreach (var shape in shapes)
                {
                    Shapes.Add(shape);
                }
            }
        }

        [RelayCommand]
        private void SelectTool(ShapeType shapeType)
        {
            CurrentShapeType = shapeType;
        }

        [RelayCommand]
        private async Task SaveDrawing()
        {
            if (_currentProfile == null)
                return;

            var shapesList = Shapes.ToList();

            if (_currentDrawingBoard == null)
            {
                _currentDrawingBoard = new DrawingBoard
                {
                    Name = $"Drawing {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    Width = 800,
                    Height = 600,
                    BackgroundColor = "#FFFFFF",
                    ProfileId = _currentProfile.Id
                };
            }

            _currentDrawingBoard = await _drawingService.SaveDrawingAsync(_currentDrawingBoard, shapesList);
        }

        [RelayCommand]
        private void ClearCanvas()
        {
            Shapes.Clear();
        }
    }
}



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
        private readonly ITemplateService _templateService;

        [ObservableProperty]
        private Color currentColor = Microsoft.UI.Colors.Black;

        [ObservableProperty]
        private double currentStrokeSize = 1.0;

        [ObservableProperty]
        private ShapeType currentShapeType = ShapeType.Line;

        [ObservableProperty]
        private ObservableCollection<DrawingShape> shapes = new();

        [ObservableProperty]
        private DrawingShape? selectedShape;

        [ObservableProperty]
        private ObservableCollection<ShapeTemplate> templates = new();

        private Profile? _currentProfile;
        private DrawingBoard? _currentDrawingBoard;

        public DrawingViewModel(IDrawingService drawingService, INavigationService navigationService, ITemplateService templateService)
        {
            _drawingService = drawingService;
            _navigationService = navigationService;
            _templateService = templateService;
        }

        public void SetDrawingBoard(DrawingBoard board)
        {
            _currentDrawingBoard = board;
            _currentProfile = board.Profile;
        }

        [RelayCommand]
        private async Task LoadDrawingAsync()
        {
            if (_currentDrawingBoard == null)
                return;

            var (board, shapes) = await _drawingService.GetDrawingAsync(_currentDrawingBoard.Id);
            
            Shapes.Clear();
            foreach (var shape in shapes)
            {
                Shapes.Add(shape);
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
            if (_currentDrawingBoard == null)
                return;

            var shapesList = Shapes.ToList();
            _currentDrawingBoard = await _drawingService.SaveDrawingAsync(_currentDrawingBoard, shapesList);
        }

        [RelayCommand]
        private void ClearCanvas()
        {
            Shapes.Clear();
            SelectedShape = null;
        }

        [RelayCommand]
        private void DeleteSelectedShape()
        {
            if (SelectedShape != null && Shapes.Contains(SelectedShape))
            {
                Shapes.Remove(SelectedShape);
                SelectedShape = null;
            }
        }

        [RelayCommand]
        private async Task LoadTemplatesAsync()
        {
            if (_currentProfile == null)
                return;

            var templatesList = await _templateService.GetAllTemplatesAsync(_currentProfile.Id);
            Templates.Clear();
            foreach (var template in templatesList)
            {
                Templates.Add(template);
            }
        }

        [RelayCommand]
        private async Task SaveAsTemplateAsync(string? name = null)
        {
            if (SelectedShape == null)
                return;

            if (_currentProfile == null)
                return;

            if (string.IsNullOrWhiteSpace(name))
            {
                // TODO: Show dialog to get name from user
                name = $"Template {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }

            var template = await _templateService.SaveTemplateAsync(name, SelectedShape, _currentProfile.Id);
            Templates.Add(template);
        }

        [RelayCommand]
        private void LoadTemplate(ShapeTemplate template)
        {
            if (template == null)
                return;

            var drawingShape = _templateService.ConvertTemplateToDrawingShape(template);
            if (drawingShape != null)
            {
                var newX = 100.0;
                var newY = 100.0;
                var oldX = drawingShape.X;
                var oldY = drawingShape.Y;
                var deltaX = newX - oldX;
                var deltaY = newY - oldY;

                if (drawingShape is PolygonShape polygonShape)
                {
                    for (int i = 0; i < polygonShape.Points.Count; i++)
                    {
                        var pt = polygonShape.Points[i];
                        polygonShape.Points[i] = new Windows.Foundation.Point(pt.X + deltaX, pt.Y + deltaY);
                    }
                    
                    var minX = polygonShape.Points.Min(p => p.X);
                    var minY = polygonShape.Points.Min(p => p.Y);
                    var maxX = polygonShape.Points.Max(p => p.X);
                    var maxY = polygonShape.Points.Max(p => p.Y);
                    
                    polygonShape.X = minX;
                    polygonShape.Y = minY;
                    polygonShape.Width = maxX - minX;
                    polygonShape.Height = maxY - minY;
                }
                else if (drawingShape is LineShape lineShape)
                {
                    if (Math.Abs(lineShape.Width) < 1)
                        lineShape.Width = 50;
                    if (Math.Abs(lineShape.Height) < 1)
                        lineShape.Height = 50;
                    
                    lineShape.X = newX;
                    lineShape.Y = newY;
                }
                else
                {
                    drawingShape.X = newX;
                    drawingShape.Y = newY;
                    
                    if (drawingShape.Width <= 0)
                        drawingShape.Width = 50;
                    if (drawingShape.Height <= 0)
                        drawingShape.Height = 50;
                }

                Shapes.Add(drawingShape);
            }
        }
    }
}



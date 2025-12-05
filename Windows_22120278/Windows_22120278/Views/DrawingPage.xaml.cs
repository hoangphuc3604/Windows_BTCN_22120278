using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows_22120278.Models;
using Windows_22120278.ViewModels;
using Windows_22120278_Data.models;

namespace Windows_22120278.Views
{
    public sealed partial class DrawingPage : Page
    {
        public DrawingViewModel ViewModel { get; }

        private Shape? _previewShape;
        private Point _startPoint;
        private bool _isDrawing = false;

        private Windows_22120278_Data.models.Profile? _currentProfile;

        public DrawingPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<DrawingViewModel>();
            DataContext = ViewModel;

            ViewModel.Shapes.CollectionChanged += Shapes_CollectionChanged;
            this.Unloaded += DrawingPage_Unloaded;
            RenderAllShapes();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Windows_22120278_Data.models.Profile profile)
            {
                _currentProfile = profile;
            }
        }

        private void DrawingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Shapes.CollectionChanged -= Shapes_CollectionChanged;
        }

        private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isDrawing) return;

            var point = e.GetCurrentPoint(DrawingCanvas);
            _startPoint = point.Position;
            _isDrawing = true;

            DrawingCanvas.CapturePointer(e.Pointer);

            _previewShape = CreatePreviewShape(ViewModel.CurrentShapeType);
            if (_previewShape != null)
            {
                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor);
                _previewShape.Stroke = brush;
                _previewShape.StrokeThickness = ViewModel.CurrentStrokeSize;

                if (ViewModel.CurrentShapeType == ShapeType.Line)
                {
                    if (_previewShape is Line line)
                    {
                        line.X1 = _startPoint.X;
                        line.Y1 = _startPoint.Y;
                        line.X2 = _startPoint.X;
                        line.Y2 = _startPoint.Y;
                    }
                }
                else
                {
                    Canvas.SetLeft(_previewShape, _startPoint.X);
                    Canvas.SetTop(_previewShape, _startPoint.Y);
                    _previewShape.Width = 0;
                    _previewShape.Height = 0;
                }

                DrawingCanvas.Children.Add(_previewShape);
            }

            e.Handled = true;
        }

        private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDrawing || _previewShape == null) return;

            var point = e.GetCurrentPoint(DrawingCanvas);
            var currentPoint = point.Position;

            if (ViewModel.CurrentShapeType == ShapeType.Line)
            {
                if (_previewShape is Line line)
                {
                    line.X2 = currentPoint.X;
                    line.Y2 = currentPoint.Y;
                }
            }
            else
            {
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);
                var left = Math.Min(_startPoint.X, currentPoint.X);
                var top = Math.Min(_startPoint.Y, currentPoint.Y);

                Canvas.SetLeft(_previewShape, left);
                Canvas.SetTop(_previewShape, top);
                _previewShape.Width = width;
                _previewShape.Height = height;
            }

            e.Handled = true;
        }

        private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDrawing) return;

            var point = e.GetCurrentPoint(DrawingCanvas);
            var endPoint = point.Position;

            if (_previewShape != null)
            {
                DrawingCanvas.Children.Remove(_previewShape);
            }

            var drawingShape = CreateDrawingShape(_startPoint, endPoint);
            if (drawingShape != null)
            {
                ViewModel.Shapes.Add(drawingShape);
            }

            _previewShape = null;
            _isDrawing = false;
            DrawingCanvas.ReleasePointerCapture(e.Pointer);

            e.Handled = true;
        }

        private Shape? CreatePreviewShape(ShapeType shapeType)
        {
            return shapeType switch
            {
                ShapeType.Line => new Line(),
                ShapeType.Rectangle => new Rectangle(),
                ShapeType.Ellipse => new Ellipse(),
                _ => null
            };
        }

        private DrawingShape? CreateDrawingShape(Point startPoint, Point endPoint)
        {
            var left = Math.Min(startPoint.X, endPoint.X);
            var top = Math.Min(startPoint.Y, endPoint.Y);
            var width = Math.Abs(endPoint.X - startPoint.X);
            var height = Math.Abs(endPoint.Y - startPoint.Y);

            DrawingShape? shape = ViewModel.CurrentShapeType switch
            {
                ShapeType.Line => new LineShape
                {
                    X = startPoint.X,
                    Y = startPoint.Y,
                    Width = endPoint.X - startPoint.X,
                    Height = endPoint.Y - startPoint.Y
                },
                ShapeType.Rectangle => new RectangleShape
                {
                    X = left,
                    Y = top,
                    Width = width,
                    Height = height
                },
                ShapeType.Ellipse => new EllipseShape
                {
                    X = left,
                    Y = top,
                    Width = width,
                    Height = height
                },
                _ => null
            };

            if (shape != null)
            {
                shape.Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor);
                shape.StrokeThickness = ViewModel.CurrentStrokeSize;
            }

            return shape;
        }

        private void Shapes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RenderAllShapes();
        }

        private void RenderAllShapes()
        {
            var shapesToRemove = new List<UIElement>();
            foreach (var child in DrawingCanvas.Children)
            {
                if (child != _previewShape && child is Shape)
                {
                    shapesToRemove.Add(child);
                }
            }
            foreach (var shape in shapesToRemove)
            {
                DrawingCanvas.Children.Remove(shape);
            }

            foreach (var drawingShape in ViewModel.Shapes)
            {
                var uiShape = CreateUIShapeFromDrawingShape(drawingShape);
                if (uiShape != null)
                {
                    DrawingCanvas.Children.Add(uiShape);
                }
            }
        }

        private Shape? CreateUIShapeFromDrawingShape(DrawingShape drawingShape)
        {
            Shape? uiShape = drawingShape switch
            {
                LineShape => new Line(),
                RectangleShape => new Rectangle(),
                EllipseShape => new Ellipse(),
                _ => null
            };

            if (uiShape == null) return null;

            uiShape.Stroke = drawingShape.Color;
            uiShape.StrokeThickness = drawingShape.StrokeThickness;

            if (drawingShape is LineShape lineShape)
            {
                if (uiShape is Line line)
                {
                    line.X1 = lineShape.X;
                    line.Y1 = lineShape.Y;
                    line.X2 = lineShape.X + lineShape.Width;
                    line.Y2 = lineShape.Y + lineShape.Height;
                }
            }
            else
            {
                Canvas.SetLeft(uiShape, drawingShape.X);
                Canvas.SetTop(uiShape, drawingShape.Y);
                uiShape.Width = drawingShape.Width;
                uiShape.Height = drawingShape.Height;
            }

            return uiShape;
        }
    }
}

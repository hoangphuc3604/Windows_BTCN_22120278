using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;
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

        private List<Point> _polygonPoints = new();
        private Polygon? _polygonPreview;
        private bool _isDrawingPolygon = false;

        private Windows_22120278_Data.models.Profile? _currentProfile;
        private Dictionary<UIElement, DrawingShape> _shapeMapping = new();

        public DrawingPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<DrawingViewModel>();
            DataContext = ViewModel;

            ViewModel.Shapes.CollectionChanged += Shapes_CollectionChanged;
            this.Unloaded += DrawingPage_Unloaded;
            this.Loaded += DrawingPage_Loaded;
            RenderAllShapes();
        }

        private void DrawingPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColorPreview();
            if (ColorPickerControl != null)
            {
                var winColor = Windows.UI.Color.FromArgb(
                    ViewModel.CurrentColor.A,
                    ViewModel.CurrentColor.R,
                    ViewModel.CurrentColor.G,
                    ViewModel.CurrentColor.B);
                ColorPickerControl.Color = winColor;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Windows_22120278_Data.models.Profile profile)
            {
                _currentProfile = profile;
                ViewModel.SetProfile(profile);
                await ViewModel.LoadDrawingCommand.ExecuteAsync(null);
            }
        }

        private void DrawingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Shapes.CollectionChanged -= Shapes_CollectionChanged;
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            var newColor = Windows.UI.Color.FromArgb(
                args.NewColor.A,
                args.NewColor.R,
                args.NewColor.G,
                args.NewColor.B);
            ViewModel.CurrentColor = newColor;
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            if (ColorPreviewBorder != null)
            {
                ColorPreviewBorder.Background = new SolidColorBrush(ViewModel.CurrentColor);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                this.Frame?.Navigate(typeof(ProfilePage));
            }
        }

        private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var source = e.OriginalSource as UIElement;
            if (source != null && source != DrawingCanvas && _shapeMapping.ContainsKey(source))
            {
                return;
            }

            if (ViewModel.CurrentShapeType == ShapeType.Polygon)
            {
                HandlePolygonClick(e);
                return;
            }

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

        private void HandlePolygonClick(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(DrawingCanvas);
            var clickPoint = point.Position;

            if (!_isDrawingPolygon)
            {
                _polygonPoints.Clear();
                _polygonPoints.Add(clickPoint);
                _isDrawingPolygon = true;

                _polygonPreview = new Polygon
                {
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor),
                    StrokeThickness = ViewModel.CurrentStrokeSize,
                    Fill = null
                };

                var pointCollection = new PointCollection();
                pointCollection.Add(clickPoint);
                _polygonPreview.Points = pointCollection;

                DrawingCanvas.Children.Add(_polygonPreview);
            }
            else
            {
                _polygonPoints.Add(clickPoint);
                UpdatePolygonPreview();
            }

            e.Handled = true;
        }

        private void UpdatePolygonPreview()
        {
            if (_polygonPreview != null && _polygonPoints.Count > 0)
            {
                var pointCollection = new PointCollection();
                foreach (var pt in _polygonPoints)
                {
                    pointCollection.Add(pt);
                }
                _polygonPreview.Points = pointCollection;
            }
        }

        private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentShapeType == ShapeType.Polygon && _isDrawingPolygon)
            {
                var point = e.GetCurrentPoint(DrawingCanvas);
                var currentPoint = point.Position;

                if (_polygonPoints.Count > 0)
                {
                    var tempPoints = new List<Point>(_polygonPoints) { currentPoint };
                    var pointCollection = new PointCollection();
                    foreach (var pt in tempPoints)
                    {
                        pointCollection.Add(pt);
                    }
                    if (_polygonPreview != null)
                    {
                        _polygonPreview.Points = pointCollection;
                    }
                }
                e.Handled = true;
                return;
            }

            if (!_isDrawing || _previewShape == null) return;

            var point2 = e.GetCurrentPoint(DrawingCanvas);
            var currentPoint2 = point2.Position;

            if (ViewModel.CurrentShapeType == ShapeType.Line)
            {
                if (_previewShape is Line line)
                {
                    line.X2 = currentPoint2.X;
                    line.Y2 = currentPoint2.Y;
                }
            }
            else
            {
                var width = Math.Abs(currentPoint2.X - _startPoint.X);
                var height = Math.Abs(currentPoint2.Y - _startPoint.Y);
                var left = Math.Min(_startPoint.X, currentPoint2.X);
                var top = Math.Min(_startPoint.Y, currentPoint2.Y);

                Canvas.SetLeft(_previewShape, left);
                Canvas.SetTop(_previewShape, top);
                _previewShape.Width = width;
                _previewShape.Height = height;
            }

            e.Handled = true;
        }

        private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentShapeType == ShapeType.Polygon)
            {
                return;
            }

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

        private void DrawingCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ViewModel.CurrentShapeType == ShapeType.Polygon && _isDrawingPolygon && _polygonPoints.Count >= 3)
            {
                if (_polygonPreview != null)
                {
                    DrawingCanvas.Children.Remove(_polygonPreview);
                }

                var polygonShape = new PolygonShape
                {
                    Points = new List<Point>(_polygonPoints),
                    Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor),
                    StrokeThickness = ViewModel.CurrentStrokeSize
                };

                if (_polygonPoints.Count > 0)
                {
                    var minX = _polygonPoints.Min(p => p.X);
                    var minY = _polygonPoints.Min(p => p.Y);
                    var maxX = _polygonPoints.Max(p => p.X);
                    var maxY = _polygonPoints.Max(p => p.Y);

                    polygonShape.X = minX;
                    polygonShape.Y = minY;
                    polygonShape.Width = maxX - minX;
                    polygonShape.Height = maxY - minY;
                }

                ViewModel.Shapes.Add(polygonShape);

                _polygonPreview = null;
                _polygonPoints.Clear();
                _isDrawingPolygon = false;

                e.Handled = true;
            }
        }

        private Shape? CreatePreviewShape(ShapeType shapeType)
        {
            return shapeType switch
            {
                ShapeType.Line => new Line(),
                ShapeType.Rectangle => new Rectangle(),
                ShapeType.Ellipse => new Ellipse(),
                ShapeType.Polygon => new Polygon(),
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
                if (child != _previewShape && child != _polygonPreview && child is Shape)
                {
                    shapesToRemove.Add(child);
                }
            }
            foreach (var shape in shapesToRemove)
            {
                if (_shapeMapping.ContainsKey(shape))
                {
                    _shapeMapping.Remove(shape);
                }
                DrawingCanvas.Children.Remove(shape);
            }

            foreach (var drawingShape in ViewModel.Shapes)
            {
                var uiShape = CreateUIShapeFromDrawingShape(drawingShape);
                if (uiShape != null)
                {
                    uiShape.IsHitTestVisible = true;
                    uiShape.PointerPressed += Shape_PointerPressed;
                    _shapeMapping[uiShape] = drawingShape;
                    DrawingCanvas.Children.Add(uiShape);
                }
            }
        }

        private Shape? CreateUIShapeFromDrawingShape(DrawingShape drawingShape)
        {
            if (drawingShape is PolygonShape polygonShape)
            {
                var polygon = new Polygon
                {
                    Stroke = polygonShape.Color,
                    StrokeThickness = polygonShape.StrokeThickness,
                    Fill = null
                };

                var pointCollection = new PointCollection();
                foreach (var pt in polygonShape.Points)
                {
                    pointCollection.Add(pt);
                }
                polygon.Points = pointCollection;

                return polygon;
            }

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

        private void Shape_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement uiElement && _shapeMapping.TryGetValue(uiElement, out var drawingShape))
            {
                ViewModel.SelectedShape = drawingShape;
                e.Handled = true;
            }
        }
    }
}

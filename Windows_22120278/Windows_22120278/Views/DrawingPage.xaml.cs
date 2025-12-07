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

        private List<Point> _trianglePoints = new();
        private Polygon? _trianglePreview;
        private bool _isDrawingTriangle = false;

        private Windows_22120278_Data.models.Profile? _currentProfile;
        private Dictionary<UIElement, DrawingShape> _shapeMapping = new();
        private DrawingShape? _previousSelectedShape;
        private UIElement? _draggingShape;
        private Point _dragStartPoint;
        private Point _shapeStartPosition;
        private bool _isDragging = false;
        private UIElement? _selectedUIElement;

        public DrawingPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<DrawingViewModel>();
            DataContext = ViewModel;

            ViewModel.Shapes.CollectionChanged += Shapes_CollectionChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.Unloaded += DrawingPage_Unloaded;
            this.Loaded += DrawingPage_Loaded;
            RenderAllShapes();
        }

        private void DrawingPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColorPreview();
            UpdatePropertiesPanelVisibility();
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
            if (e.Parameter is Windows_22120278_Data.models.DrawingBoard board)
            {
                _currentProfile = board.Profile;
                ViewModel.SetDrawingBoard(board);
                await ViewModel.LoadDrawingCommand.ExecuteAsync(null);
                await ViewModel.LoadTemplatesCommand.ExecuteAsync(null);
            }
            else
            {
                var selectedProfileService = App.Services.GetRequiredService<Windows_22120278.Services.ISelectedProfileService>();
                var profile = selectedProfileService.SelectedProfile;
                
                if (profile != null)
                {
                    _currentProfile = profile;
                    
                    var drawingService = App.Services.GetRequiredService<Windows_22120278.Services.IDrawingService>();
                    var newBoard = new Windows_22120278_Data.models.DrawingBoard
                    {
                        Name = $"Drawing {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                        Width = profile.DefaultBoardWidth,
                        Height = profile.DefaultBoardHeight,
                        BackgroundColor = "#FFFFFF",
                        ProfileId = profile.Id,
                        CreatedDate = DateTime.Now
                    };
                    
                    var createdBoard = await drawingService.CreateDrawingBoardAsync(newBoard);
                    _currentProfile = createdBoard.Profile;
                    ViewModel.SetDrawingBoard(createdBoard);
                    await ViewModel.LoadTemplatesCommand.ExecuteAsync(null);
                }
            }
        }

        private void DrawingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Shapes.CollectionChanged -= Shapes_CollectionChanged;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (_previousSelectedShape != null)
            {
                _previousSelectedShape.PropertyChanged -= SelectedShape_PropertyChanged;
                _previousSelectedShape = null;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedShape))
            {
                if (_previousSelectedShape != null)
                {
                    _previousSelectedShape.PropertyChanged -= SelectedShape_PropertyChanged;
                }

                // Find and store the UI element for the selected shape
                _selectedUIElement = null;
                if (ViewModel.SelectedShape != null)
                {
                    foreach (var kvp in _shapeMapping)
                    {
                        if (kvp.Value == ViewModel.SelectedShape)
                        {
                            _selectedUIElement = kvp.Key;
                            break;
                        }
                    }
                    ViewModel.SelectedShape.PropertyChanged += SelectedShape_PropertyChanged;
                }

                _previousSelectedShape = ViewModel.SelectedShape;
                UpdatePropertiesPanelVisibility();
                UpdateShapeColorPreview();
            }
        }

        private void SelectedShape_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Don't update UI while dragging (position changes)
            if (_isDragging && (e.PropertyName == nameof(DrawingShape.X) || e.PropertyName == nameof(DrawingShape.Y)))
            {
                return;
            }

            if (sender is DrawingShape drawingShape && drawingShape == ViewModel.SelectedShape)
            {
                // Use the stored UI element reference, or find it if not stored
                UIElement? uiElement = _selectedUIElement;
                
                // Validate the stored reference
                if (uiElement == null || !_shapeMapping.ContainsKey(uiElement) || _shapeMapping[uiElement] != drawingShape)
                {
                    // Try to find it in the mapping
                    foreach (var kvp in _shapeMapping)
                    {
                        if (kvp.Value == drawingShape)
                        {
                            uiElement = kvp.Key;
                            _selectedUIElement = uiElement; // Update stored reference
                            break;
                        }
                    }
                }

                if (uiElement != null)
                {
                    // Update only the specific property that changed
                    if (e.PropertyName == nameof(DrawingShape.StrokeThickness) && uiElement is Shape shape)
                    {
                        shape.StrokeThickness = drawingShape.StrokeThickness;
                        return;
                    }
                    else if (e.PropertyName == nameof(DrawingShape.Color) && uiElement is Shape shapeColor)
                    {
                        shapeColor.Stroke = drawingShape.Color;
                        return;
                    }
                    else if (e.PropertyName == nameof(DrawingShape.Width) || e.PropertyName == nameof(DrawingShape.Height))
                    {
                        if (drawingShape is LineShape lineShape && uiElement is Line line)
                        {
                            line.X1 = lineShape.X;
                            line.Y1 = lineShape.Y;
                            line.X2 = lineShape.X + lineShape.Width;
                            line.Y2 = lineShape.Y + lineShape.Height;
                        }
                        else if (drawingShape is PolygonShape polygonShape && uiElement is Polygon polygon)
                        {
                            var newPoints = new PointCollection();
                            foreach (var pt in polygonShape.Points)
                            {
                                newPoints.Add(pt);
                            }
                            polygon.Points = newPoints;
                        }
                        else if (drawingShape is TriangleShape triangleShape && uiElement is Polygon triangle)
                        {
                            var newPoints = new PointCollection();
                            foreach (var pt in triangleShape.Points)
                            {
                                newPoints.Add(pt);
                            }
                            triangle.Points = newPoints;
                        }
                        else if (drawingShape is CircleShape circleShape && uiElement is Shape circle)
                        {
                            var size = Math.Max(circleShape.Width, circleShape.Height);
                            circle.Width = size;
                            circle.Height = size;
                        }
                        else if (uiElement is Shape shapeSize)
                        {
                            shapeSize.Width = drawingShape.Width;
                            shapeSize.Height = drawingShape.Height;
                        }
                        return;
                    }
                    else if (e.PropertyName == nameof(DrawingShape.X) || e.PropertyName == nameof(DrawingShape.Y))
                    {
                        if (drawingShape is LineShape lineShape && uiElement is Line line)
                        {
                            line.X1 = lineShape.X;
                            line.Y1 = lineShape.Y;
                            line.X2 = lineShape.X + lineShape.Width;
                            line.Y2 = lineShape.Y + lineShape.Height;
                        }
                        else if (drawingShape is PolygonShape polygonShape && uiElement is Polygon polygon)
                        {
                            var newPoints = new PointCollection();
                            foreach (var pt in polygonShape.Points)
                            {
                                newPoints.Add(pt);
                            }
                            polygon.Points = newPoints;
                        }
                        else if (drawingShape is TriangleShape triangleShape && uiElement is Polygon triangle)
                        {
                            var newPoints = new PointCollection();
                            foreach (var pt in triangleShape.Points)
                            {
                                newPoints.Add(pt);
                            }
                            triangle.Points = newPoints;
                        }
                        else if (drawingShape is CircleShape circleShape && uiElement is Shape circle)
                        {
                            var size = Math.Max(circleShape.Width, circleShape.Height);
                            Canvas.SetLeft(circle, circleShape.X);
                            Canvas.SetTop(circle, circleShape.Y);
                            circle.Width = size;
                            circle.Height = size;
                        }
                        else
                        {
                            Canvas.SetLeft(uiElement, drawingShape.X);
                            Canvas.SetTop(uiElement, drawingShape.Y);
                        }
                        return;
                    }
                }
            }

            // Fallback: re-render all shapes if we can't find the specific shape
            if (e.PropertyName == nameof(DrawingShape.Width) || 
                e.PropertyName == nameof(DrawingShape.Height) || 
                e.PropertyName == nameof(DrawingShape.StrokeThickness) ||
                e.PropertyName == nameof(DrawingShape.Color) ||
                e.PropertyName == nameof(DrawingShape.X) ||
                e.PropertyName == nameof(DrawingShape.Y))
            {
                RenderAllShapes();
                // After re-render, update the stored reference
                if (ViewModel.SelectedShape != null)
                {
                    foreach (var kvp in _shapeMapping)
                    {
                        if (kvp.Value == ViewModel.SelectedShape)
                        {
                            _selectedUIElement = kvp.Key;
                            break;
                        }
                    }
                }
            }
        }

        private void UpdatePropertiesPanelVisibility()
        {
            if (PropertiesPanel != null)
            {
                PropertiesPanel.Visibility = ViewModel.SelectedShape != null 
                    ? Microsoft.UI.Xaml.Visibility.Visible 
                    : Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void UpdateShapeColorPreview()
        {
            if (ShapeColorPreviewBorder != null && ViewModel.SelectedShape != null)
            {
                var color = ViewModel.SelectedShape.Color;
                if (color != null)
                {
                    ShapeColorPreviewBorder.Background = color;
                    
                    if (ShapeColorPickerControl != null)
                    {
                        var winColor = Windows.UI.Color.FromArgb(
                            color.Color.A,
                            color.Color.R,
                            color.Color.G,
                            color.Color.B);
                        ShapeColorPickerControl.Color = winColor;
                    }
                }
            }
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
            if (_draggingShape != null || _isDragging) return;

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

            if (ViewModel.CurrentShapeType == ShapeType.Triangle)
            {
                HandleTriangleClick(e);
                return;
            }

            if (_isDrawing) return;

            if (source == DrawingCanvas && ViewModel.SelectedShape != null)
            {
                ViewModel.SelectedShape = null;
            }

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
                else if (ViewModel.CurrentShapeType == ShapeType.Circle)
                {
                    Canvas.SetLeft(_previewShape, _startPoint.X);
                    Canvas.SetTop(_previewShape, _startPoint.Y);
                    _previewShape.Width = 0;
                    _previewShape.Height = 0;
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

            if (ViewModel.CurrentShapeType == ShapeType.Triangle && _isDrawingTriangle)
            {
                var point = e.GetCurrentPoint(DrawingCanvas);
                var currentPoint = point.Position;

                if (_trianglePoints.Count > 0 && _trianglePoints.Count < 3)
                {
                    var tempPoints = new List<Point>(_trianglePoints) { currentPoint };
                    var pointCollection = new PointCollection();
                    foreach (var pt in tempPoints)
                    {
                        pointCollection.Add(pt);
                    }
                    if (_trianglePreview != null)
                    {
                        _trianglePreview.Points = pointCollection;
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
            else if (ViewModel.CurrentShapeType == ShapeType.Circle)
            {
                var width = Math.Abs(currentPoint2.X - _startPoint.X);
                var height = Math.Abs(currentPoint2.Y - _startPoint.Y);
                var size = Math.Max(width, height);
                var left = _startPoint.X - size / 2;
                var top = _startPoint.Y - size / 2;

                Canvas.SetLeft(_previewShape, left);
                Canvas.SetTop(_previewShape, top);
                _previewShape.Width = size;
                _previewShape.Height = size;
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
            if (ViewModel.CurrentShapeType == ShapeType.Polygon || ViewModel.CurrentShapeType == ShapeType.Triangle)
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

        private void HandleTriangleClick(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(DrawingCanvas);
            var clickPoint = point.Position;

            if (!_isDrawingTriangle)
            {
                _trianglePoints.Clear();
                _trianglePoints.Add(clickPoint);
                _isDrawingTriangle = true;

                _trianglePreview = new Polygon
                {
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor),
                    StrokeThickness = ViewModel.CurrentStrokeSize,
                    Fill = null
                };

                var pointCollection = new PointCollection();
                pointCollection.Add(clickPoint);
                _trianglePreview.Points = pointCollection;

                DrawingCanvas.Children.Add(_trianglePreview);
            }
            else if (_trianglePoints.Count < 3)
            {
                _trianglePoints.Add(clickPoint);
                UpdateTrianglePreview();

                if (_trianglePoints.Count == 3)
                {
                    if (_trianglePreview != null)
                    {
                        DrawingCanvas.Children.Remove(_trianglePreview);
                    }

                    var triangleShape = new TriangleShape
                    {
                        Points = new List<Point>(_trianglePoints),
                        Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(ViewModel.CurrentColor),
                        StrokeThickness = ViewModel.CurrentStrokeSize
                    };

                    var minX = _trianglePoints.Min(p => p.X);
                    var minY = _trianglePoints.Min(p => p.Y);
                    var maxX = _trianglePoints.Max(p => p.X);
                    var maxY = _trianglePoints.Max(p => p.Y);

                    triangleShape.X = minX;
                    triangleShape.Y = minY;
                    triangleShape.Width = maxX - minX;
                    triangleShape.Height = maxY - minY;

                    ViewModel.Shapes.Add(triangleShape);

                    _trianglePreview = null;
                    _trianglePoints.Clear();
                    _isDrawingTriangle = false;
                }
            }

            e.Handled = true;
        }

        private void UpdateTrianglePreview()
        {
            if (_trianglePreview != null && _trianglePoints.Count > 0)
            {
                var pointCollection = new PointCollection();
                foreach (var pt in _trianglePoints)
                {
                    pointCollection.Add(pt);
                }
                _trianglePreview.Points = pointCollection;
            }
        }

        private Shape? CreatePreviewShape(ShapeType shapeType)
        {
            return shapeType switch
            {
                ShapeType.Line => new Line(),
                ShapeType.Rectangle => new Rectangle(),
                ShapeType.Ellipse => new Ellipse(),
                ShapeType.Circle => new Ellipse(),
                ShapeType.Polygon => new Polygon(),
                ShapeType.Triangle => new Polygon(),
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
                ShapeType.Circle => new CircleShape
                {
                    X = left,
                    Y = top,
                    Width = Math.Max(width, height),
                    Height = Math.Max(width, height)
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
                if (child != _previewShape && child != _polygonPreview && child != _trianglePreview && child is Shape)
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
                if (_draggingShape == shape)
                {
                    _draggingShape = null;
                    _isDragging = false;
                }
                if (_selectedUIElement == shape)
                {
                    _selectedUIElement = null;
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
                    uiShape.PointerMoved += Shape_PointerMoved;
                    uiShape.PointerReleased += Shape_PointerReleased;
                    uiShape.PointerCanceled += Shape_PointerReleased;
                    _shapeMapping[uiShape] = drawingShape;
                    DrawingCanvas.Children.Add(uiShape);
                    
                    // Update stored reference if this is the selected shape
                    if (ViewModel.SelectedShape == drawingShape)
                    {
                        _selectedUIElement = uiShape;
                    }
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

            if (drawingShape is TriangleShape triangleShape)
            {
                var triangle = new Polygon
                {
                    Stroke = triangleShape.Color,
                    StrokeThickness = triangleShape.StrokeThickness,
                    Fill = null
                };

                var pointCollection = new PointCollection();
                foreach (var pt in triangleShape.Points)
                {
                    pointCollection.Add(pt);
                }
                triangle.Points = pointCollection;

                return triangle;
            }

            Shape? uiShape = drawingShape switch
            {
                LineShape => new Line(),
                RectangleShape => new Rectangle(),
                EllipseShape => new Ellipse(),
                CircleShape => new Ellipse(),
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
            else if (drawingShape is CircleShape circleShape)
            {
                Canvas.SetLeft(uiShape, circleShape.X);
                Canvas.SetTop(uiShape, circleShape.Y);
                var size = Math.Max(circleShape.Width, circleShape.Height);
                uiShape.Width = size;
                uiShape.Height = size;
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
                // Set selected UI element immediately when shape is clicked
                _selectedUIElement = uiElement;
                ViewModel.SelectedShape = drawingShape;
                
                _draggingShape = uiElement;
                _isDragging = true;
                var point = e.GetCurrentPoint(DrawingCanvas);
                _dragStartPoint = point.Position;
                _shapeStartPosition = new Point(drawingShape.X, drawingShape.Y);
                
                uiElement.CapturePointer(e.Pointer);
                e.Handled = true;
            }
        }

        private void Shape_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_draggingShape != null && ReferenceEquals(sender, _draggingShape) && _shapeMapping.TryGetValue(_draggingShape, out var drawingShape))
            {
                try
                {
                    var point = e.GetCurrentPoint(DrawingCanvas);
                    var currentPoint = point.Position;
                    
                    var deltaX = currentPoint.X - _dragStartPoint.X;
                    var deltaY = currentPoint.Y - _dragStartPoint.Y;
                    
                    var newX = _shapeStartPosition.X + deltaX;
                    var newY = _shapeStartPosition.Y + deltaY;

                    if (drawingShape is LineShape && _draggingShape is Line line)
                    {
                        drawingShape.X = newX;
                        drawingShape.Y = newY;
                        line.X1 = drawingShape.X;
                        line.Y1 = drawingShape.Y;
                        line.X2 = drawingShape.X + drawingShape.Width;
                        line.Y2 = drawingShape.Y + drawingShape.Height;
                    }
                    else if (drawingShape is PolygonShape polygonShape && _draggingShape is Polygon polygon)
                    {
                        var deltaXTotal = newX - polygonShape.X;
                        var deltaYTotal = newY - polygonShape.Y;

                        for (int i = 0; i < polygonShape.Points.Count; i++)
                        {
                            var pt = polygonShape.Points[i];
                            polygonShape.Points[i] = new Point(pt.X + deltaXTotal, pt.Y + deltaYTotal);
                        }
                        
                        polygonShape.X = newX;
                        polygonShape.Y = newY;
                        
                        var newPoints = new PointCollection();
                        foreach (var pt in polygonShape.Points)
                        {
                            newPoints.Add(pt);
                        }
                        polygon.Points = newPoints;
                    }
                    else if (drawingShape is TriangleShape triangleShape && _draggingShape is Polygon triangle)
                    {
                        var deltaXTotal = newX - triangleShape.X;
                        var deltaYTotal = newY - triangleShape.Y;

                        for (int i = 0; i < triangleShape.Points.Count; i++)
                        {
                            var pt = triangleShape.Points[i];
                            triangleShape.Points[i] = new Point(pt.X + deltaXTotal, pt.Y + deltaYTotal);
                        }
                        
                        triangleShape.X = newX;
                        triangleShape.Y = newY;
                        
                        var newPoints = new PointCollection();
                        foreach (var pt in triangleShape.Points)
                        {
                            newPoints.Add(pt);
                        }
                        triangle.Points = newPoints;
                    }
                    else if (drawingShape is CircleShape circleShape && _draggingShape is Shape circle)
                    {
                        circleShape.X = newX;
                        circleShape.Y = newY;
                        Canvas.SetLeft(circle, newX);
                        Canvas.SetTop(circle, newY);
                    }
                    else
                    {
                        drawingShape.X = newX;
                        drawingShape.Y = newY;
                        Canvas.SetLeft(_draggingShape, drawingShape.X);
                        Canvas.SetTop(_draggingShape, drawingShape.Y);
                    }
                    
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    // Log error but don't crash
                    System.Diagnostics.Debug.WriteLine($"Error in Shape_PointerMoved: {ex.Message}");
                }
            }
        }

        private void Shape_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_draggingShape != null && ReferenceEquals(sender, _draggingShape))
            {
                try
                {
                    _draggingShape.ReleasePointerCapture(e.Pointer);
                }
                catch
                {
                    // Ignore if pointer was already released
                }
                _isDragging = false;
                _draggingShape = null;
                e.Handled = true;
            }
        }

        private void ShapeColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (ViewModel.SelectedShape != null)
            {
                var newColor = Windows.UI.Color.FromArgb(
                    args.NewColor.A,
                    args.NewColor.R,
                    args.NewColor.G,
                    args.NewColor.B);
                
                var colorBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);
                ViewModel.SelectedShape.Color = colorBrush;
                UpdateShapeColorPreview();
                // Don't call RenderAllShapes() - let PropertyChanged handler update the UI directly
            }
        }

        private void ClosePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedShape = null;
        }

        private void LoadTemplateToCanvas(ShapeTemplate template)
        {
            if (template == null)
                return;

            try
            {
                var templateService = App.Services.GetRequiredService<Windows_22120278.Services.ITemplateService>();
                var drawingShape = templateService.ConvertTemplateToDrawingShape(template);
                if (drawingShape != null)
                {
                    var newX = 100.0;
                    var newY = 100.0;
                    var oldX = drawingShape.X;
                    var oldY = drawingShape.Y;
                    var deltaX = newX - oldX;
                    var deltaY = newY - oldY;

                    if (drawingShape is Windows_22120278.Models.PolygonShape polygonShape)
                    {
                        if (polygonShape.Points != null && polygonShape.Points.Count >= 3)
                        {
                            for (int i = 0; i < polygonShape.Points.Count; i++)
                            {
                                var pt = polygonShape.Points[i];
                                polygonShape.Points[i] = new Point(pt.X + deltaX, pt.Y + deltaY);
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
                        else
                        {
                            return;
                        }
                    }
                    else if (drawingShape is Windows_22120278.Models.TriangleShape triangleShape)
                    {
                        if (triangleShape.Points != null && triangleShape.Points.Count == 3)
                        {
                            for (int i = 0; i < triangleShape.Points.Count; i++)
                            {
                                var pt = triangleShape.Points[i];
                                triangleShape.Points[i] = new Point(pt.X + deltaX, pt.Y + deltaY);
                            }
                            
                            var minX = triangleShape.Points.Min(p => p.X);
                            var minY = triangleShape.Points.Min(p => p.Y);
                            var maxX = triangleShape.Points.Max(p => p.X);
                            var maxY = triangleShape.Points.Max(p => p.Y);
                            
                            triangleShape.X = minX;
                            triangleShape.Y = minY;
                            triangleShape.Width = maxX - minX;
                            triangleShape.Height = maxY - minY;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (drawingShape is Windows_22120278.Models.CircleShape circleShape)
                    {
                        circleShape.X = newX;
                        circleShape.Y = newY;
                        var size = Math.Max(circleShape.Width, circleShape.Height);
                        if (size <= 0)
                            size = 50;
                        circleShape.Width = size;
                        circleShape.Height = size;
                    }
                    else if (drawingShape is Windows_22120278.Models.LineShape lineShape)
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

                    ViewModel.Shapes.Add(drawingShape);
                    System.Diagnostics.Debug.WriteLine($"Template loaded: {template.Name}, Shape type: {drawingShape.GetType().Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to convert template: {template.Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading template: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TemplatesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplatesGridView.SelectedItem is ShapeTemplate template)
            {
                LoadTemplateToCanvas(template);
                TemplatesGridView.SelectedItem = null;
            }
        }
    }
}

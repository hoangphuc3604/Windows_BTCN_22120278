using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows_22120278.Models;
using Windows_22120278.Services;
using Windows_22120278.ViewModels;
using Windows_22120278_Data.models;

namespace Windows_22120278.Views
{
    public sealed partial class TemplatesPage : Page
    {
        public BoardsViewModel ViewModel { get; }
        public List<string> BreadcrumbItems { get; } = new() { "Home", "Management", "Templates" };

        public TemplatesPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<BoardsViewModel>();
            var selectedProfileService = App.Services.GetRequiredService<ISelectedProfileService>();
            ViewModel.Profile = selectedProfileService.SelectedProfile;
            DataContext = ViewModel;
            Loaded += TemplatesPage_Loaded;
            ViewModel.Templates.CollectionChanged += Templates_CollectionChanged;
        }

        private async void Templates_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(100);
            DispatcherQueue.TryEnqueue(() => RenderTemplatePreviews());
        }

        private async void TemplatesPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.LoadTemplatesCommand.ExecuteAsync(null);
            
            await System.Threading.Tasks.Task.Delay(100);
            RenderTemplatePreviews();
        }

        private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is string item)
            {
                var navigationService = App.Services.GetRequiredService<INavigationService>();
                if (item == "Home")
                {
                    navigationService.NavigateTo("Home");
                }
                else if (item == "Management")
                {
                    navigationService.NavigateTo("Management");
                }
            }
        }

        private void TemplatesGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is ShapeTemplate template && !args.InRecycleQueue && args.ItemContainer != null)
            {
                var canvas = FindVisualChild<Canvas>(args.ItemContainer);
                if (canvas != null)
                {
                    RenderTemplateToCanvas(template, canvas);
                }
            }
        }

        private void RenderTemplatePreviews()
        {
            if (TemplatesGridView == null)
                return;

            foreach (var item in TemplatesGridView.Items)
            {
                if (item is ShapeTemplate template)
                {
                    var container = TemplatesGridView.ContainerFromItem(item) as GridViewItem;
                    if (container != null)
                    {
                        var canvas = FindVisualChild<Canvas>(container);
                        if (canvas != null)
                        {
                            RenderTemplateToCanvas(template, canvas);
                        }
                    }
                }
            }
        }

        private void RenderTemplateToCanvas(ShapeTemplate template, Canvas canvas)
        {
            canvas.Children.Clear();

            var templateService = App.Services.GetRequiredService<ITemplateService>();
            var drawingShape = templateService.ConvertTemplateToDrawingShape(template);
            
            if (drawingShape == null)
                return;

            var canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 176;
            var canvasHeight = canvas.ActualHeight > 0 ? canvas.ActualHeight : 112;

            var shapeWidth = Math.Max(drawingShape.Width, 1);
            var shapeHeight = Math.Max(drawingShape.Height, 1);

            var scaleX = (canvasWidth - 20) / shapeWidth;
            var scaleY = (canvasHeight - 20) / shapeHeight;
            var scale = Math.Min(scaleX, scaleY);
            scale = Math.Min(scale, 1.0);

            var centerX = canvasWidth / 2;
            var centerY = canvasHeight / 2;
            var scaledWidth = shapeWidth * scale;
            var scaledHeight = shapeHeight * scale;

            var offsetX = centerX - (scaledWidth / 2) - (drawingShape.X * scale);
            var offsetY = centerY - (scaledHeight / 2) - (drawingShape.Y * scale);

            Shape? uiShape = null;

            if (drawingShape is PolygonShape polygonShape)
            {
                var polygon = new Polygon
                {
                    Stroke = drawingShape.Color,
                    StrokeThickness = drawingShape.StrokeThickness * scale,
                    Fill = null
                };

                var pointCollection = new PointCollection();
                foreach (var pt in polygonShape.Points)
                {
                    pointCollection.Add(new Point(
                        (pt.X * scale) + offsetX,
                        (pt.Y * scale) + offsetY));
                }
                polygon.Points = pointCollection;
                uiShape = polygon;
            }
            else if (drawingShape is LineShape lineShape)
            {
                var line = new Line
                {
                    Stroke = drawingShape.Color,
                    StrokeThickness = drawingShape.StrokeThickness * scale,
                    X1 = (lineShape.X * scale) + offsetX,
                    Y1 = (lineShape.Y * scale) + offsetY,
                    X2 = ((lineShape.X + lineShape.Width) * scale) + offsetX,
                    Y2 = ((lineShape.Y + lineShape.Height) * scale) + offsetY
                };
                uiShape = line;
            }
            else if (drawingShape is RectangleShape)
            {
                var rect = new Rectangle
                {
                    Stroke = drawingShape.Color,
                    StrokeThickness = drawingShape.StrokeThickness * scale,
                    Fill = null,
                    Width = scaledWidth,
                    Height = scaledHeight
                };
                Canvas.SetLeft(rect, (drawingShape.X * scale) + offsetX);
                Canvas.SetTop(rect, (drawingShape.Y * scale) + offsetY);
                uiShape = rect;
            }
            else if (drawingShape is EllipseShape)
            {
                var ellipse = new Ellipse
                {
                    Stroke = drawingShape.Color,
                    StrokeThickness = drawingShape.StrokeThickness * scale,
                    Fill = null,
                    Width = scaledWidth,
                    Height = scaledHeight
                };
                Canvas.SetLeft(ellipse, (drawingShape.X * scale) + offsetX);
                Canvas.SetTop(ellipse, (drawingShape.Y * scale) + offsetY);
                uiShape = ellipse;
            }
            else if (drawingShape is CircleShape circleShape)
            {
                var circleSize = Math.Max(circleShape.Width, circleShape.Height);
                var scaledSize = circleSize * scale;
                var centerOffsetX = centerX - (scaledSize / 2);
                var centerOffsetY = centerY - (scaledSize / 2);
                
                var ellipse = new Ellipse
                {
                    Stroke = drawingShape.Color,
                    StrokeThickness = drawingShape.StrokeThickness * scale,
                    Fill = null,
                    Width = scaledSize,
                    Height = scaledSize
                };
                Canvas.SetLeft(ellipse, centerOffsetX);
                Canvas.SetTop(ellipse, centerOffsetY);
                uiShape = ellipse;
            }
            else if (drawingShape is TriangleShape triangleShape)
            {
                if (triangleShape.Points != null && triangleShape.Points.Count == 3)
                {
                    var triangle = new Polygon
                    {
                        Stroke = drawingShape.Color,
                        StrokeThickness = drawingShape.StrokeThickness * scale,
                        Fill = null
                    };

                    var pointCollection = new PointCollection();
                    foreach (var pt in triangleShape.Points)
                    {
                        pointCollection.Add(new Point(
                            (pt.X * scale) + offsetX,
                            (pt.Y * scale) + offsetY));
                    }
                    triangle.Points = pointCollection;
                    uiShape = triangle;
                }
            }

            if (uiShape != null)
            {
                canvas.Children.Add(uiShape);
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }
}


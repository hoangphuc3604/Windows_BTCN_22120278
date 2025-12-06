using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Windows.Foundation;
using Windows.UI;
using Windows_22120278.Models;
using Windows_22120278_Data;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly AppDbContext _context;

        public TemplateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShapeTemplate>> GetAllTemplatesAsync(int profileId)
        {
            return await _context.ShapeTemplates
                .Where(t => t.ProfileId == profileId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<ShapeTemplate> SaveTemplateAsync(string name, DrawingShape shape, int profileId)
        {
            var template = new ShapeTemplate
            {
                Name = name,
                Color = ConvertColorToHex(shape.Color),
                StrokeThickness = shape.StrokeThickness,
                ProfileId = profileId
            };

            if (shape is LineShape lineShape)
            {
                template.Type = ShapeType.Line;
                template.StartX = lineShape.X;
                template.StartY = lineShape.Y;
                template.EndX = lineShape.X + lineShape.Width;
                template.EndY = lineShape.Y + lineShape.Height;
            }
            else if (shape is RectangleShape rectShape)
            {
                template.Type = ShapeType.Rectangle;
                template.StartX = rectShape.X;
                template.StartY = rectShape.Y;
                template.EndX = rectShape.X + rectShape.Width;
                template.EndY = rectShape.Y + rectShape.Height;
            }
            else if (shape is EllipseShape ellipseShape)
            {
                template.Type = ShapeType.Ellipse;
                template.StartX = ellipseShape.X;
                template.StartY = ellipseShape.Y;
                template.EndX = ellipseShape.X + ellipseShape.Width;
                template.EndY = ellipseShape.Y + ellipseShape.Height;
            }
            else if (shape is PolygonShape polygonShape)
            {
                template.Type = ShapeType.Polygon;
                template.StartX = polygonShape.X;
                template.StartY = polygonShape.Y;
                template.EndX = polygonShape.X + polygonShape.Width;
                template.EndY = polygonShape.Y + polygonShape.Height;

                var pointsData = JsonSerializer.Serialize(polygonShape.Points.Select(p => new { X = p.X, Y = p.Y }));
                template.PointsData = pointsData;
            }

            _context.ShapeTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var template = await _context.ShapeTemplates.FindAsync(templateId);
            if (template == null)
                return false;

            _context.ShapeTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public DrawingShape? ConvertTemplateToDrawingShape(ShapeTemplate template)
        {
            DrawingShape? shape = template.Type switch
            {
                ShapeType.Line => new LineShape
                {
                    X = template.StartX,
                    Y = template.StartY,
                    Width = template.EndX - template.StartX,
                    Height = template.EndY - template.StartY
                },
                ShapeType.Rectangle => new RectangleShape
                {
                    X = template.StartX,
                    Y = template.StartY,
                    Width = Math.Abs(template.EndX - template.StartX),
                    Height = Math.Abs(template.EndY - template.StartY)
                },
                ShapeType.Ellipse => new EllipseShape
                {
                    X = template.StartX,
                    Y = template.StartY,
                    Width = Math.Abs(template.EndX - template.StartX),
                    Height = Math.Abs(template.EndY - template.StartY)
                },
                ShapeType.Polygon => new PolygonShape
                {
                    X = template.StartX,
                    Y = template.StartY,
                    Width = Math.Abs(template.EndX - template.StartX),
                    Height = Math.Abs(template.EndY - template.StartY),
                    Points = ParsePointsData(template.PointsData)
                },
                _ => null
            };

            if (shape != null)
            {
                shape.Color = ConvertHexToColor(template.Color);
                shape.StrokeThickness = template.StrokeThickness;
                
                if (shape is PolygonShape polygonShape && (polygonShape.Points == null || polygonShape.Points.Count < 3))
                {
                    return null;
                }
            }

            return shape;
        }

        private string ConvertColorToHex(Microsoft.UI.Xaml.Media.SolidColorBrush brush)
        {
            if (brush == null) return "#000000";
            var color = brush.Color;
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private Microsoft.UI.Xaml.Media.SolidColorBrush ConvertHexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#"))
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);

            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                var color = Windows.UI.Color.FromArgb(255, r, g, b);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
            }
            else if (hex.Length == 8)
            {
                var a = Convert.ToByte(hex.Substring(0, 2), 16);
                var r = Convert.ToByte(hex.Substring(2, 2), 16);
                var g = Convert.ToByte(hex.Substring(4, 2), 16);
                var b = Convert.ToByte(hex.Substring(6, 2), 16);
                var color = Windows.UI.Color.FromArgb(a, r, g, b);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
            }

            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        private List<Point> ParsePointsData(string pointsData)
        {
            if (string.IsNullOrEmpty(pointsData))
                return new List<Point>();

            try
            {
                var points = JsonSerializer.Deserialize<List<PointData>>(pointsData);
                if (points == null)
                    return new List<Point>();

                return points.Select(p => new Point(p.X, p.Y)).ToList();
            }
            catch
            {
                return new List<Point>();
            }
        }

        private class PointData
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}


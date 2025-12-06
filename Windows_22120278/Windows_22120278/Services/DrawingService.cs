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
    public class DrawingService : IDrawingService
    {
        private readonly AppDbContext _context;

        public DrawingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DrawingBoard> SaveDrawingAsync(DrawingBoard drawingBoard, List<DrawingShape> shapes)
        {
            if (drawingBoard.Id == 0)
            {
                drawingBoard.CreatedDate = DateTime.Now;
                _context.DrawingBoards.Add(drawingBoard);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existingBoard = await _context.DrawingBoards
                    .Include(d => d.Shapes)
                    .FirstOrDefaultAsync(d => d.Id == drawingBoard.Id);

                if (existingBoard != null)
                {
                    existingBoard.Name = drawingBoard.Name;
                    existingBoard.Width = drawingBoard.Width;
                    existingBoard.Height = drawingBoard.Height;
                    existingBoard.BackgroundColor = drawingBoard.BackgroundColor;

                    _context.Shapes.RemoveRange(existingBoard.Shapes);
                    await _context.SaveChangesAsync();
                }
            }

            foreach (var shape in shapes)
            {
                var shapeEntity = ConvertToShapeEntity(shape, drawingBoard.Id);
                _context.Shapes.Add(shapeEntity);
            }

            await _context.SaveChangesAsync();
            return drawingBoard;
        }

        private ShapeEntity ConvertToShapeEntity(DrawingShape drawingShape, int drawingBoardId)
        {
            var shapeEntity = new ShapeEntity
            {
                DrawingBoardId = drawingBoardId,
                Color = ConvertColorToHex(drawingShape.Color),
                StrokeThickness = drawingShape.StrokeThickness
            };

            if (drawingShape is LineShape lineShape)
            {
                shapeEntity.Type = ShapeType.Line;
                shapeEntity.StartX = lineShape.X;
                shapeEntity.StartY = lineShape.Y;
                shapeEntity.EndX = lineShape.X + lineShape.Width;
                shapeEntity.EndY = lineShape.Y + lineShape.Height;
            }
            else if (drawingShape is RectangleShape rectShape)
            {
                shapeEntity.Type = ShapeType.Rectangle;
                shapeEntity.StartX = rectShape.X;
                shapeEntity.StartY = rectShape.Y;
                shapeEntity.EndX = rectShape.X + rectShape.Width;
                shapeEntity.EndY = rectShape.Y + rectShape.Height;
            }
            else if (drawingShape is EllipseShape ellipseShape)
            {
                shapeEntity.Type = ShapeType.Ellipse;
                shapeEntity.StartX = ellipseShape.X;
                shapeEntity.StartY = ellipseShape.Y;
                shapeEntity.EndX = ellipseShape.X + ellipseShape.Width;
                shapeEntity.EndY = ellipseShape.Y + ellipseShape.Height;
            }
            else if (drawingShape is PolygonShape polygonShape)
            {
                shapeEntity.Type = ShapeType.Polygon;
                shapeEntity.StartX = polygonShape.X;
                shapeEntity.StartY = polygonShape.Y;
                shapeEntity.EndX = polygonShape.X + polygonShape.Width;
                shapeEntity.EndY = polygonShape.Y + polygonShape.Height;

                var pointsData = JsonSerializer.Serialize(polygonShape.Points.Select(p => new { X = p.X, Y = p.Y }));
                shapeEntity.PointsData = pointsData;
            }
            else if (drawingShape is CircleShape circleShape)
            {
                shapeEntity.Type = ShapeType.Circle;
                shapeEntity.StartX = circleShape.X;
                shapeEntity.StartY = circleShape.Y;
                var size = Math.Max(circleShape.Width, circleShape.Height);
                shapeEntity.EndX = circleShape.X + size;
                shapeEntity.EndY = circleShape.Y + size;
            }
            else if (drawingShape is TriangleShape triangleShape)
            {
                shapeEntity.Type = ShapeType.Triangle;
                shapeEntity.StartX = triangleShape.X;
                shapeEntity.StartY = triangleShape.Y;
                shapeEntity.EndX = triangleShape.X + triangleShape.Width;
                shapeEntity.EndY = triangleShape.Y + triangleShape.Height;

                var pointsData = JsonSerializer.Serialize(triangleShape.Points.Select(p => new { X = p.X, Y = p.Y }));
                shapeEntity.PointsData = pointsData;
            }

            return shapeEntity;
        }

        public async Task<(DrawingBoard? board, List<DrawingShape> shapes)> GetDrawingAsync(int drawingBoardId)
        {
            var board = await _context.DrawingBoards
                .Include(d => d.Shapes)
                .Include(d => d.Profile)
                .FirstOrDefaultAsync(d => d.Id == drawingBoardId);

            if (board == null)
                return (null, new List<DrawingShape>());

            var shapes = new List<DrawingShape>();
            foreach (var shapeEntity in board.Shapes)
            {
                var drawingShape = ConvertToDrawingShape(shapeEntity);
                if (drawingShape != null)
                {
                    shapes.Add(drawingShape);
                }
            }

            return (board, shapes);
        }

        public async Task<DrawingBoard?> GetLatestDrawingBoardAsync(int profileId)
        {
            return await _context.DrawingBoards
                .Where(d => d.ProfileId == profileId)
                .Include(d => d.Profile)
                .OrderByDescending(d => d.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DrawingBoard>> GetDrawingBoardsByProfileIdAsync(int profileId)
        {
            return await _context.DrawingBoards
                .Where(d => d.ProfileId == profileId)
                .Include(d => d.Profile)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        public async Task<DrawingBoard> CreateDrawingBoardAsync(DrawingBoard board)
        {
            board.CreatedDate = DateTime.Now;
            _context.DrawingBoards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<bool> DeleteDrawingBoardAsync(int boardId)
        {
            var board = await _context.DrawingBoards.FindAsync(boardId);
            if (board == null)
                return false;

            _context.DrawingBoards.Remove(board);
            await _context.SaveChangesAsync();
            return true;
        }

        private DrawingShape? ConvertToDrawingShape(ShapeEntity shapeEntity)
        {
            DrawingShape? shape = shapeEntity.Type switch
            {
                ShapeType.Line => new LineShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndY - shapeEntity.StartY
                },
                ShapeType.Rectangle => new RectangleShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndY - shapeEntity.StartY
                },
                ShapeType.Ellipse => new EllipseShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndY - shapeEntity.StartY
                },
                ShapeType.Polygon => new PolygonShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndY - shapeEntity.StartY,
                    Points = DeserializePolygonPoints(shapeEntity.PointsData)
                },
                ShapeType.Circle => new CircleShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndX - shapeEntity.StartX
                },
                ShapeType.Triangle => new TriangleShape
                {
                    X = shapeEntity.StartX,
                    Y = shapeEntity.StartY,
                    Width = shapeEntity.EndX - shapeEntity.StartX,
                    Height = shapeEntity.EndY - shapeEntity.StartY,
                    Points = DeserializePolygonPoints(shapeEntity.PointsData)
                },
                _ => null
            };

            if (shape != null)
            {
                shape.Color = ConvertHexToColorBrush(shapeEntity.Color);
                shape.StrokeThickness = shapeEntity.StrokeThickness;
            }

            return shape;
        }

        private List<Point> DeserializePolygonPoints(string pointsData)
        {
            if (string.IsNullOrEmpty(pointsData))
                return new List<Point>();

            try
            {
                var jsonPoints = JsonSerializer.Deserialize<List<JsonPoint>>(pointsData);
                if (jsonPoints == null)
                    return new List<Point>();

                return jsonPoints.Select(p => new Point(p.X, p.Y)).ToList();
            }
            catch
            {
                return new List<Point>();
            }
        }

        private Microsoft.UI.Xaml.Media.SolidColorBrush ConvertHexToColorBrush(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#"))
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);

            try
            {
                hexColor = hexColor.TrimStart('#');
                if (hexColor.Length == 6)
                {
                    var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    var color = Windows.UI.Color.FromArgb(255, r, g, b);
                    return new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
                }
            }
            catch { }

            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        private string ConvertColorToHex(Microsoft.UI.Xaml.Media.SolidColorBrush brush)
        {
            if (brush?.Color == null)
                return "#000000";

            var color = brush.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private class JsonPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}


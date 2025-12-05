using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Windows_22120278_Data.models
{
    public class ShapeEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(DrawingBoard))]
        public int DrawingBoardId { get; set; }

        public DrawingBoard? DrawingBoard { get; set; }

        public ShapeType Type { get; set; }

        public string Color { get; set; } = "#000000";

        public double StrokeThickness { get; set; } = 1.0;

        public double StartX { get; set; }

        public double StartY { get; set; }

        public double EndX { get; set; }

        public double EndY { get; set; }

        public string PointsData { get; set; } = string.Empty;
    }
}


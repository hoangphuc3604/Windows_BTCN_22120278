using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Windows_22120278_Data.models
{
    public class ShapeTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public ShapeType Type { get; set; }

        public string Color { get; set; } = "#000000";

        public double StrokeThickness { get; set; } = 1.0;

        public double StartX { get; set; }

        public double StartY { get; set; }

        public double EndX { get; set; }

        public double EndY { get; set; }

        public string PointsData { get; set; } = string.Empty;

        // FK to Profile, since each Profile owns many templates
        [ForeignKey(nameof(Profile))]
        public int ProfileId { get; set; }

        public Profile? Profile { get; set; }
    }
}



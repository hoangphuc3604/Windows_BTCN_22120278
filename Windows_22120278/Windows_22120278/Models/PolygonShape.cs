using System.Collections.Generic;
using Windows.Foundation;

namespace Windows_22120278.Models
{
    public partial class PolygonShape : DrawingShape
    {
        public List<Point> Points { get; set; } = new();
    }
}


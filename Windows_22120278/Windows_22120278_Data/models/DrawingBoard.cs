using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Windows_22120278_Data.models
{
    public class DrawingBoard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public double Width { get; set; }

        public double Height { get; set; }

        public string BackgroundColor { get; set; } = "#FFFFFF";

        public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(Profile))]
        public int ProfileId { get; set; }

        public Profile? Profile { get; set; }

        public List<ShapeEntity> Shapes { get; set; } = new();
    }
}


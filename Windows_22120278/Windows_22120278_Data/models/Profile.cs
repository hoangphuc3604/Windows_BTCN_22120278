using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Windows_22120278_Data.models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsDefaultThemeDark { get; set; }

        public double DefaultBoardWidth { get; set; }

        public double DefaultBoardHeight { get; set; }

        // Navigation: one Profile owns many DrawingBoards
        public List<DrawingBoard> DrawingBoards { get; set; } = new();

        // Navigation: one Profile owns many ShapeTemplates
        public List<ShapeTemplate> ShapeTemplates { get; set; } = new();
    }
}


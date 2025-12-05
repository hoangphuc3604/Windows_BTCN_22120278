using System.ComponentModel.DataAnnotations;

namespace Models
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
    }
}

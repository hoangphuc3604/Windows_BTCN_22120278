using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Windows_22120278.Views
{
    public sealed partial class CreateBoardDialog : ContentDialog
    {
        public CreateBoardDialog(double defaultWidth, double defaultHeight, string defaultBackgroundColor = "#FFFFFF")
        {
            InitializeComponent();
            
            WidthNumberBox.Value = defaultWidth;
            HeightNumberBox.Value = defaultHeight;
            
            if (!string.IsNullOrEmpty(defaultBackgroundColor) && defaultBackgroundColor.StartsWith("#"))
            {
                try
                {
                    var hex = defaultBackgroundColor.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        var r = Convert.ToByte(hex.Substring(0, 2), 16);
                        var g = Convert.ToByte(hex.Substring(2, 2), 16);
                        var b = Convert.ToByte(hex.Substring(4, 2), 16);
                        var color = Color.FromArgb(255, r, g, b);
                        ColorPickerControl.Color = color;
                        ColorPreviewBorder.Background = new SolidColorBrush(color);
                    }
                }
                catch { }
            }

            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
        }

        private void ColorPickerControl_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            var color = Windows.UI.Color.FromArgb(
                args.NewColor.A,
                args.NewColor.R,
                args.NewColor.G,
                args.NewColor.B);
            ColorPreviewBorder.Background = new SolidColorBrush(color);
        }

        public string GetBoardName()
        {
            return NameTextBox.Text?.Trim() ?? $"Board {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        public double GetWidth()
        {
            return WidthNumberBox.Value;
        }

        public double GetHeight()
        {
            return HeightNumberBox.Value;
        }

        public string GetBackgroundColor()
        {
            var color = ColorPickerControl.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}


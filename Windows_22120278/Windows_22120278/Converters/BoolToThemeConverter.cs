using System;
using Microsoft.UI.Xaml.Data;

namespace Windows_22120278.Converters
{
    public class BoolToThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDark)
            {
                return isDark ? "Dark Theme" : "Light Theme";
            }
            return "System Theme";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}


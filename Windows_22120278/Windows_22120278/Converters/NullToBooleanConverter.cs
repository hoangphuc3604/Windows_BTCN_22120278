using System;
using Microsoft.UI.Xaml.Data;

namespace Windows_22120278.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter?.ToString() == "Invert")
            {
                return value != null;
            }
            return value == null ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}


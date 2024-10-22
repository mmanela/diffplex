using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiffPlex.Wpf.Controls
{
    public class BooleanToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTextWrapEnabled)
            {
                return isTextWrapEnabled ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
            }
            return ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SubtitleTranslator.Application.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ok = (parameter == null || bool.Parse(parameter.ToString()));

            return ((bool) value == ok ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ok = (parameter == null || bool.Parse(parameter.ToString()));

            return ((Visibility) value == Visibility.Visible ? ok : !ok);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Mapping_Tools.Classes.BeatmapHelper.Enums;

namespace Mapping_Tools.Components.Domain
{
    public class BooleanOrToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (object value in values) {
                if ((value is bool) && (bool)value == false) {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanOrConverter is a OneWay converter.");
        }
    }
}

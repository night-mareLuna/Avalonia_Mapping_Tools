using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Mapping_Tools.Components.Domain {
    public class FlagToBoolConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value != null && parameter != null && ((Enum) value).HasFlag((Enum) parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value != null && parameter != null && (bool) value ? parameter : BindingOperations.DoNothing;
        }
    }
}
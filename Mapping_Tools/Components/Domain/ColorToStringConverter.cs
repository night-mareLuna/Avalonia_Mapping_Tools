using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Mapping_Tools.Components.Domain {
    internal class ColorToStringConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var str = ((Color)value!).ToString();
            if (str.Length == 9) {
                str = string.Concat("#", str.AsSpan(3, str.Length - 3));
            }
            return str;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string str = value!.ToString()!;
			if(Color.TryParse(str, out var color))
				return color;
			return Colors.Transparent;
        }
    }
}

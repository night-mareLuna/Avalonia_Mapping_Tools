using System;
using System.Globalization;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Avalonia.Data.Converters;

namespace Mapping_Tools.Components.Domain {
    class HitsoundToStringConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return ((Hitsound)value!).ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string str = value!.ToString()!;
            return Enum.Parse(typeof(Hitsound), str);
        }
    }
}

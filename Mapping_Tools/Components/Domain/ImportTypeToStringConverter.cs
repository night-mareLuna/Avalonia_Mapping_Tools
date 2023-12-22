﻿using Avalonia.Data.Converters;
using Mapping_Tools.Classes.HitsoundStuff;
using System;
using System.Globalization;

namespace Mapping_Tools.Components.Domain {
    class ImportTypeToStringConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return ((ImportType)value!).ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string str = value!.ToString()!;
            return Enum.Parse(typeof(ImportType), str);
        }
    }
}

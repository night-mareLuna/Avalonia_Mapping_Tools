﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Mapping_Tools.Components.Domain
{
    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (object value in values) {
                if ((value is bool) && (bool)value) {
                    return true;
                }
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanOrConverter is a OneWay converter.");
        }
    }
}

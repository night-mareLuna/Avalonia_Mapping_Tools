﻿using Mapping_Tools.Classes.HitsoundStuff;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mapping_Tools.Components.Domain {
    class ImportTypeToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((ImportType)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string str = value.ToString();
            switch (str) {
                case "None":
                    return ImportType.None;
                case "Stack":
                    return ImportType.Stack;
                case "Hitsounds":
                    return ImportType.Hitsounds;
                case "MIDI":
                    return ImportType.MIDI;
                default:
                    return ImportType.None;
            }
        }
    }
}

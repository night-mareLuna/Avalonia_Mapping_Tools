﻿using Mapping_Tools.Classes.BeatmapHelper;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Mapping_Tools.Classes.BeatmapHelper.BeatDivisors;
using Mapping_Tools.Classes.SystemTools;
using Avalonia.Data.Converters;
using System.ComponentModel.DataAnnotations;
using Avalonia.Data;

namespace Mapping_Tools.Components.Domain {
    internal class BeatDivisorArrayToStringConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (!(value is IBeatDivisor[] beatDivisors)) return string.Empty;

            var builder = new StringBuilder();
            bool first = true;
            foreach (var beatDivisor in beatDivisors) {
                if (!first) {
                    builder.Append(", ");
                }

                switch (beatDivisor) {
                    case RationalBeatDivisor rbd:
                        builder.Append($"{rbd.Numerator.ToInvariant()}/{rbd.Denominator.ToInvariant()}");
                        break;
                    case IrrationalBeatDivisor ibd:
                        builder.Append(ibd.GetValue().ToInvariant());
                        break;
                }

                first = false;
            }

            return builder.ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (!(value is string str)) return Array.Empty<IBeatDivisor>();

            var vals = str.Split(',');
            var beatDivisors = new IBeatDivisor[vals.Length];

            for (int i = 0; i < vals.Length; i++) {
                var val = vals[i];

                // Check if it is a positive rational and non-zero and not dividing by zero
                if (Regex.IsMatch(val, "^[\\s]*[1-9][0-9]*[\\s]*/[\\s]*[1-9][0-9]*[\\s]*$")) {
                    var ndSplit = val.Split('/');
                    beatDivisors[i] = new RationalBeatDivisor(int.Parse(ndSplit[0], CultureInfo.InvariantCulture),
                                                            int.Parse(ndSplit[1], CultureInfo.InvariantCulture));
                } else {
                    var valid = TypeConverters.TryParseDouble(val, out double doubleValue);
                    if (valid) {
                        if (doubleValue <= 0)
                            return new BindingNotification(new ValidationException("Beat divisor must be greater than zero."),
								BindingErrorType.DataValidationError);

                        beatDivisors[i] = new IrrationalBeatDivisor(doubleValue);
                    } else {
                        return new BindingNotification(new ValidationException("Double format error."),
							BindingErrorType.DataValidationError);
                    }
                }
            }

            return beatDivisors;
        }
    }
}

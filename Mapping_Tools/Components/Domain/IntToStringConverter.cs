using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Mapping_Tools.Classes.SystemTools;

namespace Mapping_Tools.Components.Domain {
    internal class IntToStringConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value != null) {
                if ((int)value == 727) {
                    return "727 WYSI";
                }
                return ((int) value).ToString(CultureInfo.InvariantCulture);
            }
            return parameter != null ? parameter.ToString()! : "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null) {
                if (parameter != null) {
                    return (int) parameter;
                }
                return new BindingNotification(new ValidationException("Cannot convert back null."),
					BindingErrorType.DataValidationError);
            }

            if (value.ToString() == "727 WYSI") {
                return 727;
            }

            if (parameter == null) {
                if (TypeConverters.TryParseInt(value.ToString()!, out int result1)) {
                    return result1;
                }

                return new BindingNotification(new ValidationException("Int format error."),
					BindingErrorType.DataValidationError);
            }
            if(TypeConverters.TryParseInt(value.ToString()!, out int result2, int.Parse(parameter.ToString()!)))
				return result2;

			return new BindingNotification(new ValidationException("Something went wrong"),
				BindingErrorType.Error);
        }
    }
}

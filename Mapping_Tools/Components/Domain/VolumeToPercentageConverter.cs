using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Mapping_Tools.Classes.SystemTools;

namespace Mapping_Tools.Components.Domain {
    class VolumeToPercentageConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return ((double)value! * 100).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(TypeConverters.TryParseDouble(value!.ToString()!, out double result, double.Parse(parameter!.ToString()!)))
            	return result / 100;
			return new BindingNotification(new ValidationException("Something went wrong"),
				BindingErrorType.DataValidationError);
        }
    }
}

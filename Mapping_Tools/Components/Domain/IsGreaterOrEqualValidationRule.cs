using System.ComponentModel.DataAnnotations;

namespace Mapping_Tools.Components.Domain;
public sealed class GreaterThanOrEqualAttribute(double propertyName) : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
		if(value is null) return new("Error");

		return propertyName <= (double)value ?
			ValidationResult.Success! : new($"Value needs to be greater than or equal {propertyName}!");
    }
}
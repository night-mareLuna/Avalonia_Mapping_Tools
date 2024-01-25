using System.ComponentModel.DataAnnotations;

namespace Mapping_Tools.Components.Domain;
public sealed class GreaterThanAttribute(double propertyName) : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
		if(value is null) return new("Error");

		var numericValue = 0.0;

		if(value is int i)
			numericValue = i;
		else if(value is double d)
			numericValue = d;
		else return new("Parse Error.");

		return propertyName < numericValue ?
			ValidationResult.Success! : new($"Value needs to be greater than {propertyName}!");
    }
}
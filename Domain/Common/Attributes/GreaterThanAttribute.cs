using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly bool _validateNullable;

        public GreaterThanAttribute(string comparisonProperty, bool validateNullable = true)
        {
            _comparisonProperty = comparisonProperty;
            this._validateNullable = validateNullable;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return _validateNullable ? new ValidationResult("Invalid entry") : ValidationResult.Success;

            ErrorMessage = ErrorMessageString;

            if (value.GetType() == typeof(IComparable)) throw new ArgumentException("value has not implemented IComparable interface");
            var currentValue = (IComparable)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null) throw new ArgumentException("Comparison property with this name not found");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);
            if (comparisonValue == null)
            {
                // Skip validation for nullable fields if _validateNullable is false
                return _validateNullable ? new ValidationResult("Invalid entry") : ValidationResult.Success;
            }
            if (!ReferenceEquals(value.GetType(), comparisonValue.GetType()))
                throw new ArgumentException("The types of the fields to compare are not the same.");

            return currentValue.CompareTo((IComparable)comparisonValue) > 0 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class GreaterThanTodayAttribute : ValidationAttribute
    {
        private readonly bool _validateNullable;

        public GreaterThanTodayAttribute(bool validateNullable = true)
        {
            _validateNullable = validateNullable;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Skip validation for nullable fields if _validateNullable is false
                return _validateNullable ? new ValidationResult("Invalid entry") : ValidationResult.Success;
            }
            if (value is not DateTime) return new ValidationResult("Invalid entry");
            ErrorMessage = ErrorMessageString;

            if (value.GetType() == typeof(IComparable)) throw new ArgumentException("value has not implemented IComparable interface");
            var currentValue = (IComparable)value;

            var comparisonValue = DateTime.Today;
            if (!ReferenceEquals(value.GetType(), comparisonValue.GetType()))
                throw new ArgumentException("The types of the fields to compare are not the same.");

            return currentValue.CompareTo((IComparable)comparisonValue) > 0 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;

namespace Domain.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfAllNotNullAttribute : ValidationAttribute
    {
        private readonly string[] _propertyNames;

        public RequiredIfAllNotNullAttribute(params string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;
            bool allPropertiesAreNull = true;

            // Check if all specified properties are null
            foreach (var propertyName in _propertyNames)
            {
                if (string.IsNullOrEmpty(propertyName)) continue;
                var propertyInfo = model.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    return new ValidationResult($"Property {propertyName} not found.");
                }

                var propertyValue = propertyInfo.GetValue(model);
                if (propertyValue != null)
                {
                    allPropertiesAreNull = false;
                    break;
                }
            }

            // If all specified properties are null, none of them are required.
            if (allPropertiesAreNull)
            {
                return ValidationResult.Success;
            }

            // If any of the specified properties is null, the current property is required.
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}

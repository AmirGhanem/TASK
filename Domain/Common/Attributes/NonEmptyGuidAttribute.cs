using Domain.Common;
using System.ComponentModel.DataAnnotations;
[AttributeUsage(AttributeTargets.Property)]
public class NonEmptyGuidAttribute : ValidationAttribute
{
    private readonly ErrorCode errorCode;

    public NonEmptyGuidAttribute(ErrorCode ErrorCode=ErrorCode.FieldRequired)
    {
        errorCode = ErrorCode;
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if ((value is Guid) && (value==null || Guid.Empty == (Guid)value))
        {
            return new ValidationResult(errorCode.ToString());
        }
        return null;
    }
}
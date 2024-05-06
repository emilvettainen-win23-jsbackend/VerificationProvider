using System.ComponentModel.DataAnnotations;
using VerificationProvider.Models;

namespace VerificationProvider.Helpers;

public class CustomValidation
{
    public static ValidationModel<VerificationRequestModel> ValidateVerificationRequest(VerificationRequestModel verificationRequest)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(verificationRequest);
        var isValid = Validator.TryValidateObject(verificationRequest, context, validationResults, true);

        return new ValidationModel<VerificationRequestModel>
        {
            IsValid = isValid,
            Value = verificationRequest,
            ValidationResults = validationResults
        };
    }
}

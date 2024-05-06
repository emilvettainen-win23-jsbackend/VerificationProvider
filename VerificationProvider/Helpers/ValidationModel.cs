using System.ComponentModel.DataAnnotations;

namespace VerificationProvider.Helpers;

public class ValidationModel<T>
{
    public bool IsValid { get; set; }
    public T? Value { get; set; }

    public IEnumerable<ValidationResult> ValidationResults { get; set; } = [];
}
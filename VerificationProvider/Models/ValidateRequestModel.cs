namespace VerificationProvider.Models;

public class ValidateRequestModel
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}

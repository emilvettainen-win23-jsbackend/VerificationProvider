using System.ComponentModel.DataAnnotations;

namespace VerificationProvider.Models;

public class VerificationRequestModel
{
    [RegularExpression(@"^(([^<>()\]\\.,;:\s@\""]+(\.[^<>()\]\\.,;:\s@\""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", ErrorMessage = "The field Email must match xx@xx.xx")]
    public string Email { get; set; } = null!;
}
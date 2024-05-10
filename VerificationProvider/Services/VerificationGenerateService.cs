using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationGenerateService(ILogger<VerificationGenerateService> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<VerificationGenerateService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;


    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);
            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationGenerateService.GenerateCode() :: {ex.Message}");
        }
        return null!;
    }


    public EmailRequestModel GenerateEmailRequest(VerificationRequestModel verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequestModel
                {
                    To = verificationRequest.Email,
                    Subject = $"Verification Code {code}",
                    HtmlBody = $@"<html lang='en'>
                                <head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='with=device-width, initial-scale=1.0'>
                                <title>Verification Code</title>
                                </head>
                                <body>
                                    <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff;'>
                                        <div style='background-color: #0046ae; color: white; padding: 10px 20px; text-align: center;'>
                                            <h1>Verify</h1>
                                        </div>
                                        <div style='padding: 20px;'>
                                            <p>Hello,</p>
                                            <p>To complete your request, please enter the following verification code in the appropriate field:</p>
                                            <p style='font-weight: bold; font-size: 24px; color: #0046ae;'>{code}</p>
                                            <p style='margin-top: 20px;'>If you did not request this code, you can safely ignore this email. Otherwise, please proceed to verify your request.</p>
                                            <p>Thank you!<br>Silicon</p>
                                        </div>
                                    </div>
                                </body>
                                </html>",
                    PlainText = $"Verify, To complete your request, please enter the following verification code: {code} in the appropriate field, If you did not request this code, you can safely ignore this email. Otherwise, please proceed to verify your request. Thank you! Silicon"
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationGenerateService.GenerateEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequestAsync(VerificationRequestModel verificationRequestModel, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<VerificationDataContext>();
            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequestModel.Email);
            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new Data.Entities.VerificationRequestEntity() { Email = verificationRequestModel.Email, Code = code });
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationGenerateService.SaveVerificationRequestAsync() :: {ex.Message}");
        }
        return false;
    }


    public void LogValidationErrors(IEnumerable<ValidationResult> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError($"Validation Error: {error.ErrorMessage}");
        }
    }
}

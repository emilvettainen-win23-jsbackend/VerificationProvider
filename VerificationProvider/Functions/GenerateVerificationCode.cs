using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Helpers.Validations;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider, VerificationGenerateService verificationService)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly VerificationGenerateService _verificationService = verificationService;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(Encoding.UTF8.GetString(message.Body));
            if (verificationRequest == null)
            {
                return null!;
            }
            var modelState = CustomValidation.ValidateModel(verificationRequest);
            if (!modelState.IsValid)
            {
                _verificationService.LogValidationErrors(modelState.ValidationResults);
                return null!;
            }
            var code = _verificationService.GenerateCode();
            var saveRequest = await _verificationService.SaveVerificationRequestAsync(verificationRequest, code);
            if (!saveRequest)
            {
                return null!;
            }
            var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
            var payload = JsonConvert.SerializeObject(emailRequest);
            await messageActions.CompleteMessageAsync(message);
            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.Run() :: {ex.Message}");
        }
        return null!;
    }
}
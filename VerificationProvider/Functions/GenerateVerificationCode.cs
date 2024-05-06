using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Helpers;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode
{
    private readonly ILogger<GenerateVerificationCode> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly VerificationService _verificationService;

    public GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider, VerificationService verificationService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _verificationService = verificationService;
    }

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(message.Body.ToString());
            if (verificationRequest == null)
            {
                return null!;
            }
            var validation = CustomValidation.ValidateVerificationRequest(verificationRequest);
            if (!validation.IsValid)
            {
                _verificationService.LogValidationErrors(validation.ValidationResults);
                return null!;
            }
            var code = _verificationService.GenerateCode();
            var saveRequest = await _verificationService.SaveVerificationRequest(verificationRequest, code);
            if (!saveRequest)
            {
                return null!;
            }
            var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
            var payload = JsonConvert.SerializeObject(emailRequest);
            await messageActions.CompleteMessageAsync(message);
            return payload;


            //var verificationRequest = _verificationService.UnpackVerificationRequest(message);
            //if (verificationRequest != null)
            //{
            //    var code = _verificationService.GenerateCode();
            //    if (!string.IsNullOrEmpty(code))
            //    {
            //        var result = await _verificationService.SaveVerificationRequest(verificationRequest, code);
            //        if (result)
            //        {
            //            var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
            //            if (emailRequest != null)
            //            {
            //                var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
            //                if (!string.IsNullOrEmpty(payload))
            //                {
            //                    await messageActions.CompleteMessageAsync(message);
            //                    return payload;
            //                }
            //            }
            //        }
            //    }
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.Run() :: {ex.Message}");
        }
        return null!;
    }

  

}

using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using VerificationProvider.Helpers.Validations;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions.Http
{
    public class GenerateVerificationCodeHttp(ILogger<GenerateVerificationCodeHttp> logger, VerificationGenerateService verificationGenerateService, ServiceBusClient serviceBusClient)
    {
        private readonly ILogger<GenerateVerificationCodeHttp> _logger = logger;
        private readonly VerificationGenerateService _verificationGenerateService = verificationGenerateService;
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

        [Function("GenerateVerificationCodeHttp")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,"post", Route = "generatecode")] HttpRequest req)
        {
            try
            {
                var verificationRequest = await new StreamReader(req.Body).ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<VerificationRequestModel>(verificationRequest);
                if (model == null)
                {
                    return new BadRequestResult();
                }
                var modelState = CustomValidation.ValidateModel(model);
                if (!modelState.IsValid)
                {
                    _verificationGenerateService.LogValidationErrors(modelState.ValidationResults);
                    return new BadRequestObjectResult(modelState.ValidationResults);
                }

                var code = _verificationGenerateService.GenerateCode();
                var saveRequest = await _verificationGenerateService.SaveVerificationRequestAsync(model, code);
                if (!saveRequest)
                {
                    return new BadRequestResult();
                }
                var emailRequest = _verificationGenerateService.GenerateEmailRequest(model, code);
                if (emailRequest == null)
                {
                    return new BadRequestResult();
                }
                var sender = _serviceBusClient.CreateSender("email_request");
                await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailRequest))));
                return new OkResult();

            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : GenerateVerificationCodeHttp.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
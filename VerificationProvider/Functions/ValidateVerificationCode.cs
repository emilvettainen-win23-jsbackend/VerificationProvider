using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Helpers.Validations;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class ValidateVerificationCode
{
    private readonly ILogger<ValidateVerificationCode> _logger;
    private readonly ValidateVerificationService _validateVerificationService;

    public ValidateVerificationCode(ILogger<ValidateVerificationCode> logger, ValidateVerificationService validateVerificationService)
    {
        _logger = logger;
        _validateVerificationService = validateVerificationService;
    }

    [Function("ValidateVerificationCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "validatecode")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateVerificationService.UnpackValidateRequestAsync(req);
            if (validateRequest == null)
            {
                return new BadRequestResult();
            }
            var modelState = CustomValidation.ValidateModel(validateRequest);
            if (!modelState.IsValid)
            {
                return new BadRequestResult();
            }
            var validateResult = await _validateVerificationService.ValidateCodeAsync(validateRequest);
            if (!validateResult)
            {
                return new UnauthorizedResult();
            }
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.Run() :: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;
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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "verification")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateVerificationService.UnpackValidateRequestAsync(req);
            if (validateRequest != null)
            {
                var validateResult = await _validateVerificationService.ValidateCodeAsync(validateRequest);
                if (validateResult)
                {
                    return new OkResult();
                }
            }

        }
        catch (Exception ex)
        {

            _logger.LogError($"ERROR : ValidateVerificationCode.Run() :: {ex.Message}");
        }
        return new UnauthorizedResult();
       
    }

   
}

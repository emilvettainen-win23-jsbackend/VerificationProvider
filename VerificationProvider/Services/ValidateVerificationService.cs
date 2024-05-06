using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public class ValidateVerificationService
    {
        private readonly ILogger<ValidateVerificationService> _logger;
        private readonly VerificationDataContext _context;

        public ValidateVerificationService(ILogger<ValidateVerificationService> logger, VerificationDataContext verificationDataContext)
        {
            _logger = logger;
            _context = verificationDataContext;
        }

        public async Task<ValidateRequestModel> UnpackValidateRequestAsync(HttpRequest req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    var validateRequest = JsonConvert.DeserializeObject<ValidateRequestModel>(body);
                    if (validateRequest != null)
                        return validateRequest;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"ERROR : ValidateVerificationService.UnpackValidateRequestAsync() :: {ex.Message}");
            }
            return null!;
        }

        public async Task<bool> ValidateCodeAsync(ValidateRequestModel validateRequest)
        {
            try
            {
                var entity = await _context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == validateRequest.Email && x.Code == validateRequest.Code);
                if (entity != null)
                {
                    _context.VerificationRequests.Remove(entity);
                    await _context.SaveChangesAsync();
                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : ValidateVerificationService.ValidateCodeAsync() :: {ex.Message}");
            }
            return false;
        }
    }
}

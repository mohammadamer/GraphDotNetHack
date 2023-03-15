
using Microsoft.Azure.Functions.Worker.Http;

namespace MeetingRecordingNotifier.Services
{
    public interface ITokenValidationService
    {
        Task<string> ValidateAuthorizationHeaderAsync(HttpRequestData request);
    }
}

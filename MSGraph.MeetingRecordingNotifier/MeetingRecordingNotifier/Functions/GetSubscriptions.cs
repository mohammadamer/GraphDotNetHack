using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MeetingRecordingNotifier.Graph;
using Microsoft.Extensions.Configuration;
using MeetingRecordingNotifier.Models;

namespace MeetingRecordingNotifier.Functions
{
    public class GetSubscriptions
    {
        private readonly IGraphClientService _graphClientService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public GetSubscriptions(
                IGraphClientService graphClientService,
                IConfiguration config,
                ILoggerFactory loggerFactory)
        {
            _graphClientService = graphClientService;
            _config = config;
            _logger = loggerFactory.CreateLogger<GetSubscriptions>();
        }

        [Function("GetSubscriptions")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                _logger.LogError("Could not create a Graph client for the app");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                var subscriptions = await graphClient.Subscriptions.Request().GetAsync();
                await response.WriteAsJsonAsync(subscriptions);
            }
            //catch (ODataError odataError)
            catch (Exception ex)
            {
                //_logger.LogInformation($"Error while excuting the request, Error Code: {odataError.Error.Code}");
                //_logger.LogInformation($"Error while excuting the request, Error Message: {odataError.Error.Message}");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                throw;
            }

            return response;
        }
    }
}

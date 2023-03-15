using System.Net;
using MeetingRecordingNotifier.Graph;
using MeetingRecordingNotifier.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace MeetingRecordingNotifier.Functions
{
    public class DeleteSubscription
    {
        private readonly IGraphClientService _graphClientService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public DeleteSubscription(
                IGraphClientService graphClientService,
                IConfiguration config,
                ILoggerFactory loggerFactory)
        {
            _graphClientService = graphClientService;
            _config = config;
            _logger = loggerFactory.CreateLogger<DeleteSubscription>();
        }

        [Function("DeleteSubscription")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                _logger.LogError("Could not create a Graph client for the app");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            try
            {
                var payload = await req.ReadFromJsonAsync<SubscriptionPayload>();
                if (payload == null)
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString("Invalid request payload");
                    return response;
                }
                await graphClient.Subscriptions[payload.SubscriptionId].Request().DeleteAsync();
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

using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Notifyer
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("SetSubscription function triggered.");

            // Validate the bearer token
            //var validationResult = await _tokenValidationService.ValidateAuthorizationHeaderAsync(req);
            //if (validationResult == null)
            //{
            //    // If token wasn't returned it isn't valid
            //    return req.CreateResponse(HttpStatusCode.Unauthorized);
            //}

            //https://learn.microsoft.com/en-us/graph/system-messages#call-recording

            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                _logger.LogError("Could not create a Graph client for the app");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // Get the POST body
            var payload = await req.ReadFromJsonAsync<SetSubscriptionPayload>();
            if (payload == null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteString("Invalid request payload");
                return response;
            }

            if (string.Compare(payload.RequestType, "subscribe", true, CultureInfo.InvariantCulture) == 0)
            {
                //if (string.IsNullOrEmpty(payload.UserId))
                //{
                //    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                //    badRequest.WriteString("Required field 'userId' missing in payload.");
                //    return badRequest;
                //}

                // Get ngrok URL if set (for local development)
                var notificationHost = _config["ngrokUrl"] ?? req.Url.Host;

                // Create a new subscription object
                var subscription = new Microsoft.Graph.Subscription
                {
                    ChangeType = "created",
                    NotificationUrl = $"{notificationHost}/api/Notify",
                    Resource = $"communications/callRecords",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddDays(2),
                    ClientState = SetSubscription.ClientState
                };

                _logger.LogInformation($"Creating subscription for user ${payload.UserId}");

                // POST /subscriptions
                var createdSubscription = await graphClient.Subscriptions.PostAsync(subscription);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync<Subscription?>(createdSubscription);
                return response;
            }
            else
            {
                if (string.IsNullOrEmpty(payload.SubscriptionId))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequest.WriteString("Required field 'subscriptionId' missing in payload.");
                    return badRequest;
                }

                _logger.LogInformation($"Deleting subscription with ID {payload.SubscriptionId}");

                // DELETE /subscriptions/subscriptionId
                await graphClient.Subscriptions[payload.SubscriptionId].DeleteAsync();

                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }
    }
}

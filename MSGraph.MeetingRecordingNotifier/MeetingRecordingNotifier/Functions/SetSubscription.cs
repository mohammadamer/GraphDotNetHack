using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeetingRecordingNotifier.Graph;
using MeetingRecordingNotifier.Models;
using Microsoft.Graph;

namespace MeetingRecordingNotifier.Functions
{
    public class SetSubscription
    {
        private readonly IGraphClientService _graphClientService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public SetSubscription(
                IGraphClientService graphClientService,
                IConfiguration config,
                ILoggerFactory loggerFactory)
        {
            _graphClientService = graphClientService;
            _config = config;
            _logger = loggerFactory.CreateLogger<SetSubscription>();
        }

        [Function("SetSubscription")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("SetSubscription function triggered.");

            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                _logger.LogError("Could not create a Graph client for the app");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // Get the POST body
            var payload = await req.ReadFromJsonAsync<SubscriptionPayload>();
            if (payload == null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteString("Invalid request payload");
                return response;
            }

            if (string.Compare(payload.RequestType, "subscribe", true, CultureInfo.InvariantCulture) == 0)
            {
                // Get ngrok URL if set (for local development)
                var notificationHost = _config["ngrokUrl"] ?? req.Url.Host;
                var certificateThumbprint = _config["CertificateThumbprint"];
                var ClientState = _config["ClientState"];

                // Load the X509Certificate and add it to the subscription object
                var certificate = Utilities.X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.CurrentUser, certificateThumbprint);
                var encryptionCertificateId = !string.IsNullOrEmpty(certificate?.FriendlyName) ? certificate.FriendlyName : certificate?.Subject;
                var encryptionCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));


                // Create a new subscription object
                var subscription = new Subscription
                {
                    ChangeType = "created",
                    IncludeResourceData = true,
                    NotificationUrl = $"{notificationHost}/api/Notify",
                    Resource = "/teams/getAllMessages",// all messages and replies across channels.
                    ExpirationDateTime = DateTime.UtcNow.AddMinutes(60),
                    ClientState = ClientState,
                    EncryptionCertificateId = encryptionCertificateId,
                    //EncryptionCertificate = encryptionCertificate,
                };
                subscription.AddPublicEncryptionCertificate(certificate);

                _logger.LogInformation($"Creating subscription...");
                var response = req.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var createdSubscription = await graphClient.Subscriptions.Request().AddAsync(subscription);
                    await response.WriteAsJsonAsync(createdSubscription);
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
            else
            {
                if (string.IsNullOrEmpty(payload.SubscriptionId))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequest.WriteString("Required field 'subscriptionId' missing in payload.");
                    return badRequest;
                }

                _logger.LogInformation($"Deleting subscription with ID {payload.SubscriptionId}");

                // DELETE subscription
                await graphClient.Subscriptions[payload.SubscriptionId].Request().DeleteAsync();

                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }



    }
}

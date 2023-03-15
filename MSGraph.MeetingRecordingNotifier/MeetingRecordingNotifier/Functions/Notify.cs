using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using MeetingRecordingNotifier.Graph;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using MeetingRecordingNotifier.Mail;
using System.Globalization;

namespace MeetingRecordingNotifier.Functions
{
    public class Notify
    {
        private readonly IGraphClientService _graphClientService;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IGraphMail _graphMail;

        public Notify(IGraphClientService graphClientService, ILoggerFactory loggerFactory, IConfiguration config, IGraphMail graphMail)
        {
            _graphClientService = graphClientService;
            _logger = loggerFactory.CreateLogger<Notify>();
            _config = config;
            _graphMail = graphMail;
        }

        [Function("Notify")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Notify function triggered.");
            if (req.FunctionContext.BindingContext.BindingData.TryGetValue("validationToken", out object? validationToken))
            {
                // Return the validation token in a plain text body
                var validationResponse = req.CreateResponse(HttpStatusCode.OK);
                validationResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                validationResponse.WriteString(validationToken?.ToString() ?? string.Empty);
                return validationResponse;
            }

            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                _logger.LogError("Could not create a Graph client for the app");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var tenantId = new Guid[] { new Guid(_config["tenantId"]) };
            var appId = new Guid[] { new Guid(_config["webhookClientId"]) };
            var clientState = _config["ClientState"];

            // Load the X509Certificate and add it to the subscription object
            var certificateThumbprint = _config["CertificateThumbprint"];
            var certificate = Utilities.X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.CurrentUser, certificateThumbprint);

            // Get the notification content
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var collection = graphClient.HttpProvider.Serializer.DeserializeObject<ChangeNotificationCollection>(content);


            // Validate the tokens
            var areTokensValid = await collection.AreTokensValid(tenantId, appId);
            foreach (var notificationItem in collection.Value)
            {
                try
                {
                    if (string.Compare(notificationItem.ClientState, clientState, true, CultureInfo.InvariantCulture) == 0)
                    {
                        var attachedMessage = await notificationItem.EncryptedContent.DecryptAsync<ChatMessage>((id, thumbprint) => Task.FromResult(certificate));
                        if (areTokensValid)
                        {
                            if (attachedMessage.MessageType == ChatMessageType.UnknownFutureValue)
                            {
                                //Send mail
                                var message = await _graphMail.CreateMessageDraft(_config["MessageSubject"], _config["MessageBody"], "user1.onmicrosoft.com", "user2.onmicrosoft.com");
                                await _graphMail.SendMessage(message);
                                _logger.LogInformation($"Message time: {attachedMessage.CreatedDateTime}");
                                _logger.LogInformation($"Message from: {attachedMessage.From?.User?.DisplayName}");
                                _logger.LogInformation($"Message content: {attachedMessage.Body.Content}");
                                
                            }
                        }
                    }

                }
                //catch (ODataError odataError)
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error while excuting the request, Error Code: {ex.InnerException}");
                    //_logger.LogInformation($"Error while excuting the request, Error Code: {odataError.Error.Code}");
                    //_logger.LogInformation($"Error while excuting the request, Error Message: {odataError.Error.Message}");
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                    throw;
                }
            }
            return req.CreateResponse(HttpStatusCode.Accepted);
        }

    }
}

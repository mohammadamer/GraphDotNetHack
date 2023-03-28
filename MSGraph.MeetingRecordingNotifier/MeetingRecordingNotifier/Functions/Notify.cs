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
using MeetingRecordingNotifier.Models;
using Newtonsoft.Json;

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
                                try
                                {

                                    var callRecordingMessageDetail = attachedMessage.AdditionalData;
                                    foreach (var item in callRecordingMessageDetail)
                                    {

                                        if (item.Key == "eventDetail" && item.Value != null)
                                        {
                                            var eventDetails = $"{item.Value}";
                                            var callRecordingdetails = JsonConvert.DeserializeObject<CallRecordingEventMessageDetailModel>(eventDetails);

                                            //Send mail
                                            if (callRecordingdetails?.CallRecordingStatus == "success")
                                            {
                                                var messageBody = String.Format(_config["MessageBody"], callRecordingdetails?.CallRecordingDisplayName, callRecordingdetails?.CallRecordingUrl);
                                                var message = await _graphMail.CreateMessageDraft(_config["MessageSubject"], messageBody, "MiriamG@abc.onmicrosoft.com", "admin@abc.onmicrosoft.com");
                                                await _graphMail.SendMessage(message);
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

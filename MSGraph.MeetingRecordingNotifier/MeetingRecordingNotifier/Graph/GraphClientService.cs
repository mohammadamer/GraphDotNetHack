
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace MeetingRecordingNotifier.Graph
{
    public class GraphClientService : IGraphClientService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private GraphServiceClient? _appGraphClient;

        public GraphClientService(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger<GraphClientService>();
        }

        public GraphServiceClient? GetAppGraphClient()
        {
            if (_appGraphClient == null)
            {
                var tenantId = _config["tenantId"];
                var clientId = _config["webhookClientId"];
                var clientSecret = _config["webhookClientSecret"];

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("Required settings missing: 'tenantId', 'webhookClientId', and 'webhookClientSecret'.");
                    return null;
                }

                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                _appGraphClient = new GraphServiceClient(clientSecretCredential);
            }

            return _appGraphClient;
        }

    }
}

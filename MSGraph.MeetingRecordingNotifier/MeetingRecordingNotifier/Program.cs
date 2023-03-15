using Azure.Core.Serialization;
using MeetingRecordingNotifier.Graph;
using MeetingRecordingNotifier.Mail;
using MeetingRecordingNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(configureOptions: options =>
    {
        options.Serializer = new JsonObjectSerializer(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        );
    })
    .ConfigureAppConfiguration(config => {
        config.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
    })
    .ConfigureServices(services => {
        services.AddSingleton<ITokenValidationService, TokenValidationService>();
        services.AddSingleton<IGraphClientService, GraphClientService>();
        services.AddScoped<IGraphMail, GraphMail>();
    })
    .Build();

host.Run();

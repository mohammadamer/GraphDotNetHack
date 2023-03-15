using MeetingRecordingNotifier.Graph;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Mail
{
    public class GraphMail: IGraphMail
    {
        private readonly IGraphClientService _graphClientService;
        public GraphMail(IGraphClientService graphClientService)
        {
            _graphClientService = graphClientService;   
        }

        public async Task<Message> CreateMessageDraft(string subject, string body, string to, string sender)
        {
            if (to.Length == 0)
            {
                throw new ArgumentException("No recipients specified");
            }

            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                throw new ArgumentException("Could not create a Graph client for the app");
            }

            var delimiter = ";";
            var recipients = to.Split(delimiter);
            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody { ContentType = BodyType.Html, Content = body },
                ToRecipients = recipients
                    .ToList()
                    .Select(address => new Recipient { EmailAddress = new EmailAddress { Address = address } })
                    .ToList(),
                Importance = Importance.Low,
                From = new Recipient { EmailAddress = new EmailAddress { Address = sender } }
            };

            return await graphClient.Users[sender].Messages.Request().AddAsync(message);
        }

        public async Task SendMessage(Message message)
        {
            var graphClient = _graphClientService.GetAppGraphClient();
            if (graphClient == null)
            {
                throw new ArgumentException("Could not create a Graph client for the app");
            }

            var sender = message.From.EmailAddress.Address;
            await graphClient.Users[sender].Messages[message.Id].Send().Request().PostAsync();
        }
    }
}

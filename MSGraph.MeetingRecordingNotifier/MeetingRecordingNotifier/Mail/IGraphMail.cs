using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Mail
{
    public interface IGraphMail
    {
        Task<Message> CreateMessageDraft(string subject, string body, string to, string sender);
        Task SendMessage(Message message);
    }
}

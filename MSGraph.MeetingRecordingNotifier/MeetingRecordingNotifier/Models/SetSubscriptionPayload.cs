using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Models
{
    public class SubscriptionPayload
    {
        // "subscribe" or "unsubscribe"
        public string? RequestType { get; set; }
        // If unsubscribing, the subscription to delete
        public string? SubscriptionId { get; set; }
        // If subscribing, the user ID to subscribe to
        // Can be object ID of user, or userPrincipalName
        public string? UserId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Models
{
    public class Notification
    {
        //[JsonProperty(PropertyName = "value")]
        public List<PublisherNotification> Value { get; set; }

        //[JsonProperty(PropertyName = "validationTokens")]
        public List<string> ValidationTokens { get; set; }
    }
}

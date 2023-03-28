using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Models
{
    public class CallRecordingEventMessageDetailModel
    {
        public string CallId { get; set; }
        public string CallRecordingDisplayName { get; set; }
        public string CallRecordingUrl { get; set; }
        public string CallRecordingDuration { get; set; }
        public string CallRecordingStatus { get; set; }
        //public string meetingOrganizer { get; set; }        
        //public string initiator { get; set; }
    }
}

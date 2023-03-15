using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Models
{
    public class KeyVaultOptions
    {
        public string KeyVaultUri { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CertificateUrl { get; set; }
    }
}

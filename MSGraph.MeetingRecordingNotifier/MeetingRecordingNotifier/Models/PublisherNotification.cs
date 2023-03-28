using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.Models
{
    public class PublisherNotification
    {
        /// <summary>
        /// Gets or sets the Id of the subscription in Teams subscription table.
        /// </summary>
        //[JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets change type.
        /// </summary>
        //[JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets OdataType
        /// Should be #Microsoft.Graph.Message.
        /// </summary>
        //[JsonProperty(PropertyName = "@odata.type")]
        public string OdataType { get; set; }

        /// <summary>
        /// Gets or sets ClientState.
        /// </summary>
        //[JsonProperty(PropertyName = "clientState")]
        public string ClientState { get; set; }

        /// <summary>
        /// Gets or sets Subscription ExpirationDateTime.
        /// </summary>
        //[JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
        public DateTimeOffset? SubscriptionExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets Resource Uri - relative path
        /// Resource Uri.
        /// </summary>
        //[JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets ResourceData, will carry the rich notification data
        /// For rich notificiations data in resourceData property. (Type: Dictionary{string, object}).
        /// </summary>
        //[JsonProperty(PropertyName = "resourceData")]
        #pragma warning disable CA2227 // Collection properties should be read only. Part of Data transfer object class.
        public Dictionary<string, object> ResourceData { get; set; }
        public Dictionary<string, object> AditionalData { get; set; }
        #pragma warning restore CA2227 // Collection properties should be read only. Part of Data transfer object class.

        /// <summary>
        /// Gets or sets the Encrypted Notification Data.
        /// </summary>
        public EncryptedContent EncryptedContent { get; set; }
    }
}

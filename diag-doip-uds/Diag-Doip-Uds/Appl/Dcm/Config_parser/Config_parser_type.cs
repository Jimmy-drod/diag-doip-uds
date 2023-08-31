using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Dcm.Config_parser
{
    // Doip network property type
    public class DoipNetworkType
    {
        [JsonProperty("TcpIpAddress")]
        // local tcp address
        public string Tcp_ip_address { get; set; } = string.Empty;
    };

    public class ConversationPropertyType
    {
        [JsonProperty("p2ClientMax")]
        // store p2 client timeout
        public UInt16 P2_client_max { get; set; }
        [JsonProperty("p2StarClientMax")]
        // store p2 star client timeout
        public UInt16 P2_star_client_max { get; set; }
        [JsonProperty("RxBufferSize")]
        // store receive buffer size
        public UInt16 Rx_buffer_size { get; set; }
        [JsonProperty("SourceAddress")]
        // store source address of client
        public UInt16 Source_address { get; set; }

        [JsonProperty("TargetAddressType")]
        // store Target address type
        public string Target_address_type { get; set; } = string.Empty;
        [JsonProperty("Network")]
        // store the doip network item
        public DoipNetworkType Network { get; set; } = new();
        [JsonProperty("ConversationName")]
        // store the client conversation name
        public string Conversation_name { get; set; } = string.Empty;
    }

    // Properties of a single conversation
    public class ConversationType
    {
        [JsonProperty("NumberOfConversation")]
        // number of conversation
        public byte Num_of_conversation { get; set; }
        [JsonProperty("ConversationProperty")]
        public List<ConversationPropertyType> ConversationPropertys = new();
    };

    // Properties of diag client configuration
    public class DcmClientConfig
    {
        [JsonProperty("UdpIpAddress")]
        // local udp address
        public string Udp_ip_address { get; set; } = string.Empty;
        [JsonProperty("UdpBroadcastAddress")]
        // broadcast address
        public string Udp_broadcast_address { get; set; } = string.Empty;
        [JsonProperty("Conversation")]
        // store all conversations
        public ConversationType Conversation { get; set; } = new();
    };
}

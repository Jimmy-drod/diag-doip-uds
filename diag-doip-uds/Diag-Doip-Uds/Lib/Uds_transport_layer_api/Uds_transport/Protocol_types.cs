using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    namespace Conversion_manager
    {
        // Conversation identification needed by user
        using ConversionHandlerID = Byte;
        public class ConversionIdentifierType
        {
            // Reception buffer
            public UInt32 Rx_buffer_size { get; set; }
            // p2 client time
            public UInt16 P2_client_max { get; set; }
            // p2 star Client time
            public UInt16 P2_star_client_max { get; set; }
            // source address of client
            public UInt16 Source_address { get; set; }
            // self tcp address
            public string Tcp_address { get; set; } = string.Empty;
            // self udp address
            public string Udp_address { get; set; } = string.Empty;
            // Vehicle broadcast address
            public string? Udp_broadcast_address { get; set; } = string.Empty;
            // Port Number
            public UInt16 Port_num { get; set; }
            // conversion handler ID
            public ConversionHandlerID Handler_id { get; set; }
        }
    }
}

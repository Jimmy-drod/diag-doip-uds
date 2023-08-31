using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Common
{
    // ip address type
    using IpAddressType = String;
    public enum rx_socket_type : byte { kBroadcast, kUnicast };
    public class DoipMessage
    {
        // rx type -> broadcast, unicast
        public rx_socket_type Rx_socket { get; set; } = rx_socket_type.kUnicast;
        // remote ip address;
        public IpAddressType Host_ip_address { get; set; } = string.Empty;
        // remote port number
        public UInt16 Host_port_number { get; set; }
        // doip protocol version
        public byte Protocol_version { get; set; }
        // doip protocol inverse version
        public byte Protocol_version_inv { get; set; }
        // doip payload type
        public UInt16 Payload_type { get; set; }
        // doip payload length
        public UInt32 Payload_length { get; set; }
        // doip payload
        public List<byte> Payload { get; set; } = new();
        public DoipMessage() { }
    }
}

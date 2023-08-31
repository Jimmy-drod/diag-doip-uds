using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Utility_support.Socket.Udp
{
    // buffer type
    using buffType = List<byte>;
    // ip address type
    using ipAddressType = String;
    public class UdpMessageType
    {
        // ctor
        public UdpMessageType() { }
        public const byte kDoipUdpResSize = 40;
        // Receive buffer
        public buffType Rx_buffer_ { get; set; } = new();
        // Transmit buffer
        public buffType Tx_buffer_ { get; set; } = new();
        // host ipaddress
        public ipAddressType Host_ip_address_ = String.Empty;
        // host port num
        public UInt16 Host_port_num_ { get; set; }
    }
}

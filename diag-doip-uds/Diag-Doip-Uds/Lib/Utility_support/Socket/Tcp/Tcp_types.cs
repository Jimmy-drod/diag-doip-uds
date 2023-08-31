using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp
{
    // buffer type
    using buffType = List<byte>;
    // ip address type
    using ipAddressType = String;
    public enum tcpSocketState : byte
    {
        // Socket state
        kIdle = 0x00,
        kSocketOnline,
        kSocketOffline,
        kSocketRxMessageReceived,
        kSocketTxMessageSend,
        kSocketTxMessageConf,
        kSocketError
    };
    public enum tcpSocketError : byte
    {
        // state
        kNone = 0x00
    };

    public class TcpMessageType
    {
        public TcpMessageType() { }
        public const byte kDoipheadrSize = 8;

        // socket state
        public tcpSocketState Tcp_socket_state_ { get; set; } = tcpSocketState.kIdle;

        // socket error
        public tcpSocketError Tcp_socket_error_ { get; set; } = tcpSocketError.kNone;

        // Receive buffer
        public buffType RxBuffer_ { get; set; } = new();

        // Transmit buffer
        public buffType TxBuffer_ { get; set; } = new();

        // host ipaddress
        public ipAddressType Host_ip_address_ { get; set; } = string.Empty;

        // host port num
        public UInt16 Host_port_num_ { get; set; }
    }
}

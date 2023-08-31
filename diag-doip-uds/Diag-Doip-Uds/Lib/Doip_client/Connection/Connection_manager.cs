using Diag_Doip_Uds.Lib.Doip_client.Handler;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Doip_connection
{
    using ChannelID = UInt32;
    // type for UDS source and target addresses
    using Address = UInt16;
    // This is the type of Priority
    using Priority = Byte;
    // This is the type of Protocol Kind
    using ProtocolKind = String;
    using static Diag_Doip_Uds.Lib.Common.Common_header;

    public class DoipTcpConnection : Connection
    {
        // Tcp Transport Handler
        private TcpTransportHandler tcp_transport_handler_;
        public DoipTcpConnection(ConversionHandler _conversation, string _tcp_ip_address, UInt16 _port_num)
            : base(1, _conversation)
        {
            tcp_transport_handler_ = new(_tcp_ip_address, _port_num, 1, this);
        }

        public override InitializationResult Initialize()
        {
            tcp_transport_handler_.Initialize();
            return (InitializationResult.kInitializeOk);
        }

        public override void Start() { tcp_transport_handler_.Start(); }

        public override void Stop() { tcp_transport_handler_.Stop(); }

        public override bool IsConnectToHost() { return (tcp_transport_handler_.IsConnectToHost()); }

        public override ConnectionResult ConnectToHost(UdsMessage _message)
        {
            return (tcp_transport_handler_.ConnectToHost(_message));
        }

        public override DisconnectionResult DisconnectFromHost()
        {
            return (tcp_transport_handler_.DisconnectFromHost());
        }

        public override Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr,
                                                                           Address _target_addr,
                                                                 TargetAddressType _type,
                                                                         ChannelID _channel_id, 
                                                                               int _size,
                                                                          Priority _priority,
                                                                      ProtocolKind _protocol_kind, 
                                                                        List<byte> _payloadInfo)
        {
            // Send Indication to conversion
            return (conversation_.IndicateMessage(_source_addr, _target_addr, _type, _channel_id, 
                                                  _size, _priority, _protocol_kind, _payloadInfo));
        }

        public override TransmissionResult Transmit(UdsMessage _message)
        {
            ChannelID channel_id = 0;
            return (tcp_transport_handler_.Transmit(_message, channel_id));
        }

        public override void HandleMessage(UdsMessage _message)
        {
            // send full message to conversion
            conversation_.HandleMessage(_message);
        }
    }

    public class DoipUdpConnection : Connection
    {
        // Udp Transport Handler
        private UdpTransportHandler udp_transport_handler_;

        // ctor
        public DoipUdpConnection(ConversionHandler _conversation, string _udp_ip_address, UInt16 _port_num)
            : base(1, _conversation)
        {
            udp_transport_handler_ = new(_udp_ip_address, _port_num, this);
        }

        // Initialize
        public override InitializationResult Initialize()
        {
            udp_transport_handler_.Initialize();
            return InitializationResult.kInitializeOk;
        }

        // Start the connection
        public override void Start() { udp_transport_handler_.Start(); }

        // Stop the connection
        public override void Stop() { udp_transport_handler_.Stop(); }

        // Check if already connected to host
        public override bool IsConnectToHost() { return false; }

        // Connect to host using the ip address
        public override ConnectionResult ConnectToHost(UdsMessage _message)
        {
            return (ConnectionResult.kConnectionFailed);
        }

        // Disconnect from Host Server
        public override DisconnectionResult DisconnectFromHost()
        {
            return (DisconnectionResult.kDisconnectionFailed);
        }

        // Indicate message Diagnostic message reception over TCP to user
        public override Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                           Address _target_addr,
                                                                 TargetAddressType _type, 
                                                                         ChannelID _channel_id, 
                                                                               int _size,
                                                                          Priority _priority, 
                                                                      ProtocolKind _protocol_kind,
                                                                        List<byte> _payloadInfo)
        {
            // Send Indication to conversion
            return (conversation_.IndicateMessage(_source_addr, _target_addr, _type, _channel_id, 
                                                  _size, _priority, _protocol_kind, _payloadInfo));
        }

        // Transmit tcp
        public override TransmissionResult Transmit(UdsMessage _message)
        {
            ChannelID channel_id = 0;
            return (udp_transport_handler_.Transmit(_message, channel_id));
        }

        // Hands over a valid message to conversion
        public override void HandleMessage(UdsMessage _message)
        {
            // send full message to conversion
            conversation_.HandleMessage(_message);
        }
    }

    public class DoipConnectionManager
    {
        // ctor
        public DoipConnectionManager() { }

        // Function to create new connection to handle doip tcp request and response
        public DoipTcpConnection FindOrCreateTcpConnection(ConversionHandler _conversation, 
                                                                      string _tcp_ip_address,
                                                                      UInt16 _port_num)
        {
            return (new DoipTcpConnection(_conversation, _tcp_ip_address, _port_num));
        }

        // Function to create new connection to handle doip udp request and response
        public DoipUdpConnection FindOrCreateUdpConnection(ConversionHandler _conversation, 
                                                                      string _udp_ip_address,
                                                                      UInt16 _port_num)
        {
            return (new DoipUdpConnection(_conversation, _udp_ip_address, _port_num));
        }
    }
}

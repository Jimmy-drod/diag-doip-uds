using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Doip_client.Doip_connection;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Handler
{
    using ChannelID = UInt32;
    // type for UDS source and target addresses
    using Address = UInt16;
    // This is the type of Priority
    using Priority = Byte;
    // This is the type of Protocol Kind
    using ProtocolKind = String;
    using static Diag_Doip_Uds.Lib.Common.Common_header;

    public class TcpTransportHandler
    {
        // reference to doip connection
        private DoipTcpConnection doip_connection_;
        // Tcp channel responsible for transmitting and reception of TCP messages
        private TcpChannel tcp_channel_;

        // ctor
        public TcpTransportHandler(string _local_ip_address, UInt16 _port_num, byte _total_tcp_channel_req,
                                    DoipTcpConnection _doip_connection)
        {
            doip_connection_ = _doip_connection;
            tcp_channel_ = new TcpChannel(_local_ip_address, this);
        }

        // Initialize
        public InitializationResult Initialize()
        {
            return (tcp_channel_.Initialize());
        }

        // Start
        public void Start() { tcp_channel_.Start(); }

        // Stop
        public void Stop() { tcp_channel_.Stop(); }

        // Check if already connected to host
        public bool IsConnectToHost() { return (tcp_channel_.IsConnectToHost()); }

        // Connect to remote Host
        public ConnectionResult ConnectToHost(UdsMessage _message)
        {
            return (tcp_channel_.ConnectToHost(_message));
        }

        // Disconnect from remote Host
        public DisconnectionResult DisconnectFromHost()
        {
            return (tcp_channel_.DisconnectFromHost());
        }

        // Transmit
        public TransmissionResult Transmit(UdsMessage _message,ChannelID _channel_id)
        {
            // find the corresponding channel
            // Trigger transmit
            return (tcp_channel_.Transmit(_message));
        }

        // Indicate message Diagnostic message reception over TCP to user
        public Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                  Address _target_addr,
                                                        TargetAddressType _type, 
                                                                ChannelID _channel_id, 
                                                                      int _size,
                                                                 Priority _priority, 
                                                             ProtocolKind _protocol_kind, 
                                                               List<byte> _payloadInfo)
        {
            return (doip_connection_.IndicateMessage(_source_addr, _target_addr, _type, _channel_id,
                                                     _size, _priority, _protocol_kind, _payloadInfo));
        }

        // Hands over a valid received Uds message (currently this is only a request type) from transport
        // layer to session layer
        public void HandleMessage(UdsMessage _message)
        {
            doip_connection_.HandleMessage(_message);
        }
    }
}

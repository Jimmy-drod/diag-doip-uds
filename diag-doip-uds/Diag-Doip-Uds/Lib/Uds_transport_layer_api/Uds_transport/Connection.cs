using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    // type alias for connection id
    using ConnectionId = Byte;
    using Address = UInt16;
    // This is the type of Channel Id
    using ChannelID = UInt32;
    // This is the type of Priority
    using Priority = Byte;
    // This is the type of Protocol Kind
    using ProtocolKind = String;
    using static Diag_Doip_Uds.Lib.Common.Common_header;

    public abstract class Connection
    {
        // store the connection id
        private ConnectionId connection_id_;
        // Store the conversion
        protected ConversionHandler conversation_;

        // ctor
        public Connection(ConnectionId _connection_id, ConversionHandler _conversation)
        {
            conversation_ = _conversation;
            connection_id_ = _connection_id;
        }

        // Initialize
        public abstract InitializationResult Initialize();

        // Start the connection
        public abstract void Start();

        // Stop the connection
        public abstract void Stop();

        // Check if already connected to host
        public abstract bool IsConnectToHost();

        // Connect to Host Server
        public abstract ConnectionResult ConnectToHost(UdsMessage message);

        // Disconnect from Host Server
        public abstract DisconnectionResult DisconnectFromHost();

        // Indicate message Diagnostic message reception over TCP to user
        public abstract Pair<IndicationResult, UdsMessage> IndicateMessage( Address _source_addr, 
                                                                            Address _target_addr, 
                                                                  TargetAddressType _type,
                                                                          ChannelID _channel_id, 
                                                                                int _size, 
                                                                           Priority _priority, 
                                                                       ProtocolKind _protocol_kind,
                                                                         List<byte> _payloadInfo);

        // Transmit tcp/udp data
        public abstract TransmissionResult Transmit(UdsMessage message);

        // Hands over a valid message to conversion
        public abstract void HandleMessage(UdsMessage message);
    }
}

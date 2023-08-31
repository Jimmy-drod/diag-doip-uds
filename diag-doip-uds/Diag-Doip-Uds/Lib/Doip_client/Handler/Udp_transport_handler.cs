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

    public class UdpTransportHandler
    {
        // reference to doip Connection
        private DoipUdpConnection doip_connection_;
        // Udp channel responsible for transmitting and reception of UDP messages
        private UdpChannel udp_channel_;

        // ctor
        public UdpTransportHandler(string _localIpaddress, UInt16 _portNum, DoipUdpConnection _doipConnection)
        {
            doip_connection_ = _doipConnection;
            udp_channel_ = new(_localIpaddress, _portNum, this);
        }

        // Initialize
        public InitializationResult Initialize()
        {
            return (udp_channel_.Initialize());
        }

        // Start
        public void Start()
        {
            udp_channel_.Start();
        }

        // Stop
        public void Stop()
        {
            udp_channel_.Stop();
        }

        // Transmit
        public TransmissionResult Transmit(UdsMessage _message, ChannelID _channel_id)
        {
            return (udp_channel_.Transmit(_message));
        }

        // Indicate message Diagnostic message reception over UDP to user
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

        // Hands over a valid received UDP message (currently this is only a request type) from transport
        // layer to session layer
        public void HandleMessage(UdsMessage _message)
        {
            doip_connection_.HandleMessage(_message);
        }
    }
}

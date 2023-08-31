using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    using ConversionHandlerID = Byte;
    using Address = UInt16;
    // This is the type of Channel Id
    using ChannelID = UInt32;
    // This is the type of Priority
    using Priority = Byte;
    // This is the type of Protocol Kind
    using ProtocolKind = String;
    using static Diag_Doip_Uds.Lib.Common.Common_header;

    public abstract class ConversionHandler
    {
        protected ConversionHandlerID handler_id_;

        // ctor
        public ConversionHandler(ConversionHandlerID _handler_id) { handler_id_ = _handler_id; }

        // Indicate message Diagnostic message reception over TCP/UDP to user
        public abstract Pair<IndicationResult, UdsMessage> IndicateMessage( Address _source_addr, 
                                                                            Address _target_addr, 
                                                                  TargetAddressType _type,
                                                                          ChannelID _channel_id,
                                                                                int _size, 
                                                                           Priority _priority, 
                                                                       ProtocolKind _protocol_kind,
                                                                         List<byte> _payloadInfo);

        // Hands over a valid message to conversion
        public abstract void HandleMessage(UdsMessage message);
    }
}

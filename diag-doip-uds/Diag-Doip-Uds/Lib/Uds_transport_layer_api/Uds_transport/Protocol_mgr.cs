using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    // This is the type of Channel Id
    using ChannelID = UInt32;
    // UdsTransportProtocolHandler are flexible "plugins", which need an identification
    using UdsTransportProtocolHandlerID = Byte;
    // Type of tuple to pack UdsTransportProtocolHandlerID and ChannelID together, to form a global
    // unique (among all used UdsTransportProtocolHandlers within DM) identifier of a UdsTransport
    // Protocol channel.
    //using GlobalChannelIdentifier = Tuple<UdsTransportProtocolHandlerID, ChannelID>;
    // Result for Indication of message received
    public enum IndicationResult : byte
    {
        kIndicationOk = 0,
        kIndicationOccupied,
        kIndicationOverflow,
        kIndicationUnknownTargetAddress,
        kIndicationPending,
        kIndicationNOk
    };
    // Result for transmission of message sent
    public enum TransmissionResult : byte
    {
        kTransmitOk = 0,
        kTransmitFailed,
        kNoTransmitAckReceived,
        kNegTransmitAckReceived,
        kBusyProcessing
    };
    // Result for connection to remote endpoint
    public enum ConnectionResult : byte { kConnectionOk = 0, kConnectionFailed, kConnectionTimeout };
    // Result for disconnection to remote endpoint
    public enum DisconnectionResult : byte { kDisconnectionOk = 0, kDisconnectionFailed };
    public abstract class UdsTransportProtocolMgr
    {
        //ctor
        public UdsTransportProtocolMgr() { }

        // initialize all the transport protocol handler
        public abstract void Startup();

        // start all the transport protocol handler
        public abstract void Run();

        // terminate all the transport protocol handler
        public abstract void Shutdown();
    }
}

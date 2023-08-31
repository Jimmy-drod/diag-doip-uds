using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    using UdsTransportProtocolHandlerID = Byte;
    public enum InitializationResult : byte { kInitializeOk = 0, kInitializeFailed = 1 };
    public abstract class UdsTransportProtocolHandler
    {
        protected UdsTransportProtocolHandlerID handler_id_;
        private UdsTransportProtocolMgr? transport_protocol_mgr_;

        //ctor
        public UdsTransportProtocolHandler(UdsTransportProtocolHandlerID _handler_id,
                                           UdsTransportProtocolMgr _transport_protocol_mgr)
        {
            handler_id_ = _handler_id;
            transport_protocol_mgr_ = _transport_protocol_mgr;
        }

        // Return the UdsTransportProtocolHandlerID
        public virtual UdsTransportProtocolHandlerID GetHandlerID() { return handler_id_; }

        // Initialize
        public abstract InitializationResult Initialize();

        // Start processing the implemented Uds Transport Protocol
        public abstract void Start();

        // Method to indicate that this UdsTransportProtocolHandler should terminate
        public abstract void Stop();

        // Get or Create a Tcp Connection
        public abstract Connection FindOrCreateTcpConnection(ConversionHandler _conversion_handler, 
                                                                        string _tcpIpaddress,
                                                                        UInt16 _portNum);

        // Get or Create an Udp Connection
        public abstract Connection FindOrCreateUdpConnection(ConversionHandler _conversion_handler, 
                                                                        string _udpIpaddress,
                                                                        UInt16 _portNum);
    }
}

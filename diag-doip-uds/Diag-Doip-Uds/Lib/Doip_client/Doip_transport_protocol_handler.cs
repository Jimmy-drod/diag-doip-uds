using Diag_Doip_Uds.Lib.Doip_client.Doip_connection;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client
{
    // UdsTransportProtocolHandler are flexible "plugins", which need an identification
    using UdsTransportProtocolHandlerID = Byte;
    /*
    @ Class Name        : DoipTransportProtocolHandler
    @ Class Description : This class must be instantiated by user for using the DoIP functionalities.
                          This will inherit uds transport protocol handler
    */
    public sealed class DoipTransportProtocolHandler : UdsTransportProtocolHandler
    {
        // store handle id
        private UdsTransportProtocolHandlerID handle_id_e;
        // store the transport protocol manager
        private UdsTransportProtocolMgr transport_protocol_mgr_;
        // Create Doip Connection Manager
        private DoipConnectionManager doip_connection_mgr_ptr;

        //ctor
        public DoipTransportProtocolHandler(UdsTransportProtocolHandlerID _handler_id,
                    UdsTransportProtocolMgr _transport_protocol_mgr) : base(_handler_id, _transport_protocol_mgr)
        {
            handle_id_e = _handler_id;
            transport_protocol_mgr_ = _transport_protocol_mgr;
            doip_connection_mgr_ptr = new();
        }

        // Return the UdsTransportProtocolHandlerID, which was given to the implementation during construction (ctor call).
        public override UdsTransportProtocolHandlerID GetHandlerID()
        {
            return handle_id_e;
        }

        // Initializes handler
        public override InitializationResult Initialize()
        {
            return InitializationResult.kInitializeOk;
        }

        // Start processing the implemented Uds Transport Protocol
        public override void Start()
        {
        }

        // Method to indicate that this UdsTransportProtocolHandler should terminate
        public override void Stop()
        {
        }

        // Get or Create Tcp connection
        public override Connection FindOrCreateTcpConnection(ConversionHandler _conversation, 
                                                                        string _tcp_ip_address,
                                                                        UInt16 _port_num)
        {
            return doip_connection_mgr_ptr.FindOrCreateTcpConnection(_conversation,
                                                                     _tcp_ip_address,
                                                                     _port_num);
        }

        // Get or Create Udp connection
        public override Connection FindOrCreateUdpConnection(ConversionHandler _conversation,
                                                                        string _udp_ip_address,
                                                                        UInt16 _port_num)
        {
            return doip_connection_mgr_ptr.FindOrCreateUdpConnection(_conversation, 
                                                                     _udp_ip_address, 
                                                                     _port_num);
        }
    };
}

using Diag_Doip_Uds.Lib.Doip_client;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Dcm.Connection
{
    // UdsTransportProtocolHandler are flexible "plugins", which need an identification
    using UdsTransportProtocolHandlerID = Byte;
    /*
    @ Class Name        : UdsTransportProtocolManager
    @ Class Description : This class must be instantiated by user for using the transport protocol functionalities.
                          This will inherit uds transport protocol handler
    */
    public sealed class UdsTransportProtocolManager : UdsTransportProtocolMgr
    {
        // store doip transport handler
        public UdsTransportProtocolHandler doip_transport_handler;

        // handler id count
        public UdsTransportProtocolHandlerID handler_id_count = 0;
        //ctor
        public UdsTransportProtocolManager()
        {
            doip_transport_handler = new DoipTransportProtocolHandler(handler_id_count, this);
        }

        // initialize all the transport protocol handler
        public override void Startup()
        {
            //Initialize all the handlers in box
            doip_transport_handler.Initialize();
        }

        // start all the transport protocol handler
        public override void Run()
        {
            //Start all the handlers in box
            doip_transport_handler.Start();
        }

        // terminate all the transport protocol handler
        public override void Shutdown()
        {
            //Stop all the handlers in box
            doip_transport_handler.Stop();
        }
    };
}

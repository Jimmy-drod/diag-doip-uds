using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Include
{
    using static Diag_Doip_Uds.Lib.Common.Common_header;

    /**
* @brief         Type alias of ip address type
*/
    using IpAddress = String;

    /**
    * @brief      Definitions of Connection results
    */
    public enum ConnectResult : byte
    {
        kConnectSuccess = 0, /* Successfully connected to Diagnostic Server */
        kConnectFailed = 1,  /* Connection failure to Diagnostic Server, check logs for more failure information */
        kConnectTimeout = 2  /* No Connection response received from Diagnostic Server */
    };

    /**
    * @brief      Definitions of Disconnection results
    */
    public enum DisconnectResult : byte
    {
        kDisconnectSuccess = 0,  /* Successfully disconnected from Diagnostic Server */
        kDisconnectFailed = 1,   /* Disconnection failure with Diagnostic Server, check logs for more information */
        kAlreadyDisconnected = 2 /* Not connected to Diagnostic Server */
    };

    /**
        * @brief      Definitions of Diagnostics Request Response results
        */
    public enum DiagResult : byte
    {
        kDiagSuccess = 0,           /* Diagnostic request message transmitted and response received successfully */
        kDiagGenericFailure = 1,    /* Generic Diagnostic Error, see logs for more information */
        kDiagRequestSendFailed = 2, /* Diagnostic request message transmission failure */
        kDiagAckTimeout = 3,        /* No diagnostic acknowledgement response received within 2 seconds */
        kDiagNegAckReceived = 4,    /* Diagnostic negative acknowledgement received */
        kDiagResponseTimeout = 5,   /* No diagnostic response message received within P2/P2Star time */
        kDiagInvalidParameter = 6,  /* Passed parameter value is not valid */
        kDiagBusyProcessing = 7     /* Conversation is already busy processing previous request */
    };


    /**
    * @brief       Conversation class to establish connection with a Diagnostic Server
    * @details     Conversation class only support DoIP communication protocol for connecting to remote ECU
    */
    public interface IDiagClientConversation
    {
        /**
        * @brief      Function to startup the Diagnostic Client Conversation
        * @details    Must be called once and before using any other functions of DiagClientConversation
        * @remarks    Implemented requirements:
        *             DiagClientLib-Conversation-StartUp
        */
        public void Startup();

        /**
        * @brief      Function to shutdown the Diagnostic Client Conversation
        * @details    Must be called during shutdown phase, no further processing of any
        *             function will be allowed after this call
        * @remarks    Implemented requirements:
        *             DiagClientLib-Conversation-Shutdown
        */
        public void Shutdown();

        /**
        * @brief       Function to connect to Diagnostic Server.
        * @param[in]   target_address
        *              Logical address of the Remote server
        * @param[in]   host_ip_addr
        *              IP address of the Remote server
        * @return      ConnectResult
        *              Connection result returned
        * @remarks     Implemented requirements:
        *              DiagClientLib-Conversation-Connect
        */
        public ConnectResult ConnectToDiagServer(UInt16 _target_address, IpAddress _host_ip_addr);

        /**
        * @brief       Function to disconnect from Diagnostic Server
        * @return      DisconnectResult
        *              Disconnection result returned
        * @remarks     Implemented requirements:
        *              DiagClientLib-Conversation-Disconnect
        */
        public DisconnectResult DisconnectFromDiagServer();

        /**
        * @brief       Function to send Diagnostic Request and get Diagnostic Response
        * @param[in]   message
        *              Diagnostic request message wrapped in a unique pointer
        * @return      DiagResult
        *              Result returned
        * @return      uds_message::UdsResponseMessagePtr
        *              Diagnostic Response message received, null_ptr in case of error
        * @remarks     Implemented requirements:
        *              DiagClientLib-Conversation-DiagRequestResponse
        */
        public Pair<DiagResult, IUdsMessage> SendDiagnosticRequest(IUdsMessage _message);
    };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Appl.Include
{
    /**
    * @brief  Definitions of Vehicle Identification response result
    */
    public enum VehicleResponseResult : byte
    {
        kTransmitFailed = 0,     /*  Failure on Transmissions */
        kInvalidParameters = 1,  /* Invalid Parameter passed */
        kNoResponseReceived = 2, /* No vehicle identification response received */
        kStatusOk = 3            /* Vehicle identification response received success */
    };

    /**
    * @brief    Class to manage Diagnostic Client
    */
    public interface IDiagClient
    {
        /**
        * @brief        Function to initialize the already created instance of DiagClient
        * @details      Must be called once and before using any other functions of DiagClient
        * @remarks      Implemented requirements:
        *               DiagClientLib-Initialization
        */
        public void Initialize();

        /**
        * @brief        Function to de-initialize the already initialized instance of DiagClient
        * @details      Must be called during shutdown phase, no further processing of any
        *               function will be allowed after this call
        * @remarks      Implemented requirements:
        *               DiagClientLib-DeInitialization
        */
        public void DeInitialize();

        /**
        * @brief       Function to send vehicle identification request and get the Diagnostic Server list
        * @param[in]   vehicle_info_request
        *              Vehicle information sent along with request
        * @return      std::pair<VehicleResponseResult, diag::client::vehicle_info::VehicleInfoMessageResponsePtr>
        *              Pair consisting the result & response, contains valid response when result = kStatusOk
        * @remarks     Implemented requirements:
        *              DiagClientLib-VehicleDiscovery
        */
        public Pair<VehicleResponseResult, IVehicleInfoMessage>
        SendVehicleIdentificationRequest(VehicleAddrInfoRequest _vehicle_info_request);

        /**
        * @brief       Function to get required diag client conversation object based on conversation name
        * @param[in]   conversation_name
        *              Name of conversation configured as json parameter "ConversationName"
        * @return      DiagClientConversation&
        *              Reference to diag client conversation
        * @remarks     Implemented requirements:
        *              DiagClientLib-MultipleTester-Connection, DiagClientLib-Conversation-Construction
        */
        public IDiagClientConversation GetDiagnosticClientConversation(string _conversation_name);

    };
}

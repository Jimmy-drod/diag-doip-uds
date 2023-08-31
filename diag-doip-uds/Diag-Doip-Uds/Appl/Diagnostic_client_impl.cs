using Diag_Doip_Uds.Appl.Common;
using Diag_Doip_Uds.Appl.Dcm;
using Diag_Doip_Uds.Appl.Include;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Appl
{
    public sealed class DiagClientImpl : IDiagClient
    {
        // dcm client instance
        private DiagnosticManager dcm_instance_ptr;

        // thread to hold dcm client instance
        private Thread? dcm_thread_;

        // ctor
        public DiagClientImpl(string _dm_client_config)
        {
            dcm_instance_ptr = new DCMClient(_dm_client_config);
            dcm_thread_ = null;
        }

        // Initialize
        public void Initialize()
        {
            dcm_thread_ = new Thread(() =>
            {
                dcm_instance_ptr.Main();
            });
            dcm_thread_.Name = "DCMClient_Main";
            dcm_thread_.Start();
        }

        // De-Initialize
        public void DeInitialize()
        {
            dcm_instance_ptr.SignalShutdown();
            if(dcm_thread_ != null)
            {
                dcm_thread_.Join();
            }
        }

        // Get Required Conversation based on Conversation Name
        public IDiagClientConversation GetDiagnosticClientConversation(string _conversation_name)
        {
            return dcm_instance_ptr.GetDiagnosticClientConversation(_conversation_name);
        }

        // Send Vehicle Identification Request and get response
        public Pair<VehicleResponseResult, IVehicleInfoMessage>
        SendVehicleIdentificationRequest(VehicleAddrInfoRequest _vehicle_info_request)
        {
            return dcm_instance_ptr.SendVehicleIdentificationRequest(_vehicle_info_request);
        }
    };
}

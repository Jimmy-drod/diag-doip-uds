using Diag_Doip_Uds.Appl.Common;
using Diag_Doip_Uds.Appl.Dcm.Config_parser;
using Diag_Doip_Uds.Appl.Dcm.Connection;
using Diag_Doip_Uds.Appl.Dcm.Conversation;
using Diag_Doip_Uds.Appl.Include;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Appl.Dcm
{
    /*
    @ Class Name        : DCM Client
    @ Class Description : Class to create Diagnostic Manager Client functionality
    */
    public sealed class DCMClient : DiagnosticManager
    {
        // uds transport protocol Manager
        private UdsTransportProtocolManager uds_transport_protocol_mgr;

        // conversation manager
        private ConversationManager conversation_mgr;

        // map to store conversation pointer along with conversation name
        private Dictionary<string, IDiagClientConversation> diag_client_conversation_map;

        // store the diag client conversation for vehicle discovery
        private VdConversation diag_client_vehicle_discovery_conversation;

        //ctor
        public DCMClient(string _config_file) : base()
        {
            uds_transport_protocol_mgr = new UdsTransportProtocolManager();
            conversation_mgr = new ConversationManager(GetDcmClientConfig(_config_file), uds_transport_protocol_mgr);
            diag_client_vehicle_discovery_conversation = 
                conversation_mgr.GetDiagnosticClientVehicleDiscoveryConversation("VehicleDiscovery");
            diag_client_conversation_map = new();
        }

        // Initialize
        public override void Initialize()
        {
            // start Vehicle Discovery
            diag_client_vehicle_discovery_conversation?.Startup();
            // start Conversation Manager
            conversation_mgr.Startup();
            // start all the udsTransportProtocol Layer
            uds_transport_protocol_mgr.Startup();
        }

        // Run
        public override void Run()
        {
            // run udsTransportProtocol layer
            uds_transport_protocol_mgr.Run();
        }

        // Shutdown
        public override void Shutdown()
        {
            // shutdown Vehicle Discovery
            diag_client_vehicle_discovery_conversation?.Shutdown();
            // shutdown Conversation Manager
            conversation_mgr.Shutdown();
            // shutdown udsTransportProtocol layer
            uds_transport_protocol_mgr.Shutdown();
        }

        // Function to get the diagnostic client conversation
        public override IDiagClientConversation
        GetDiagnosticClientConversation(string _conversation_name)
        {
            string diag_client_conversation_name = _conversation_name;
            IDiagClientConversation? ret_conversation = null;
            IDiagClientConversation? conversation =
                conversation_mgr.GetDiagnosticClientConversation(diag_client_conversation_name);
            if (conversation != null)
            {
                diag_client_conversation_map.Add( _conversation_name, conversation);
                ret_conversation = diag_client_conversation_map[diag_client_conversation_name];
            }
            return ret_conversation;
        }

        // function to read from property tree to config structure
        private static DcmClientConfig? GetDcmClientConfig(string _config_file)
        {
            DcmClientConfig? config = null;
            if (!File.Exists(_config_file))
            {
                return config;
            }

            try
            {
                string str = File.ReadAllText(_config_file);
                if (str.Length == 0)
                {
                    return config;
                }
                else
                {
                    config = JsonConvert.DeserializeObject<DcmClientConfig>(str, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return config;
        }

        // Send Vehicle Identification Request and get response
        public override Pair<VehicleResponseResult, IVehicleInfoMessage>
        SendVehicleIdentificationRequest(VehicleAddrInfoRequest _vehicle_info_request)
        {
            return diag_client_vehicle_discovery_conversation.SendVehicleIdentificationRequest(_vehicle_info_request);
        }
    };
}

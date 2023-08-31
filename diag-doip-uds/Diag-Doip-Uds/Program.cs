using Diag_Doip_Uds.Appl.Dcm.Service;
using Diag_Doip_Uds.Appl.Include;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if true
            /*
            * Main Entry point of diag client example 
            * Example to find available ECU in a network through vehicle discovery and connecting to it.
            */

            // Create the Diagnostic client and pass the config for creating internal properties
            IDiagClient diag_client = DiagClientHelper.CreateDiagnosticClient("diag_client_config.json");

            // Initialize the Diagnostic Client library
            diag_client.Initialize();

            // Get conversation for tester one by providing the conversation name configured
            // in diag_client_config file passed while creating the diag client
            IDiagClientConversation diag_client_conversation = diag_client.GetDiagnosticClientConversation("DiagTesterOne");

            // Start the conversation for tester one
            diag_client_conversation.Startup();

            // Create a vehicle info request for finding ECU with matching VIN - ABCDEFGH123456789
            VehicleAddrInfoRequest  vehicle_info_request = new(){ Preselection_mode = 1, Preselection_value = "ABCDEFGH123456789" };
            // Send Vehicle Identification request and get the response with available ECU information
            Pair<VehicleResponseResult, IVehicleInfoMessage> vehicle_response_result = 
                diag_client.SendVehicleIdentificationRequest(vehicle_info_request);


            // Valid vehicle discovery response must be received before connecting to ECU
            if (vehicle_response_result.First == VehicleResponseResult.kStatusOk 
                && (vehicle_response_result.Second != null))
            {
                // Get the list of all vehicle available with matching VIN
                List<VehicleAddrInfoResponse> vehicle_response_collection =
                    vehicle_response_result.Second.GetVehicleList();

                // Create an uds message using first vehicle available in the list of response collection
                string remote_ecu_ip_address = vehicle_response_collection[0].Ip_address; // Remote ECU ip address
                UInt16 remote_ecu_server_logical_address = vehicle_response_collection[0].Logical_address; // Remote ECU logical address

                IUdsMessage uds_message = new UdsMessageImpl(remote_ecu_ip_address, new(){ 0x10, 0x01});

                // Connect Tester One to remote ECU
                ConnectResult connect_result =
                    diag_client_conversation.ConnectToDiagServer(remote_ecu_server_logical_address,
                                                                 uds_message.GetHostIpAddress());

                if (connect_result == ConnectResult.kConnectSuccess)
                {
                    // Use Tester One to send the diagnostic message to remote ECU
                    Pair<DiagResult, IUdsMessage> diag_response =
                        diag_client_conversation.SendDiagnosticRequest(uds_message);

                    if (diag_response.First == DiagResult.kDiagSuccess)
                    {
                        Console.WriteLine($"diag_client_conversation Total size: : {diag_response.Second.GetPayload().Count}");
                        // Print the payload
                        Console.WriteLine($"diag_client_conversation1 byte : {BytesToHexStr(diag_response.Second.GetPayload().ToArray())}");
                    }

                    // Disconnect Tester One from ECU1 with remote ip address "172.16.25.128"
                    diag_client_conversation.DisconnectFromDiagServer();
                }

            }

            // shutdown the conversation
            diag_client_conversation.Shutdown();

            // important to release all the resources by calling de-initialize
            diag_client.DeInitialize();
#else
            /*
            * Main Entry point of diag client example 
            * Example to connect to multiple ECU by creating multiple diagnostic tester instance.
            */

            // Create the Diagnostic client and pass the config for creating internal properties
            IDiagClient diag_client = DiagClientHelper.CreateDiagnosticClient("diag_client_config.json");

            // Initialize the Diagnostic Client library
            diag_client.Initialize();

            // Get conversation for tester one by providing the conversation name configured
            // in diag_client_config file passed while creating the diag client
            IDiagClientConversation diag_client_conversation1 = diag_client.GetDiagnosticClientConversation("DiagTesterOne");

            // Start the conversation for tester one
            diag_client_conversation1.Startup();

            // Get conversation for tester two by providing the conversation name configured
            // in diag_client_config file passed while creating the diag client
            IDiagClientConversation diag_client_conversation2 = diag_client.GetDiagnosticClientConversation("DiagTesterTwo");

            // Start the conversation for tester two
            diag_client_conversation2.Startup();

            // Create an uds payload 10 01 ( Default Session )
            List<byte> payload_1 = new(){ 0x10, 0x01};

            // Create an uds payload 3E 00( Tester Present )
            List<byte> payload_2 = new(){ 0x3E, 0x00};

            {
                // Create an uds message with payload for ECU1 with remote ip address 10.167.226.194
                IUdsMessage uds_message_1 = new UdsMessageImpl("10.167.226.194", payload_1);

                // Create an uds message with payload for ECU2 with remote ip address 10.167.218.217
                IUdsMessage uds_message_2 = new UdsMessageImpl("10.167.218.217", payload_2);

                // Connect Tester One to ECU1 with target address "0x1234" and remote ip address "10.167.226.194"
                diag_client_conversation1.ConnectToDiagServer(0x1234, uds_message_1.GetHostIpAddress());

                // Connect Tester Two to ECU2 with target address "0x1235" and remote ip address "10.167.218.217"
                diag_client_conversation2.ConnectToDiagServer(0x1235, uds_message_2.GetHostIpAddress());

                // Use Tester One to send the diagnostic message to ECU1
                Pair<DiagResult, IUdsMessage> ret_val_1 = diag_client_conversation1.SendDiagnosticRequest(uds_message_1);

                if (ret_val_1.First == DiagResult.kDiagSuccess)
                {
                    Console.WriteLine($"diag_client_conversation1 Total size : {ret_val_1.Second.GetPayload().Count}");
                    // Print the payload
                    Console.WriteLine($"diag_client_conversation1 byte : {BytesToHexStr(ret_val_1.Second.GetPayload().ToArray())}");
                }

                // Use Tester Two to send the diagnostic message to ECU2
                Pair<DiagResult, IUdsMessage> ret_val_2 = diag_client_conversation2.SendDiagnosticRequest(uds_message_2);

                if (ret_val_2.First == DiagResult.kDiagSuccess)
                {
                    Console.WriteLine($"diag_client_conversation2 Total size : {ret_val_2.Second.GetPayload().Count}");
                    // Print the payload
                    Console.WriteLine($"diag_client_conversation2 byte : {BytesToHexStr(ret_val_2.Second.GetPayload().ToArray())}");
                }

                // Disconnect Tester One from ECU1 with remote ip address "10.167.226.194"
                diag_client_conversation1.DisconnectFromDiagServer();
                // Disconnect Tester Two from ECU2 with remote ip address "10.167.218.217"
                diag_client_conversation2.DisconnectFromDiagServer();
            }

            // shutdown all the conversation
            diag_client_conversation1.Shutdown();
            diag_client_conversation2.Shutdown();

            // important to release all the resources by calling de-initialize
            diag_client.DeInitialize();
#endif
        }

        private static string BytesToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}
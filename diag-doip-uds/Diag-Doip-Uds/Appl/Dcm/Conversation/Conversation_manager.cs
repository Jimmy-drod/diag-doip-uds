using Diag_Doip_Uds.Appl.Dcm.Config_parser;
using Diag_Doip_Uds.Appl.Dcm.Connection;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport.Conversion_manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Dcm.Conversation
{
    /*
    @ Class Name        : ConversationManager
    @ Class Description : Class to manage all the conversion created from usr request
    */
    public class ConversationManager
    {
        // store uds transport manager
        private UdsTransportProtocolManager uds_transport_mgr_;

        // store the conversion name with conversation configurations
        private Dictionary<string, ConversionIdentifierType> conversation_config_ = new();

        // store the vehicle discovery with conversation configuration
        private Dictionary<string, ConversionIdentifierType> vd_conversation_config_ = new();

        // ctor
        public ConversationManager(DcmClientConfig? _config, UdsTransportProtocolManager _uds_transport_mgr)
        {
            uds_transport_mgr_ = _uds_transport_mgr;
            // create the conversation config (vd & dm) out of passed config
            CreateConversationConfig(_config);
        }

        // startup
        public void Startup()
        {
        }

        // shutdown
        public void Shutdown()
        {
        }

        // Get the required conversion
        public DmConversation? GetDiagnosticClientConversation(string _conversation_name)
        {
            DmConversation? dm_conversation = null;
            if (conversation_config_.ContainsKey(_conversation_name))
            {
                // create the conversation
                dm_conversation = new DmConversation(_conversation_name, conversation_config_[_conversation_name]);
                // Register the connection
                dm_conversation.RegisterConnection(
                  uds_transport_mgr_.doip_transport_handler.FindOrCreateTcpConnection(
                    dm_conversation.Dm_conversion_handler_, 
                    conversation_config_[_conversation_name].Tcp_address, 
                    conversation_config_[_conversation_name].Port_num
                  )
                );
            }
            return dm_conversation;
        }

        // Get the required conversion
        public VdConversation GetDiagnosticClientVehicleDiscoveryConversation(string _conversation_name)
        {
            VdConversation? vd_conversation = null;
            if(vd_conversation_config_.ContainsKey(_conversation_name))
            {
                // create the conversation
                vd_conversation = new VdConversation(_conversation_name, vd_conversation_config_[_conversation_name]);
                // Register the connection
                vd_conversation.RegisterConnection(
                  uds_transport_mgr_.doip_transport_handler.FindOrCreateUdpConnection(
                    vd_conversation.GetConversationHandler(), 
                    vd_conversation_config_[_conversation_name].Udp_address, 
                    vd_conversation_config_[_conversation_name].Port_num
                  )
                );
            }
            return vd_conversation;
        }

        // function to create or find new conversion
        private void CreateConversationConfig(DcmClientConfig? _config)
        {
            if(_config == null )
            {
                Console.WriteLine("CreateConversationConfig error : _config == null");
                return;
            }

            // Create Vehicle discovery config
            {
                ConversionIdentifierType conversion_identifier = new();
                conversion_identifier.Udp_address = _config.Udp_ip_address;
                conversion_identifier.Udp_broadcast_address = _config.Udp_broadcast_address;
                conversion_identifier.Port_num = 0;  // random selection of port number
                vd_conversation_config_.Add("VehicleDiscovery", conversion_identifier);
            }

            // Create Conversation config
            {
                for (byte conv_count = 0; conv_count < _config.Conversation.Num_of_conversation; conv_count++)
                {
                    if(_config.Conversation.ConversationPropertys[conv_count] != null)
                    {
                        ConversionIdentifierType conversion_identifier = new();
                        conversion_identifier.Rx_buffer_size = _config.Conversation.ConversationPropertys[conv_count].Rx_buffer_size;
                        conversion_identifier.P2_client_max = _config.Conversation.ConversationPropertys[conv_count].P2_client_max;
                        conversion_identifier.P2_star_client_max = _config.Conversation.ConversationPropertys[conv_count].P2_star_client_max;
                        conversion_identifier.Source_address = _config.Conversation.ConversationPropertys[conv_count].Source_address;
                        conversion_identifier.Tcp_address = _config.Conversation.ConversationPropertys[conv_count].Network.Tcp_ip_address;
                        conversion_identifier.Port_num = 0;  // random selection of port number
                        conversation_config_.Add(_config.Conversation.ConversationPropertys[conv_count].Conversation_name, conversion_identifier);
                    }
                }
            }
        }
    };
}

using Diag_Doip_Uds.Appl.Dcm.Service;
using Diag_Doip_Uds.Appl.Include;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport.Conversion_manager;
using Diag_Doip_Uds.Lib.Utility_support.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Appl.Dcm.Conversation
{
    using ProtocolKind = String;
    using Priority = Byte;
    using ChannelID = UInt32;
    using Address = UInt16;
    using PortNumber = UInt16;
    using MetaInfoMap = Dictionary<string, string>;
    using IpAddress = String;
    using VehicleIdentificationResponseResult = Pair<VehicleResponseResult, IVehicleInfoMessage>;
    using PreselectionMode = Byte;
    using PreselectionValue = List<byte>;
    using VehicleAddrInfoResponseStruct = VehicleAddrInfoResponse;
    using ConversionHandlerID = Byte;

    // Vehicle Info Message implementation class
    public sealed class VehicleInfoMessageImpl : IVehicleInfoMessage
    {
        public VehicleInfoMessageImpl(
        Dictionary<UInt16, VehicleAddrInfoResponse> _vehicle_info_collection)
        {
            vehicle_info_messages_ = new();
            foreach (var item in _vehicle_info_collection)
            {
                Push(item.Value);
            }
        }

        public List<VehicleAddrInfoResponse> GetVehicleList() { return vehicle_info_messages_; }

        // Function to push the vehicle address info received
        private void Push(VehicleAddrInfoResponse _vehicle_addr_info_response)
        {
            vehicle_info_messages_.Add(_vehicle_addr_info_response);
        }

        // store the vehicle info message list
        private List<VehicleAddrInfoResponse> vehicle_info_messages_;
    };

    /*
    @ Class Name        : VdConversation
    @ Class Description : Class to query Diagnostic Server list
    */
    public class VdConversation
    {
        // shared pointer to store the conversion handler
        private ConversionHandler vd_conversion_handler_;

        // conversation name
        private string conversation_name_;

        // Vehicle broadcast address
        private string? broadcast_address_;

        // Tp connection
        private Lib.Uds_transport_layer_api.Uds_transport.Connection? connection_ptr_;

        // container to store the vehicle information
        private Dictionary<UInt16, VehicleAddrInfoResponseStruct> vehicle_info_collection_;

        // mutex to lock the vehicle info collection container
        private Mutex vehicle_info_container_mutex_;
        // ctor
        public VdConversation(string _conversion_name, ConversionIdentifierType _conversion_identifier)
        {
            vd_conversion_handler_ = new VdConversationHandler(_conversion_identifier.Handler_id, this);
            conversation_name_ = _conversion_name;
            broadcast_address_ = _conversion_identifier.Udp_broadcast_address;
            connection_ptr_ = null;
            vehicle_info_collection_ = new();
            vehicle_info_container_mutex_ = new();
        }

        // startup
        public void Startup()
        {
            if (connection_ptr_ == null) return;
            // initialize the connection
            connection_ptr_.Initialize();
            // start the connection
            connection_ptr_.Start();
        }

        // shutdown
        public void Shutdown()
        {
            if (connection_ptr_ == null) return;
            // shutdown connection
            connection_ptr_.Stop();
        }

        // Register Connection
        public void RegisterConnection(Lib.Uds_transport_layer_api.Uds_transport.Connection _connection)
        {
            connection_ptr_ = _connection;
        }

        // Send Vehicle Identification Request and get response
        public VehicleIdentificationResponseResult 
        SendVehicleIdentificationRequest(VehicleAddrInfoRequest _vehicle_info_request)
        {
            VehicleIdentificationResponseResult ret_val = new(VehicleResponseResult.kTransmitFailed, null);
            if (connection_ptr_ == null || broadcast_address_ == null) return ret_val;

            // Deserialize first , Todo: Add optional when deserialize fails
            Pair<PreselectionMode, PreselectionValue> vehicle_info_request_deserialized_value =
                DeserializeVehicleInfoRequest(_vehicle_info_request);

            if (VerifyVehicleInfoRequest(vehicle_info_request_deserialized_value.First,
                                         (byte)vehicle_info_request_deserialized_value.Second.Count))
            {
                if (connection_ptr_.Transmit(new VdMessage(
                        vehicle_info_request_deserialized_value.First, vehicle_info_request_deserialized_value.Second,
                        broadcast_address_)) != TransmissionResult.kTransmitFailed)
                {
                    // Check if any response received
                    if (vehicle_info_collection_.Count == 0)
                    {
                        // no response received
                        ret_val.First = VehicleResponseResult.kNoResponseReceived;
                    }
                    else
                    {
                        ret_val.First = VehicleResponseResult.kStatusOk;
                        ret_val.Second = new VehicleInfoMessageImpl(vehicle_info_collection_);
                        // all the responses are copied, now clear the map
                        vehicle_info_collection_.Clear();
                    }
                }
            }
            else
            {
                ret_val.First = VehicleResponseResult.kInvalidParameters;
            }
            return ret_val;
        }

        // Get the list of available Diagnostic Server
        public IVehicleInfoMessage? GetDiagnosticServerList()
        {
            return null;
        }

        // Indicate message Diagnostic message reception over TCP to user
        public Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                  Address _target_addr,
                                                        TargetAddressType _type, 
                                                                ChannelID _channel_id, 
                                                                      int _size,
                                                                 Priority _priority, 
                                                             ProtocolKind _protocol_kind,
                                                               List<byte> _payloadInfo)
        {
            Pair<IndicationResult, UdsMessage> ret_val = new(IndicationResult.kIndicationNOk, null);
            if (_payloadInfo.Count != 0)
            {
                ret_val.First = IndicationResult.kIndicationOk;
                ret_val.Second = new VdMessage();
                ret_val.Second.GetPayload().EnsureCapacity(_size);
            }
            return ret_val;
        }

        // Hands over a valid message to conversion
        public void HandleMessage(UdsMessage _message)
        {
            if (_message != null)
            {
                vehicle_info_container_mutex_.WaitOne();
                try
                {
                    Pair<UInt16, VehicleAddrInfoResponseStruct> vehicle_info_request =
                        DeserializeVehicleInfoResponse(_message);

                    vehicle_info_collection_.Add(vehicle_info_request.First, vehicle_info_request.Second);
                }
                finally
                {
                    vehicle_info_container_mutex_.ReleaseMutex();
                }
            }
        }

        // Function to verify Vehicle Info requests
        private bool VerifyVehicleInfoRequest(PreselectionMode _preselection_mode, byte _preselection_value_length)
        {
            bool is_veh_info_valid = false;
            if ((_preselection_mode != 0U) && (_preselection_value_length != 0U))
            {
                // 1U : DoIP Entities with given VIN
                if (_preselection_mode == 1U && (_preselection_value_length == 17U))
                {
                    is_veh_info_valid = true;
                }
                // 2U : DoIP Entities with given EID
                else if (_preselection_mode == 2U && (_preselection_value_length == 6U))
                {
                    is_veh_info_valid = true;
                }
                else
                {
                }
            }
            // 0U : No preselection
            else if (_preselection_mode == 0U && (_preselection_value_length == 0U))
            {
                is_veh_info_valid = true;
            }
            else
            {
            }
            return is_veh_info_valid;
        }

        // Function to deserialize the received Vehicle Identification Response/ Announcement
        private static Pair<UInt16, VehicleAddrInfoResponseStruct> 
        DeserializeVehicleInfoResponse(UdsMessage _message)
        {
            byte start_index_vin = 0;
            byte total_vin_length = 17;
            byte start_index_eid = 19;
            byte start_index_gid = 25;
            byte total_eid_gid_length = 6;

            string vehicle_info_data_vin = Utils.ConvertToAsciiString(start_index_vin, total_vin_length, _message.GetPayload());
            string vehicle_info_data_eid = Utils.ConvertToHexString(start_index_eid, total_eid_gid_length, _message.GetPayload());
            string vehicle_info_data_gid = Utils.ConvertToHexString(start_index_gid, total_eid_gid_length, _message.GetPayload());

            UInt16 logical_address = ((UInt16)(((_message.GetPayload()[17] & 0xFF) << 8) | (_message.GetPayload()[18] & 0xFF)));

            // Create the structure out of the extracted string
            VehicleAddrInfoResponseStruct vehicle_addr_info = new()
            {
                Ip_address = _message.GetHostIpAddress(),   // remote ip address
                Logical_address = logical_address,          // logical address
                VIN = vehicle_info_data_vin,                // vin
                EID = vehicle_info_data_eid,                // eid
                GID = vehicle_info_data_gid                 // gid
            };

            return new(logical_address, vehicle_addr_info);
        }

        // Get Conversation Handlers
        public ConversionHandler GetConversationHandler()
        {
            return vd_conversion_handler_;
        }

        private static Pair<PreselectionMode, PreselectionValue> 
        DeserializeVehicleInfoRequest(VehicleAddrInfoRequest _vehicle_info_request)
        {

            Pair<PreselectionMode, PreselectionValue> ret_val = new(_vehicle_info_request.Preselection_mode, new());

            if (ret_val.First == 1U)
            {
                // 1U : DoIP Entities with given VIN
                Utils.SerializeVINFromString(_vehicle_info_request.Preselection_value, ret_val.Second,
                                       (byte)_vehicle_info_request.Preselection_value.Length, 1);
            }
            else if (ret_val.First == 2U)
            {
                // 2U : DoIP Entities with given EID
                _vehicle_info_request.Preselection_value = new(_vehicle_info_request.Preselection_value.Where(c => c != ':').ToArray());

                Utils.SerializeEIDGIDFromString(_vehicle_info_request.Preselection_value, ret_val.Second,
                                          (byte)_vehicle_info_request.Preselection_value.Length, 2);
            }
            return ret_val;
        }
    };

    /*
    @ Class Name        : DmConversationHandler
    @ Class Description : Class to establish connection with Diagnostic Server
    */
    public class VdConversationHandler : ConversionHandler
    {
        private VdConversation vd_conversation_;
        // ctor
        public VdConversationHandler(ConversionHandlerID _handler_id, VdConversation _vd_conversion)
            :base(_handler_id)
        {
            vd_conversation_ = _vd_conversion;
        }

        // Indicate message Diagnostic message reception over TCP to user
        public override Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                           Address _target_addr,
                                                                 TargetAddressType _type, 
                                                                         ChannelID _channel_id, 
                                                                               int _size,
                                                                          Priority _priority, 
                                                                      ProtocolKind _protocol_kind,
                                                                        List<byte> _payloadInfo)
        {
            return (vd_conversation_.IndicateMessage(_source_addr, _target_addr, _type, _channel_id, 
                                                     _size, _priority, _protocol_kind, _payloadInfo));
        }

        // Hands over a valid message to conversion
        public override void HandleMessage(UdsMessage _message)
        {
            vd_conversation_.HandleMessage(_message);
        }
    };
}

using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Doip_client.Common;
using Diag_Doip_Uds.Lib.Doip_client.Handler;
using Diag_Doip_Uds.Lib.Doip_client.Sockets;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    using VehiclePayloadType = Pair<UInt16, byte>;
    using MetaInfoMap = Dictionary<string, string>;
    /*
    @ Class Name        : VehicleDiscoveryHandler
    @ Class Description : Class used as a handler to process vehicle announcement messages
    */
    class VehicleDiscoveryHandler
    {
        // socket reference
        private UdpSocketHandler udp_socket_handler_;
        // transport handler reference
        private UdpTransportHandler udp_transport_handler_;
        // channel reference
        private UdpChannel channel_;
        // ctor
        public VehicleDiscoveryHandler(UdpSocketHandler _udp_socket_handler,
                       UdpTransportHandler _udp_transport_handler, UdpChannel _channel)
        {
            udp_socket_handler_ = _udp_socket_handler;
            udp_transport_handler_ = _udp_transport_handler;
            channel_ = _channel;
        }

        // Function to process Vehicle announcement response
        public void ProcessVehicleAnnouncementResponse(DoipMessage _doip_payload)
        {
            if (channel_.GetChannelState().GetVehicleDiscoveryStateContext().GetActiveState().GetState() ==
                VehicleDiscoveryState.kWaitForVehicleAnnouncement)
            {
                // Deserialize and Add to job executor
            }
            else
            {
                // ignore
            }
        }

        // Function to process Vehicle identification response
        public void ProcessVehicleIdentificationResponse(DoipMessage _doip_payload)
        {
            if (channel_.GetChannelState().GetVehicleIdentificationStateContext().GetActiveState().GetState() ==
                VehicleIdentificationState.kViWaitForVehicleIdentificationRes)
            {
                // Deserialize data to indicate to upper layer
                Pair<IndicationResult, UdsMessage> ret_val = 
                    udp_transport_handler_.IndicateMessage(0, 0, TargetAddressType.kPhysical, 0,
                        _doip_payload.Payload.Count, 0, "DoIPUdp", _doip_payload.Payload);
                if ((ret_val.First == IndicationResult.kIndicationOk) && (ret_val.Second != null))
                {
                    // Add meta info about ip address
                    MetaInfoMap meta_info_map = new MetaInfoMap { { "kRemoteIpAddress", _doip_payload.Host_ip_address} };
                    ret_val.Second.AddMetaInfo(meta_info_map);
                    // copy to application buffer
                    ret_val.Second.GetPayload().AddRange(_doip_payload.Payload);
                    udp_transport_handler_.HandleMessage(ret_val.Second);
                }
            }
            else
            {
                // ignore
            }
        }

        // Function to send vehicle identification request
        public TransmissionResult SendVehicleIdentificationRequest(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;
            if (channel_.GetChannelState().GetVehicleIdentificationStateContext().GetActiveState().GetState() ==
                VehicleIdentificationState.kViIdle)
            {
                if (HandleVehicleIdentificationRequest(_message) == TransmissionResult.kTransmitOk)
                {
                    ret_val = TransmissionResult.kTransmitOk;

                    channel_.GetChannelState().GetVehicleIdentificationStateContext().TransitionTo(
                        VehicleIdentificationState.kViWaitForVehicleIdentificationRes);
                    // Wait for 2 sec to collect all the vehicle identification response
                    channel_.WaitForResponse(
              
                    () => {
                        channel_.GetChannelState().GetVehicleIdentificationStateContext().TransitionTo(
                            VehicleIdentificationState.kViDoIPCtrlTimeout);
                    },
                    () => {
                        // do nothing
                    },
                    (int)Common_doip_types.kDoIPCtrl);
                    channel_.GetChannelState().GetVehicleIdentificationStateContext().TransitionTo(
                        VehicleIdentificationState.kViIdle);
                }
                else
                {
                    // failed, do nothing
                }
            }
            else
            {
                // not free
            }
            return ret_val;
        }

        // Function to handle Vehicle Identification Request
        private TransmissionResult HandleVehicleIdentificationRequest(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;

            UdpMessageType doip_vehicle_ident_req = new();
            // Get preselection mode
            byte preselection_mode = _message.GetPayload()[BYTE_POS_ONE];

            // get the payload type & length from preselection mode
            VehiclePayloadType doip_vehicle_payload_type = GetVehicleIdentificationPayloadType(preselection_mode);

            // create header
            CreateDoipGenericHeader(doip_vehicle_ident_req.Tx_buffer_, doip_vehicle_payload_type.First,
                                    doip_vehicle_payload_type.Second);
            // set remote ip
            doip_vehicle_ident_req.Host_ip_address_ = _message.GetHostIpAddress();

            // set remote port num
            doip_vehicle_ident_req.Host_port_num_ = _message.GetHostPortNumber();

            // Copy only if containing VIN / EID
            if (doip_vehicle_payload_type.First != Common_doip_types.kDoip_VehicleIdentification_ReqType)
            {
                //doip_vehicle_ident_req.Tx_buffer_.insert(doip_vehicle_ident_req->tx_buffer_.begin() + kDoipheadrSize,
                //                                          message->GetPayload().begin() + 2U, message->GetPayload().end());
                doip_vehicle_ident_req.Tx_buffer_.AddRange(_message.GetPayload().GetRange(2, _message.GetPayload().Count - 2));
            }
            if (udp_socket_handler_.Transmit(doip_vehicle_ident_req))
            {
                ret_val = TransmissionResult.kTransmitOk;
            }
            return ret_val;
        }

        private static void CreateDoipGenericHeader(List<byte> _doipHeader, UInt16 _payloadType, UInt32 _payloadLen)
        {
            _doipHeader.Add(Common_doip_types.kDoip_ProtocolVersion);
            //_doipHeader.Add(~((byte)Common_doip_types.kDoip_ProtocolVersion));
            var inverse_protocol_version = ~Common_doip_types.kDoip_ProtocolVersion;
            _doipHeader.Add((byte)inverse_protocol_version);
            _doipHeader.Add((byte)((_payloadType & 0xFF00) >> 8));
            _doipHeader.Add((byte)(_payloadType & 0x00FF));
            _doipHeader.Add((byte)((_payloadLen & 0xFF000000) >> 24));
            _doipHeader.Add((byte)((_payloadLen & 0x00FF0000) >> 16));
            _doipHeader.Add((byte)((_payloadLen & 0x0000FF00) >> 8));
            _doipHeader.Add((byte)(_payloadLen & 0x000000FF));
        }

        private static VehiclePayloadType GetVehicleIdentificationPayloadType(byte _preselection_mode)
        {
            VehiclePayloadType ret_val = new(0, 0);
            switch (_preselection_mode)
            {
                case 0:
                    ret_val.First = Common_doip_types.kDoip_VehicleIdentification_ReqType;
                    ret_val.Second = Common_doip_types.kDoip_VehicleIdentification_ReqLen;
                    break;
                case 1:
                    ret_val.First = Common_doip_types.kDoip_VehicleIdentificationVIN_ReqType;
                    ret_val.Second = Common_doip_types.kDoip_VehicleIdentificationVIN_ReqLen;
                    break;
                case 2:
                    ret_val.First = Common_doip_types.kDoip_VehicleIdentificationEID_ReqType;
                    ret_val.Second = Common_doip_types.kDoip_VehicleIdentificationEID_ReqLen;
                    break;
                default:
                    break;
            }
            return ret_val;
        }
    };

    /*
    @ Class Name        : UdpChannelHandlerImpl
    @ Class Description : Class to handle received messages from lower layer
    */
    class UdpChannelHandlerImpl
    {
        // handler to process vehicle announcement
        private VehicleDiscoveryHandler vehicle_discovery_handler_;
        // handler to process vehicle identification req/ res
        private VehicleDiscoveryHandler vehicle_identification_handler_;
        // mutex to protect critical section
        private Mutex channel_handler_lock = new();

        // ctor
        public UdpChannelHandlerImpl(UdpSocketHandler _udp_socket_handler_bcast, UdpSocketHandler _udp_socket_handler_ucast,
                                     UdpTransportHandler _udp_transport_handler, UdpChannel _channel)
        {
            vehicle_discovery_handler_ = new(_udp_socket_handler_bcast, _udp_transport_handler, _channel);
            vehicle_identification_handler_ = new(_udp_socket_handler_ucast, _udp_transport_handler, _channel);
        }

        // Function to trigger transmission
        public TransmissionResult Transmit(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;
            // Get the udp handler type from payload
            byte handler_type = _message.GetPayload()[BYTE_POS_ZERO];
            // deserialize and send to proper handler
            switch (handler_type)
            {
                case 0:
                    // 0U -> Vehicle Identification Req
                    ret_val = vehicle_identification_handler_.SendVehicleIdentificationRequest(_message);
                    break;
                case 1:
                    // 1U -> Power Mode Req
                    break;
            }
            return ret_val;
        }

        // process message unicast
        public void HandleMessage(UdpMessageType _udp_rx_message)
        {
            byte nack_code = Common_doip_types.kDoip_GenericHeader_IncorrectPattern;
            DoipMessage doip_rx_message = new();
            doip_rx_message.Host_ip_address = _udp_rx_message.Host_ip_address_;
            doip_rx_message.Protocol_version = _udp_rx_message.Rx_buffer_[0];
            doip_rx_message.Protocol_version_inv = _udp_rx_message.Rx_buffer_[1];
            doip_rx_message.Payload_type = GetDoIPPayloadType(_udp_rx_message.Rx_buffer_);
            doip_rx_message.Payload_length = GetDoIPPayloadLength(_udp_rx_message.Rx_buffer_);
            // Process the Doip Generic header check
            if (ProcessDoIPHeader(doip_rx_message, ref nack_code))
            {
                //doip_rx_message.payload.resize(udp_rx_message->rx_buffer_.size() - kDoipheadrSize);
                doip_rx_message.Payload.EnsureCapacity(_udp_rx_message.Rx_buffer_.Count - Common_doip_types.kDoipheadrSize);
                // copy payload locally
                // *** Potential error!!! ***
                //(void)std::copy(udp_rx_message->rx_buffer_.begin() + kDoipheadrSize,
                //                 udp_rx_message->rx_buffer_.begin() + kDoipheadrSize + udp_rx_message->rx_buffer_.size(),
                //                 doip_rx_message.payload.begin());
                doip_rx_message.Payload.AddRange(
                    _udp_rx_message.Rx_buffer_.GetRange(Common_doip_types.kDoipheadrSize, _udp_rx_message.Rx_buffer_.Count
                    - Common_doip_types.kDoipheadrSize)
                );
                ProcessDoIPPayload(doip_rx_message);
            }
            else
            {
                // send NACK or ignore
                // SendDoIPNACKMessage(nack_code);
            }
        }

        // process message broadcast
        public void HandleMessageBroadcast(UdpMessageType _udp_rx_message)
        {
            byte nack_code = Common_doip_types.kDoip_GenericHeader_IncorrectPattern;
            DoipMessage doip_rx_message = new();
            doip_rx_message.Protocol_version = _udp_rx_message.Rx_buffer_[0];
            doip_rx_message.Protocol_version_inv = _udp_rx_message.Rx_buffer_[1];
            doip_rx_message.Payload_type = GetDoIPPayloadType(_udp_rx_message.Rx_buffer_);
            doip_rx_message.Payload_length = GetDoIPPayloadLength(_udp_rx_message.Rx_buffer_);
            // Process the Doip Generic header check
            if (ProcessDoIPHeader(doip_rx_message, ref nack_code))
            {
                //doip_rx_message.payload.resize(udp_rx_message->rx_buffer_.size() - kDoipheadrSize);
                doip_rx_message.Payload.EnsureCapacity(_udp_rx_message.Rx_buffer_.Count - Common_doip_types.kDoipheadrSize);
                // copy payload locally
                // *** Potential error!!! ***
                //(void)std::copy(udp_rx_message->rx_buffer_.begin() + kDoipheadrSize,
                //                 udp_rx_message->rx_buffer_.begin() + kDoipheadrSize + udp_rx_message->rx_buffer_.size(),
                //                 doip_rx_message.payload.begin());
                doip_rx_message.Payload.AddRange(
                    _udp_rx_message.Rx_buffer_.GetRange(Common_doip_types.kDoipheadrSize, _udp_rx_message.Rx_buffer_.Count
                    - Common_doip_types.kDoipheadrSize)
                );
                vehicle_discovery_handler_.ProcessVehicleAnnouncementResponse(doip_rx_message);
            }
            else
            {
                // send NACK or ignore
                // SendDoIPNACKMessage(nack_code);
            }
        }

        // Function to process DoIP Header
        private static bool ProcessDoIPHeader(DoipMessage _doip_rx_message, ref byte _nackCode)
        {
            bool ret_val = false;
            var inverse_protocol_version = ~Common_doip_types.kDoip_ProtocolVersion;
            var inverse_protocol_version_def = ~Common_doip_types.kDoip_ProtocolVersion_Def;
            /* Check the header synchronisation pattern */
            if (((_doip_rx_message.Protocol_version == Common_doip_types.kDoip_ProtocolVersion) &&
                 (_doip_rx_message.Protocol_version_inv == (byte)(inverse_protocol_version))) ||
                ((_doip_rx_message.Protocol_version == Common_doip_types.kDoip_ProtocolVersion_Def) &&
                 (_doip_rx_message.Protocol_version_inv == (byte)(inverse_protocol_version_def))))
            {
                /* Check the supported payload type */
                if (_doip_rx_message.Payload_type == Common_doip_types.kDoip_VehicleAnnouncement_ResType)
                {
                    /* Req-[AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00017] */
                    if (_doip_rx_message.Payload_length <= Common_doip_types.kDoip_Protocol_MaxPayload)
                    {
                        /* Req-[AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00018] */
                        if (_doip_rx_message.Payload_length <= Common_doip_types.kUdpChannelLength)
                        {
                            /* Req-[AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00019] */
                            if (ProcessDoIPPayloadLength(_doip_rx_message.Payload_length, _doip_rx_message.Payload_type))
                            {
                                ret_val = true;
                            }
                            else
                            {
                                // Send NACK code 0x04, close the socket
                                _nackCode = Common_doip_types.kDoip_GenericHeader_InvalidPayloadLen;
                                // socket closure ??
                            }
                        }
                        else
                        {
                            // Send NACK code 0x03, discard message
                            _nackCode = Common_doip_types.kDoip_GenericHeader_OutOfMemory;
                        }
                    }
                    else
                    {
                        // Send NACK code 0x02, discard message
                        _nackCode = Common_doip_types.kDoip_GenericHeader_MessageTooLarge;
                    }
                }
                else
                {   // Send NACK code 0x01, discard message
                    _nackCode = Common_doip_types.kDoip_GenericHeader_UnknownPayload;
                }
            }
            else
            {  // Send NACK code 0x00, close the socket
                _nackCode = Common_doip_types.kDoip_GenericHeader_IncorrectPattern;
                // socket closure
            }
            return ret_val;
        }

        // Function to verify payload length of various payload type
        private static bool ProcessDoIPPayloadLength(UInt32 _payload_len, UInt16 _payload_type)
        {
            bool ret_val = false;
            switch (_payload_type)
            {
                case Common_doip_types.kDoip_VehicleAnnouncement_ResType:
                    {
                        if (_payload_len <= (UInt32)Common_doip_types.kDoip_VehicleAnnouncement_ResMaxLen) ret_val = true;
                        break;
                    }
                default:
                    // do nothing
                    break;
            }
            return ret_val;
        }

        // Function to get payload type
        private static UInt16 GetDoIPPayloadType(List<byte> _payload)
        {
            return ((UInt16)(((_payload[BYTE_POS_TWO] & 0xFF) << 8) | (_payload[BYTE_POS_THREE] & 0xFF)));
        }

        // Function to get payload length
        private static UInt32 GetDoIPPayloadLength(List<byte> _payload)
        {
            // *** Potential error!!! ***
            return ((UInt32)((_payload[BYTE_POS_FOUR] << 24) & 0xFF000000) |
                    (UInt32)((_payload[BYTE_POS_FIVE] << 16) & 0x00FF0000) |
                    (UInt32)((_payload[BYTE_POS_SIX] << 8) & 0x0000FF00) | 
                    (UInt32)((_payload[BYTE_POS_SEVEN] & 0x000000FF)));
        }

        // Function to process DoIP payload responses
        private void ProcessDoIPPayload(DoipMessage _doip_payload, 
                                rx_socket_type socket_type = rx_socket_type.kUnicast)
        {
            channel_handler_lock.WaitOne();
            try
            {
                switch (_doip_payload.Payload_type)
                {
                    case Common_doip_types.kDoip_VehicleAnnouncement_ResType:
                        {
                            vehicle_identification_handler_.ProcessVehicleIdentificationResponse(_doip_payload);
                            break;
                        }
                    default:
                        /* do nothing */
                        break;
                }
            }
            finally
            {
                channel_handler_lock.ReleaseMutex();
            }
        }
    };
}

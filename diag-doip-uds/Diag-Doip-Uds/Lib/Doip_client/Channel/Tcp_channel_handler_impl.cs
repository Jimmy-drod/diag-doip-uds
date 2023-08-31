using Diag_Doip_Uds.Lib.Common;
using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Doip_client.Common;
using Diag_Doip_Uds.Lib.Doip_client.Handler;
using Diag_Doip_Uds.Lib.Doip_client.Sockets;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    /*
    @ Class Name        : RoutingActivationHandler
    @ Class Description : Class used as a handler to process routing activation messages
    */
    class RoutingActivationHandler
    {
        // socket reference
        private TcpSocketHandler tcp_socket_handler_;
        // channel reference
        private TcpChannel channel_;

        // strong type holding activation type
        struct RoutingActivationAckType
        {
            public byte act_type_;
        };

        // ctor
        public RoutingActivationHandler(TcpSocketHandler _tcp_socket_handler, TcpChannel _channel)
        {
            tcp_socket_handler_ = _tcp_socket_handler;
            channel_ = _channel;
        }

        // Function to process Routing activation response
        public void ProcessDoIPRoutingActivationResponse(DoipMessage _doip_payload)
        {
            routingActivationState final_state = routingActivationState.kRoutingActivationFailed;
            if (channel_.GetChannelState().GetRoutingActivationStateContext().GetActiveState().GetState() ==
                routingActivationState.kWaitForRoutingActivationRes)
            {
                // get the logical address of client
                UInt16 client_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ZERO]) << 8) & 0xFF00) |
                                        ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ONE]) & 0x00FF));
                // get the logical address of Server
                UInt16 server_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_TWO]) << 8) & 0xFF00) |
                                        ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_THREE]) & 0x00FF));
                // get the ack code
                RoutingActivationAckType rout_act_type = new(){ act_type_ = _doip_payload.Payload[Common_header.BYTE_POS_FOUR] };
                switch (rout_act_type.act_type_)
                {
                    case Common_doip_types.kDoip_RoutingActivation_ResCode_RoutingSuccessful:
                        {
                            // routing successful
                            final_state = routingActivationState.kRoutingActivationSuccessful;
                            Console.WriteLine($"RoutingActivation successfully activated in remote" +
                                $" server with logical Address (0x{server_address.ToString("X")})");
                        }
                        break;
                    case Common_doip_types.kDoip_RoutingActivation_ResCode_ConfirmtnRequired:
                        {
                            // trigger routing activation after sometime, not implemented yet
                            Console.WriteLine($"RoutingActivation is activated, confirmation required" +
                                $" in remote server with logical Address (0x{server_address.ToString("X")})");
                        }
                        break;
                    default:
                        // failure, do nothing
                        Console.WriteLine($"Routing activation denied due to {GetRespCodeValues(rout_act_type)}");
                        break;
                }
                channel_.GetChannelState().GetRoutingActivationStateContext().TransitionTo(final_state);
                channel_.WaitCancel();
            }
            else
            {
                /* ignore */
            }
        }

        // Function to send Routing activation request
        public TransmissionResult SendRoutingActivationRequest(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;
            TcpMessageType doip_routing_act_req = new TcpMessageType();
            // reserve bytes in vector
            doip_routing_act_req.TxBuffer_.EnsureCapacity(Common_doip_types.kDoipheadrSize +
                                          Common_doip_types.kDoip_RoutingActivation_ReqMinLen);
            // create header
            CreateDoipGenericHeader(doip_routing_act_req.TxBuffer_, Common_doip_types.kDoip_RoutingActivation_ReqType,
                                    Common_doip_types.kDoip_RoutingActivation_ReqMinLen);
            // Add source address
            doip_routing_act_req.TxBuffer_.Add((byte)((_message.GetSa() & 0xFF00) >> 8));
            doip_routing_act_req.TxBuffer_.Add((byte)(_message.GetSa() & 0x00FF));
            // Add activation type
            doip_routing_act_req.TxBuffer_.Add((byte)Common_doip_types.kDoip_RoutingActivation_ReqActType_Default);
            // Add reservation byte , default zeroes
            doip_routing_act_req.TxBuffer_.Add((byte)0x00);
            doip_routing_act_req.TxBuffer_.Add((byte)0x00);
            doip_routing_act_req.TxBuffer_.Add((byte)0x00);
            doip_routing_act_req.TxBuffer_.Add((byte)0x00);
            // transmit
            if (tcp_socket_handler_.Transmit(doip_routing_act_req))
            {
                ret_val = TransmissionResult.kTransmitOk;
            }
            return ret_val;
        }

        private void CreateDoipGenericHeader(List<byte> _doipHeader, UInt16 _payloadType, UInt32 _payloadLen)
        {
            _doipHeader.Add(Common_doip_types.kDoip_ProtocolVersion);
            var inverse_protocol_version = ~Common_doip_types.kDoip_ProtocolVersion;
            _doipHeader.Add((byte)inverse_protocol_version);
            _doipHeader.Add((byte)((_payloadType & 0xFF00) >> 8));
            _doipHeader.Add((byte)(_payloadType & 0x00FF));
            _doipHeader.Add((byte)((_payloadLen & 0xFF000000) >> 24));
            _doipHeader.Add((byte)((_payloadLen & 0x00FF0000) >> 16));
            _doipHeader.Add((byte)((_payloadLen & 0x0000FF00) >> 8));
            _doipHeader.Add((byte)(_payloadLen & 0x000000FF));
        }

        private string GetRespCodeValues(RoutingActivationAckType _act_type)
        {
            string msg = string.Empty;
            switch (_act_type.act_type_)
            {
                case Common_doip_types.kDoip_RoutingActivation_ResCode_UnknownSA:
                    msg = "unknown source address.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_AllSocktActive:
                    msg = "all Socket active.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_DifferentSA:
                    msg = "SA different on already connected socket.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_ActiveSA:
                    msg = "SA active on different socket.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_AuthentnMissng:
                    msg = "missing authentication.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_ConfirmtnRejectd:
                    msg = "rejected confirmation.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_UnsupportdActType:
                    msg = "unsupported routing activation type.";
                    break;
                case Common_doip_types.kDoip_RoutingActivation_ResCode_TLSRequired:
                    msg = "required TLS socket.";
                    break;
                default:
                    msg = "unknown reason.";
                    break;
            }
            msg += " (0x" + _act_type.act_type_.ToString("X") + ")";
            return msg;
        }
    };

    /*
    @ Class Name        : DiagnosticMessageHandler
    @ Class Description : Class used as a handler to process diagnostic messages
    */
    class DiagnosticMessageHandler
    {
        // socket reference
        private TcpSocketHandler tcp_socket_handler_;
        // transport handler reference
        private TcpTransportHandler tcp_transport_handler_;
        // channel reference
        private TcpChannel channel_;

        // strong type acknowledgement type
        struct DiagAckType
        {
            public byte ack_type_;
        };
        // ctor
        public DiagnosticMessageHandler(TcpSocketHandler _tcp_socket_handler,
        TcpTransportHandler _tcp_transport_handler, TcpChannel _channel)
        {
            tcp_socket_handler_ = _tcp_socket_handler;
            tcp_transport_handler_ = _tcp_transport_handler;
            channel_ = _channel;
        }

        // Function to process Routing activation response
        public void ProcessDoIPDiagnosticAckMessageResponse(DoipMessage _doip_payload)
        {
            diagnosticState final_state = diagnosticState.kDiagnosticNegativeAckRecvd;
            if (channel_.GetChannelState().GetDiagnosticMessageStateContext().GetActiveState().GetState() ==
                diagnosticState.kWaitForDiagnosticAck)
            {
                // check the logical address of Server
                UInt16 server_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ZERO]) & 0xFF) << 8) |
                                        ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ONE]) & 0xFF));
                // check the logical address of client
                UInt16 client_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_TWO]) & 0xFF) << 8) |
                                        ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_THREE]) & 0xFF));

                // get the ack code
                DiagAckType diag_ack_type = new(){ ack_type_ = _doip_payload.Payload[Common_header.BYTE_POS_FOUR] };
                if (_doip_payload.Payload_type == Common_doip_types.kDoip_DiagMessagePosAck_Type)
                {
                    if (diag_ack_type.ack_type_ == Common_doip_types.kDoip_DiagnosticMessage_PosAckCode_Confirm)
                    {
                        final_state = diagnosticState.kDiagnosticPositiveAckRecvd;
                        Console.WriteLine($"Diagnostic message positively acknowledged " +
                            $"from remote server (0x{server_address.ToString("X")})");
                    }
                    else
                    {
                        // do nothing
                    }
                }
                else if (_doip_payload.Payload_type == Common_doip_types.kDoip_DiagMessageNegAck_Type)
                {
                    Console.WriteLine($"Diagnostic request denied due to {GetRespCodeValues(diag_ack_type)}");
                }
                else
                {
                    // do nothing
                }
                channel_.GetChannelState().GetDiagnosticMessageStateContext().TransitionTo(final_state);
                channel_.WaitCancel();
            }
            else
            {
                // ignore
            }
        }

        // Function to process Diagnostic message response
        public void ProcessDoIPDiagnosticMessageResponse(DoipMessage _doip_payload)
        {
            if (channel_.GetChannelState().GetDiagnosticMessageStateContext().GetActiveState().GetState() ==
                diagnosticState.kWaitForDiagnosticResponse)
            {
                // create the payload to send to upper layer
                List<byte> payload_info = new();
                // check the logical address of Server
                var server_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ZERO]) & 0xFF) << 8) | 
                                     ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_ONE]) & 0xFF));
                // check the logical address of client
                var client_address = (UInt16)((((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_TWO]) & 0xFF) << 8) | 
                                     ((UInt16)(_doip_payload.Payload[Common_header.BYTE_POS_THREE]) & 0xFF));
                // payload except the address
                //payload_info.resize(doip_payload.payload.size() - 4U);
                payload_info.EnsureCapacity(_doip_payload.Payload.Count - 4);
                // copy to application buffer
                //(void)std::copy(doip_payload.payload.begin() + 4u, doip_payload.payload.end(), payload_info.begin());
                payload_info.AddRange(_doip_payload.Payload.GetRange(4, _doip_payload.Payload.Count - 4));
                // Indicate upper layer about incoming data
                Pair<IndicationResult, UdsMessage> ret_val =
                    tcp_transport_handler_.IndicateMessage(server_address,
                                                           client_address,
                                                           TargetAddressType.kPhysical, 
                                                           0,
                                                           _doip_payload.Payload.Count - 4, 
                                                           0,
                                                           "DoIPTcp", 
                                                           payload_info);
                if (ret_val.First == IndicationResult.kIndicationPending)
                {
                    // keep channel alive since pending request received, do not change channel state
                }
                else
                {
                    // Check result and udsMessagePtr
                    if ((ret_val.First == IndicationResult.kIndicationOk) &&
                        (ret_val.Second != null))
                    {
                        // copy to application buffer
                        //(void)std::copy(payload_info.begin(), payload_info.end(), ret_val.second->GetPayload().begin());
                        ret_val.Second.GetPayload().AddRange(payload_info);
                        tcp_transport_handler_.HandleMessage(ret_val.Second);
                    }
                    else
                    {
                        // other errors
                        // set to idle
                        // raise error
                    }
                    channel_.GetChannelState().GetDiagnosticMessageStateContext().TransitionTo(
                        diagnosticState.kDiagIdle);
                }
            }
            else
            {
                // ignore
                Console.WriteLine($"Diagnostic message response ignored due to channel in state: " +
                    $"{channel_.GetChannelState().GetDiagnosticMessageStateContext().GetActiveState().GetState()}");
            }
        }

        // Function to send Diagnostic request
        public TransmissionResult SendDiagnosticRequest(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;
            TcpMessageType doip_diag_req = new();
            // reserve bytes in vector
            doip_diag_req.TxBuffer_.EnsureCapacity(Common_doip_types.kDoipheadrSize + 
                                                   Common_doip_types.kDoip_DiagMessage_ReqResMinLen + 
                                                   _message.GetPayload().Count);
            // create header
            CreateDoipGenericHeader(doip_diag_req.TxBuffer_, Common_doip_types.kDoip_DiagMessage_Type,
                (uint)(Common_doip_types.kDoip_DiagMessage_ReqResMinLen + _message.GetPayload().Count()));
            // Add source address
            doip_diag_req.TxBuffer_.Add((byte)((_message.GetSa() & 0xFF00) >> 8));
            doip_diag_req.TxBuffer_.Add((byte)(_message.GetSa() & 0x00FF));
            // Add target address
            doip_diag_req.TxBuffer_.Add((byte)((_message.GetTa() & 0xFF00) >> 8));
            doip_diag_req.TxBuffer_.Add((byte)(_message.GetTa() & 0x00FF));
            // Add data bytes
            //for (std::uint8_t byte: message->GetPayload()) { doip_diag_req->txBuffer_.push_back(byte); }
            doip_diag_req.TxBuffer_.AddRange(_message.GetPayload());
            // transmit
            if (!(tcp_socket_handler_.Transmit(doip_diag_req)))
            {
                // do nothing
            }
            else
            {
                ret_val = TransmissionResult.kTransmitOk;
            }
            return ret_val;
        }

        private static void CreateDoipGenericHeader(List<byte> _doipHeader, UInt16 _payloadType,UInt32 _payloadLen)
        {
            _doipHeader.Add(Common_doip_types.kDoip_ProtocolVersion);
            var inverse_protocol_version = ~Common_doip_types.kDoip_ProtocolVersion;
            _doipHeader.Add((byte)inverse_protocol_version);
            _doipHeader.Add((byte)((_payloadType & 0xFF00) >> 8));
            _doipHeader.Add((byte)(_payloadType & 0x00FF));
            _doipHeader.Add((byte)((_payloadLen & 0xFF000000) >> 24));
            _doipHeader.Add((byte)((_payloadLen & 0x00FF0000) >> 16));
            _doipHeader.Add((byte)((_payloadLen & 0x0000FF00) >> 8));
            _doipHeader.Add((byte)(_payloadLen & 0x000000FF));
        }

        private string GetRespCodeValues(DiagAckType _diag_ack_type)
        {
            string msg = string.Empty;
            switch (_diag_ack_type.ack_type_)
            {
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_InvalidSA:
                    msg = "invalid source address.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_UnknownTA:
                    msg = "unknown target address.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_MessageTooLarge:
                    msg = "diagnostic message too large.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_OutOfMemory:
                    msg = "server out of memory.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_TargetUnreachable:
                    msg = "target unreachable.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_UnknownNetwork:
                    msg = "unknown network.";
                    break;
                case Common_doip_types.kDoip_DiagnosticMessage_NegAckCode_TPError:
                    msg = "transport protocol error.";
                    break;
                default:
                    msg = "unknown reason.";
                    break;
            }
            return msg;
        }
    };

    /*
    @ Class Name        : TcpChannelHandlerImpl
    @ Class Description : Class to handle received messages from lower layer
    */
    class TcpChannelHandlerImpl
    {
        // handler to process routing activation req/ resp
        private RoutingActivationHandler routing_activation_handler_;
        // handler to process diagnostic message req/ resp
        private DiagnosticMessageHandler diagnostic_message_handler_;
        // mutex to protect critical section
        private Mutex channel_handler_lock = new();

        // ctor
        public TcpChannelHandlerImpl(TcpSocketHandler _tcp_socket_handler,
        TcpTransportHandler _tcp_transport_handler, TcpChannel _channel)
        {
            routing_activation_handler_ = new(_tcp_socket_handler, _channel);
            diagnostic_message_handler_ = new(_tcp_socket_handler, _tcp_transport_handler, _channel);
        }

        // Function to trigger Routing activation request
        public TransmissionResult SendRoutingActivationRequest(UdsMessage _message)
        {
            return (routing_activation_handler_.SendRoutingActivationRequest(_message));
        }

        // Function to send Diagnostic request
        public TransmissionResult SendDiagnosticRequest(UdsMessage _message)
        {
            return (diagnostic_message_handler_.SendDiagnosticRequest(_message));
        }

        // process message
        public void HandleMessage(TcpMessageType _tcp_rx_message)
        {
            byte nackCode = Common_doip_types.kDoip_GenericHeader_IncorrectPattern;
            DoipMessage doip_rx_message = new();
            doip_rx_message.Protocol_version = _tcp_rx_message.RxBuffer_[0];
            doip_rx_message.Protocol_version_inv = _tcp_rx_message.RxBuffer_[1];
            doip_rx_message.Payload_type = GetDoIPPayloadType(_tcp_rx_message.RxBuffer_);
            doip_rx_message.Payload_length = GetDoIPPayloadLength(_tcp_rx_message.RxBuffer_);
            // Process the Doip Generic header check
            if (ProcessDoIPHeader(doip_rx_message, ref nackCode))
            {
                //doip_rx_message.payload.resize(tcp_rx_message->rxBuffer_.size() - kDoipheadrSize);
                doip_rx_message.Payload.EnsureCapacity(_tcp_rx_message.RxBuffer_.Count - Common_doip_types.kDoipheadrSize);
                // copy payload locally
                // *** Potential error!!! ***
                //(void)std::copy(tcp_rx_message->rxBuffer_.begin() + kDoipheadrSize,
                //                 tcp_rx_message->rxBuffer_.begin() + kDoipheadrSize + tcp_rx_message->rxBuffer_.size(),
                //                 doip_rx_message.payload.begin());
                doip_rx_message.Payload.AddRange(
                    _tcp_rx_message.RxBuffer_.GetRange(Common_doip_types.kDoipheadrSize, _tcp_rx_message.RxBuffer_.Count
                    - Common_doip_types.kDoipheadrSize)
                );
                ProcessDoIPPayload(doip_rx_message);
            }
            else
            {
                // send NACK or ignore
                // SendDoIPNACKMessage(nackCode);
            }
        }

        // Function to process DoIP Header
        private bool ProcessDoIPHeader(DoipMessage _doip_rx_message, ref byte _nackCode)
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
                if ((_doip_rx_message.Payload_type == Common_doip_types.kDoip_RoutingActivation_ResType) ||
                    (_doip_rx_message.Payload_type == Common_doip_types.kDoip_DiagMessagePosAck_Type) ||
                    (_doip_rx_message.Payload_type == Common_doip_types.kDoip_DiagMessageNegAck_Type) ||
                    (_doip_rx_message.Payload_type == Common_doip_types.kDoip_DiagMessage_Type) ||
                    (_doip_rx_message.Payload_type == Common_doip_types.kDoip_AliveCheck_ReqType))
                {
                    /* Req-[AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00017] */
                    if (_doip_rx_message.Payload_length <= Common_doip_types.kDoip_Protocol_MaxPayload)
                    {
                        /* Req-[AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00018] */
                        if (_doip_rx_message.Payload_length <= Common_doip_types.kTcpChannelLength)
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
                {  // Send NACK code 0x01, discard message
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
        private bool ProcessDoIPPayloadLength(UInt32 payloadLen, UInt16 payloadType)
        {
            bool ret_val = false;
            switch (payloadType)
            {
                case Common_doip_types.kDoip_RoutingActivation_ResType:
                    {
                        if (payloadLen <= (UInt32)Common_doip_types.kDoip_RoutingActivation_ResMaxLen) ret_val = true;
                        break;
                    }
                case Common_doip_types.kDoip_DiagMessagePosAck_Type:
                case Common_doip_types.kDoip_DiagMessageNegAck_Type:
                    {
                        if (payloadLen >= (UInt32)Common_doip_types.kDoip_DiagMessageAck_ResMinLen) ret_val = true;
                        break;
                    }
                case Common_doip_types.kDoip_DiagMessage_Type:
                    {
                        // Req - [20-11][AUTOSAR_SWS_DiagnosticOverIP][SWS_DoIP_00122]
                        if (payloadLen >= (UInt32)(Common_doip_types.kDoip_DiagMessage_ReqResMinLen + 1u)) ret_val = true;
                        break;
                    }
                case Common_doip_types.kDoip_AliveCheck_ReqType:
                    {
                        if (payloadLen <= (UInt32)Common_doip_types.kDoip_RoutingActivation_ResMaxLen) ret_val = true;
                        break;
                    }
                default:
                    // do nothing
                    break;
            }
            return ret_val;
        }

        // Function to get payload type
        private UInt16 GetDoIPPayloadType(List<byte> _payload)
        {
            return ((UInt16)((((UInt16)(_payload[Common_header.BYTE_POS_TWO]) & 0xFF) << 8) |
                    ((UInt16)(_payload[Common_header.BYTE_POS_THREE]) & 0xFF)));
        }

        // Function to get payload length
        private UInt32 GetDoIPPayloadLength(List<byte> _payload)
        {
            return ((UInt32)(((UInt32)(_payload[Common_header.BYTE_POS_FOUR]) << 24) & 0xFF000000) |
                    (UInt32)(((UInt32)(_payload[Common_header.BYTE_POS_FIVE]) << 16) & 0x00FF0000) |
                    (UInt32)(((UInt32)(_payload[Common_header.BYTE_POS_SIX]) << 8) & 0x0000FF00) | 
                    (UInt32)(((UInt32)(_payload[Common_header.BYTE_POS_SEVEN]) & 0x000000FF)));
        }

        // Function to process DoIP payload responses
        private void ProcessDoIPPayload(DoipMessage _doip_payload)
        {
            channel_handler_lock.WaitOne();
            try
            {
                switch (_doip_payload.Payload_type)
                {
                    case Common_doip_types.kDoip_RoutingActivation_ResType:
                        {
                            // Process RoutingActivation response
                            routing_activation_handler_.ProcessDoIPRoutingActivationResponse(_doip_payload);
                            break;
                        }
                    case Common_doip_types.kDoip_DiagMessage_Type:
                        {
                            // Process Diagnostic Message Response
                            diagnostic_message_handler_.ProcessDoIPDiagnosticMessageResponse(_doip_payload);
                            break;
                        }
                    case Common_doip_types.kDoip_DiagMessagePosAck_Type:
                    case Common_doip_types.kDoip_DiagMessageNegAck_Type:
                        {
                            // Process positive or negative diag ack message
                            diagnostic_message_handler_.ProcessDoIPDiagnosticAckMessageResponse(_doip_payload);
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

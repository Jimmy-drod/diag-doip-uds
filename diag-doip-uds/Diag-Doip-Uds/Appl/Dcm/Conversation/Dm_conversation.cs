using SynchronizedTimer;
using Diag_Doip_Uds.Appl.Dcm.Conversation;
using Diag_Doip_Uds.Appl.Dcm.Service;
using Diag_Doip_Uds.Appl.Include;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport.Conversion_manager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
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

    public delegate void Callback();
    /*
    @ Class Name        : DmConversation
    @ Class Description : Class to establish connection with Diagnostic Server
    */
    public sealed class DmConversation : IDiagClientConversation
    {
        // Type for active diagnostic session
        enum SessionControlType : byte
        {
            kDefaultSession = 0x01,
            kProgrammingSession = 0x02,
            kExtendedSession = 0x03,
            kSystemSafetySession = 0x04
        };
        // Type for active security level
        enum SecurityLevelType : byte
        {
            kLocked = 0x00,
            kUnLocked = 0x01,
        };
        // Type of current activity status
        enum ActivityStatusType : byte { kActive = 0x00, kInactive = 0x01 };

        // shared pointer to store the conversion handler
        public ConversionHandler Dm_conversion_handler_ { get; set; }

        // Conversion activity Status
        private ActivityStatusType activity_status_;
        // Dcm session
        private SessionControlType active_session_;
        // Dcm Security
        private SecurityLevelType active_security_;
        // Reception buffer
        private UInt32 rx_buffer_size_;
        // p2 client time
        private UInt16 p2_client_max_;
        // p2 star Client time
        private UInt16 p2_star_client_max_;
        // logical Source address
        private UInt16 source_address_;
        // logical target address
        private UInt16 target_address_;
        // Vehicle broadcast address
        private string broadcast_address = string.Empty;
        // remote Ip Address
        private string remote_address_ = string.Empty;
        // conversion name
        private string conversation_name_;
        // Tp connection
        private Lib.Uds_transport_layer_api.Uds_transport.Connection? connection_ptr_ = null;
        // timer
        private SyncTimer sync_timer_ = new();
        // rx buffer to store the uds response
        private List<byte> payload_rx_buffer = new();
        // conversation state
        private ConversationStateImpl conversation_state_ = new();

        // ctor
        public DmConversation(string _conversation_name, ConversionIdentifierType _conversation_identifier)
        {
            activity_status_ = ActivityStatusType.kInactive;
            active_session_ = SessionControlType.kDefaultSession;
            active_security_ = SecurityLevelType.kLocked;
            rx_buffer_size_ = _conversation_identifier.Rx_buffer_size;
            p2_client_max_ = _conversation_identifier.P2_client_max;
            p2_star_client_max_ = _conversation_identifier.P2_star_client_max;
            source_address_ = _conversation_identifier.Source_address;
            target_address_ = 0;
            conversation_name_ = _conversation_name;
            Dm_conversion_handler_ = new DmConversationHandler(_conversation_identifier.Handler_id, this);
        }

        // startup
        public void Startup()
        {
            if (connection_ptr_ == null) return;

            // initialize the connection
            connection_ptr_.Initialize();
            // start the connection
            connection_ptr_.Start();
            // Change the state to Active
            activity_status_ = ActivityStatusType.kActive;
        }

        // shutdown
        public void Shutdown()
        {
            if (connection_ptr_ == null) return;

            // shutdown connection
            connection_ptr_.Stop();
            // Change the state to InActive
            activity_status_ = ActivityStatusType.kInactive;
        }

        // Description   : Function to connect to Diagnostic Server
        // @param input  : Nothing
        // @return value : ConnectResult
        public ConnectResult ConnectToDiagServer(UInt16 _target_address, IpAddress _host_ip_addr)
        {
            if (connection_ptr_ == null) return ConnectResult.kConnectFailed;

            // create an uds message just to get the port number
            // source address required for Routing Activation
            List<byte> payload = new();  // empty payload
            // Send Connect request to doip layer
            ConnectResult connection_result = (ConnectResult)connection_ptr_.ConnectToHost(
                new DmUdsMessage(source_address_, _target_address, _host_ip_addr, payload)
            );

            remote_address_ = _host_ip_addr;
            target_address_ = _target_address;
            if (connection_result == ConnectResult.kConnectSuccess)
            {
                Console.WriteLine($"'{conversation_name_}'-> Successfully connected to Server with IP <" +
                    $"{remote_address_}> and LA= 0x{target_address_.ToString("X")}");
            }
            else
            {
                Console.WriteLine($"'{conversation_name_}'-> Failed connecting to Server with IP <" +
                    $"{remote_address_}> and LA= 0x{target_address_.ToString("X")}");
            }
            return connection_result;
        }

        // Description   : Function to disconnect from Diagnostic Server
        // @param input  : Nothing
        // @return value : DisconnectResult
        public DisconnectResult DisconnectFromDiagServer()
        {
            if (connection_ptr_ == null) return DisconnectResult.kDisconnectFailed;

            DisconnectResult ret_val = DisconnectResult.kDisconnectFailed;
            // Check if already connected before disconnecting
            if (connection_ptr_.IsConnectToHost())
            {
                // Send disconnect request to doip layer
                ret_val = (DisconnectResult)(connection_ptr_.DisconnectFromHost());
                if (ret_val == DisconnectResult.kDisconnectSuccess)
                {
                    Console.WriteLine($"'{conversation_name_}'-> Successfully disconnected from Server with IP <" +
                    $"{remote_address_}> and LA= 0x{target_address_.ToString("X")}");
                }
                else
                {
                    Console.WriteLine($"'{conversation_name_}'-> Failed to disconnect from Server with IP <" +
                    $"{remote_address_}>");
                }
            }
            else
            {
                ret_val = DisconnectResult.kAlreadyDisconnected;
            }
            return ret_val;
        }

        // Description   : Function to send Diagnostic Request and receive response
        // @param input  : Nothing
        // @return value : DisconnectResult
        public Pair<DiagResult, IUdsMessage> 
        SendDiagnosticRequest(IUdsMessage _message)
        {
            Pair<DiagResult, IUdsMessage> ret_val = new(DiagResult.kDiagGenericFailure, null);

            if (connection_ptr_ == null) return ret_val;

            if (_message != null)
            {
                // fill the data
                List<byte> payload = _message.GetPayload();
                // Initiate Sending of diagnostic request
                TransmissionResult transmission_result = connection_ptr_.Transmit(
                    new DmUdsMessage(source_address_, target_address_, _message.GetHostIpAddress(), payload)
                );
                if (transmission_result == TransmissionResult.kTransmitOk)
                {
                    // Diagnostic Request Sent successful
                    Console.WriteLine($"'{conversation_name_}'-> Diagnostic Request Sent & Positive Ack received");
                    conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagWaitForRes);
                    // Wait P6Max / P2ClientMax
                    WaitForResponse(
                        () => {
                            ret_val.First = DiagResult.kDiagResponseTimeout;
                            conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kIdle);
                            Console.WriteLine($"'{conversation_name_}'-> Diagnostic Response P2 Timeout happened after " +
                            $"{p2_client_max_} milliseconds");
                        },
                        () => {
                            // pending or pos/neg response
                            if (conversation_state_.GetConversationStateContext().GetActiveState().GetState() ==
                                ConversationState.kDiagRecvdFinalRes)
                            {
                                // pos/neg response received
                            }
                            else if (conversation_state_.GetConversationStateContext().GetActiveState().GetState() ==
                                       ConversationState.kDiagRecvdPendingRes)
                            {
                                // first pending received
                                conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagStartP2StarTimer);
                            }
                        },
                        p2_client_max_
                    );

                    // Wait until final response or timeout
                    while (conversation_state_.GetConversationStateContext().GetActiveState().GetState() !=
                           ConversationState.kIdle)
                    {
                        // Check the active state
                        switch (conversation_state_.GetConversationStateContext().GetActiveState().GetState())
                        {
                            case ConversationState.kDiagRecvdPendingRes:
                                conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagStartP2StarTimer);
                                break;
                            case ConversationState.kDiagRecvdFinalRes:
                                // do nothing
                                break;
                            case ConversationState.kDiagStartP2StarTimer:
                                // wait P6Star/ P2 star client time
                                WaitForResponse(
                                    () => {
                                        Console.WriteLine($"'{conversation_name_}'-> Diagnostic Response P2 Star Timeout happened after " +
                                        $"{p2_star_client_max_} milliseconds");
                                        ret_val.First = DiagResult.kDiagResponseTimeout;
                                        conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kIdle);
                                    },
                                    () => {
                                        // pending or pos/neg response
                                        if (conversation_state_.GetConversationStateContext().GetActiveState().GetState() ==
                                            ConversationState.kDiagRecvdFinalRes)
                                        {
                                            // pos/neg response received
                                        }
                                        else if (conversation_state_.GetConversationStateContext().GetActiveState().GetState() ==
                                                   ConversationState.kDiagRecvdPendingRes)
                                        {
                                            // pending received again
                                            conversation_state_.GetConversationStateContext().TransitionTo(
                                                ConversationState.kDiagStartP2StarTimer);
                                        }
                                    },
                                    p2_star_client_max_
                                );
                                break;
                            case ConversationState.kDiagSuccess:
                                // change state to idle, form the uds response and return
                                ret_val.Second = new DmUdsResponse(payload_rx_buffer);
                                ret_val.First = DiagResult.kDiagSuccess;
                                conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kIdle);
                                break;
                            default:
                                // nothing
                                break;
                        }
                    }
                }
                else
                {
                    // failure
                    ret_val.First = ConvertResponseType(transmission_result);
                }
            }
            else
            {
                ret_val.First = DiagResult.kDiagInvalidParameter;
                Console.WriteLine($"'{conversation_name_}'-> Diagnostic Request message is empty");
            }
            return ret_val;
        }

        // Register Connection
        public void RegisterConnection(Lib.Uds_transport_layer_api.Uds_transport.Connection _connection)
        {
            connection_ptr_ = _connection;
        }

        // Indicate message Diagnostic message reception over TCP to user
        public Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                  Address _target_addr,
                                                        TargetAddressType _type, 
                                                                ChannelID _channel_id, 
                                                                      int _size,
                                                                 Priority _priority, 
                                                             ProtocolKind _protocol_kind,
                                                               List<byte> _payload_info)
        {
            Pair<IndicationResult, UdsMessage> ret_val = new(IndicationResult.kIndicationNOk, null);
            // Verify the payload received :-
            if (_payload_info.Count > 0)
            {
                // Check for size, else kIndicationOverflow
                if (_size <= rx_buffer_size_)
                {
                    // Check for pending response
                    // payload = 0x7F XX 0x78
                    if (_payload_info[2] == 0x78)
                    {
                        Console.WriteLine($"'{conversation_name_}'-> Diagnostic pending response received in Conversation");
                        ret_val.First = IndicationResult.kIndicationPending;
                        conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagRecvdPendingRes);
                    }
                    else
                    {
                        Console.WriteLine($"'{conversation_name_}'-> Diagnostic final response received in Conversation");
                        // positive or negative response, provide valid buffer
                        // resize the global rx buffer
                        //payload_rx_buffer.resize(size);
                        ret_val.First = IndicationResult.kIndicationOk;
                        ret_val.Second = new DmUdsMessage(source_address_, target_address_, "", payload_rx_buffer);
                        conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagRecvdFinalRes);
                    }
                    WaitCancel();
                }
                else
                {
                    Console.WriteLine($"'{conversation_name_}'-> Diagnostic Conversation Error Indication Overflow");
                    ret_val.First = IndicationResult.kIndicationOverflow;
                }
            }
            else
            {
                Console.WriteLine($"'{conversation_name_}'-> Diagnostic Conversation Rx Payload size 0 received");
            }
            return ret_val;
        }

        // Hands over a valid message to conversion
        public void HandleMessage(UdsMessage _message)
        {
            if (_message != null)
            {
                conversation_state_.GetConversationStateContext().TransitionTo(ConversationState.kDiagSuccess);
            }
        }

        // Function to wait for response
        private void WaitForResponse(Callback _timeout_func, Callback _cancel_func, int _msec)
        {
            if (sync_timer_.Start(_msec) == TimerState.kTimeout)
            {
                _timeout_func();
            }
            else
            {
                _cancel_func();
            }
        }

        // Function to cancel the synchronous wait
        private void WaitCancel()
        {
            sync_timer_.Stop();
        }

        public static DiagResult ConvertResponseType(TransmissionResult _result_type)
        {
            DiagResult ret_result = DiagResult.kDiagGenericFailure;
            switch (_result_type)
            {
                case TransmissionResult.kTransmitFailed:
                    ret_result = DiagResult.kDiagRequestSendFailed;
                    break;
                case TransmissionResult.kNoTransmitAckReceived:
                    ret_result = DiagResult.kDiagAckTimeout;
                    break;
                case TransmissionResult.kNegTransmitAckReceived:
                    ret_result = DiagResult.kDiagNegAckReceived;
                    break;
                case TransmissionResult.kBusyProcessing:
                    ret_result = DiagResult.kDiagBusyProcessing;
                    break;
                default:
                    ret_result = DiagResult.kDiagGenericFailure;
                    break;
            }
            return ret_result;
        }
    };

    /*
    @ Class Name        : DmConversationHandler
    @ Class Description : Class to establish connection with Diagnostic Server
    */
    public class DmConversationHandler : ConversionHandler
    {
        private DmConversation dm_conversation_;
        // ctor
        public DmConversationHandler(ConversionHandlerID _handler_id, DmConversation _dm_conversion)
            : base(_handler_id)
        {
            dm_conversation_ = _dm_conversion;
        }

        // Indicate message Diagnostic message reception over TCP to user
        public override Pair<IndicationResult, UdsMessage> IndicateMessage(Address _source_addr, 
                                                                           Address _target_addr,
                                                                 TargetAddressType _type, 
                                                                         ChannelID _channel_id, 
                                                                               int _size,
                                                                          Priority _priority, 
                                                                      ProtocolKind _protocol_kind,
                                                                        List<byte> _payload_info)
        {
            return (dm_conversation_.IndicateMessage(_source_addr, _target_addr, _type, _channel_id, _size, _priority, _protocol_kind,
                                           _payload_info));
        }

        // Hands over a valid message to conversion
        public override void HandleMessage(UdsMessage _message)
        {
            dm_conversation_.HandleMessage(_message);
        }
    };
}

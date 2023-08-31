using Diag_Doip_Uds.Lib.Doip_client.Common;
using Diag_Doip_Uds.Lib.Doip_client.Handler;
using Diag_Doip_Uds.Lib.Doip_client.Sockets;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp;
using SynchronizedTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    public delegate void Callback();
    //  socket state
    public enum tcpSocketState : byte { kSocketOffline = 0, kSocketOnline };
    public class TcpChannel
    {
        // tcp socket handler
        private TcpSocketHandler tcp_socket_handler_;
        // tcp socket state
        private tcpSocketState tcp_socket_state_ = tcpSocketState.kSocketOffline;
        // tcp channel state
        private TcpChannelStateImpl tcp_channel_state_ = new();
        // tcp channel handler
        private TcpChannelHandlerImpl tcp_channel_handler_;
        // sync timer
        private SyncTimer sync_timer_ = new();

        //ctor
        public TcpChannel(string _localIpaddress, TcpTransportHandler _tcp_transport_handler)
        {
            tcp_socket_handler_ = new TcpSocketHandler(_localIpaddress, this);
            tcp_socket_state_ = tcpSocketState.kSocketOffline;
            tcp_channel_handler_ = new(tcp_socket_handler_, _tcp_transport_handler, this);
        }

        // Initialize
        public InitializationResult Initialize()
        {
            return (InitializationResult.kInitializeOk);
        }

        //Start
        public void Start()
        {
            tcp_socket_handler_.Start();
        }

        // Stop
        public void Stop()
        {
            if (tcp_socket_state_ == tcpSocketState.kSocketOnline)
            {
                tcp_socket_handler_.Stop();
                if (tcp_socket_handler_.DisconnectFromHost()) { tcp_socket_state_ = tcpSocketState.kSocketOffline; }
            }
        }

        // Check if already connected to host
        public bool IsConnectToHost()
        {
            return (tcp_socket_state_ == tcpSocketState.kSocketOnline);
        }

        // Function to connect to host
        public ConnectionResult ConnectToHost(UdsMessage _message)
        {
            ConnectionResult ret_val = ConnectionResult.kConnectionFailed;
            if (tcp_socket_state_ == tcpSocketState.kSocketOffline)
            {
                // sync connect to change the socket state
                if (tcp_socket_handler_.ConnectToHost(_message.GetHostIpAddress(), _message.GetHostPortNumber()))
                {
                    // set socket state, tcp connection established
                    tcp_socket_state_ = tcpSocketState.kSocketOnline;
                }
                else
                {  // failure
                    Console.WriteLine($"Doip Tcp socket connect failed for remote endpoints : " +
                        $"<Ip :{_message.GetHostIpAddress()}, Port : {_message.GetHostPortNumber()}>");
                }
            }
            else
            {
                // socket already online
                Console.WriteLine($"Doip Tcp socket already connected");
            }
            // If socket online, send routing activation req and get response
            if (tcp_socket_state_ == tcpSocketState.kSocketOnline)
            {
                // send routing activation request and get response
                ret_val = HandleRoutingActivationState(_message);
            }
            return ret_val;
        }

        // Function to disconnect from host
        public DisconnectionResult DisconnectFromHost()
        {
            DisconnectionResult ret_val = DisconnectionResult.kDisconnectionFailed;
            if (tcp_socket_state_ == tcpSocketState.kSocketOnline)
            {
                if (tcp_socket_handler_.DisconnectFromHost())
                {
                    tcp_socket_state_ = tcpSocketState.kSocketOffline;
                    if (tcp_channel_state_.GetRoutingActivationStateContext().GetActiveState().GetState() ==
                        routingActivationState.kRoutingActivationSuccessful)
                    {
                        // reset previous routing activation
                        tcp_channel_state_.GetRoutingActivationStateContext().TransitionTo(routingActivationState.kIdle);
                    }
                    ret_val = DisconnectionResult.kDisconnectionOk;
                }
            }
            else
            {
                Console.WriteLine($"Doip Tcp socket already in not connected state");
            }
            return ret_val;
        }

        // Function to Hand over all the message received
        public void HandleMessage(TcpMessageType _tcp_rx_message)
        {
            tcp_channel_handler_.HandleMessage(_tcp_rx_message);
        }

        // Function to trigger transmission
        public TransmissionResult Transmit(UdsMessage _message)
        {
            TransmissionResult ret_val = TransmissionResult.kTransmitFailed;
            if (tcp_socket_state_ == tcpSocketState.kSocketOnline)
            {
                // routing activation should be active before sending diag request
                if (tcp_channel_state_.GetRoutingActivationStateContext().GetActiveState().GetState() ==
                    routingActivationState.kRoutingActivationSuccessful)
                {
                    ret_val = HandleDiagnosticRequestState(_message);
                }
                else
                {
                    Console.WriteLine($"Routing Activation required, please connect to server first");
                }
            }
            else
            {
                Console.WriteLine($"Socket Offline, please connect to server first");
            }
            return ret_val;
        }

        // Function to get the channel context
        public TcpChannelStateImpl GetChannelState() { return tcp_channel_state_; }

        // Function to wait for response
        public void WaitForResponse(Callback _timeout_func, Callback _cancel_func, int _msec)
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
        public void WaitCancel()
        {
            sync_timer_.Stop();
        }

        // Function to handle the routing states
        private ConnectionResult HandleRoutingActivationState(UdsMessage _message)
        {
            ConnectionResult result = ConnectionResult.kConnectionFailed;
            if (tcp_channel_state_.GetRoutingActivationStateContext().GetActiveState().GetState() ==
                routingActivationState.kIdle)
            {
                if (tcp_channel_handler_.SendRoutingActivationRequest(_message) ==
                    TransmissionResult.kTransmitOk)
                {
                    tcp_channel_state_.GetRoutingActivationStateContext().TransitionTo(
                        routingActivationState.kWaitForRoutingActivationRes);
                    WaitForResponse(
                    () => {
                        // todo : make sure result be assigned correctly
                        ref ConnectionResult inner_result = ref result;
                        inner_result = ConnectionResult.kConnectionTimeout;
                        tcp_channel_state_.GetRoutingActivationStateContext().TransitionTo(routingActivationState.kIdle);
                        Console.WriteLine($"RoutingActivation response timeout, no response received in : " +
                            $"{Common_doip_types.kDoIPRoutingActivationTimeout} milliseconds");
                    },
                    () => {
                        if (tcp_channel_state_.GetRoutingActivationStateContext().GetActiveState().GetState() ==
                            routingActivationState.kRoutingActivationSuccessful)
                        {
                            // success
                            // todo : make sure result be assigned correctly
                            ref ConnectionResult inner_result = ref result;
                            inner_result = ConnectionResult.kConnectionOk;
                            Console.WriteLine($"RoutingActivation successful with remote server");
                        }
                        else
                        {  // failed
                            tcp_channel_state_.GetRoutingActivationStateContext().TransitionTo(
                                routingActivationState.kIdle);
                            Console.WriteLine($"RoutingActivation failed with remote server");
                        }
                    },
                    (int)Common_doip_types.kDoIPRoutingActivationTimeout);
                }
                else
                {
                    // failed, do nothing
                    tcp_channel_state_.GetRoutingActivationStateContext().TransitionTo(routingActivationState.kIdle);
                    Console.WriteLine($"RoutingActivation Request send failed with remote server");
                }
            }
            else
            {
                // channel not free
                Console.WriteLine($"RoutingActivation channel not free");
            }
            return result;
        }

        // Function to handle the diagnostic request response state
        private TransmissionResult HandleDiagnosticRequestState(UdsMessage _message)
        {
            TransmissionResult result = TransmissionResult.kTransmitFailed;
            if (tcp_channel_state_.GetDiagnosticMessageStateContext().GetActiveState().GetState() ==
                diagnosticState.kDiagIdle)
            {
                if (tcp_channel_handler_.SendDiagnosticRequest(_message) ==
                    TransmissionResult.kTransmitOk)
                {
                    tcp_channel_state_.GetDiagnosticMessageStateContext().TransitionTo(
                        diagnosticState.kWaitForDiagnosticAck);
                    WaitForResponse(
                    () => {
                        ref TransmissionResult inner_result = ref result;
                        inner_result = TransmissionResult.kNoTransmitAckReceived;
                        tcp_channel_state_.GetDiagnosticMessageStateContext().TransitionTo(
                            diagnosticState.kDiagIdle);
                        Console.WriteLine($"Diagnostic Message Ack Request timed out, no response received" +
                            $" in: {Common_doip_types.kDoIPDiagnosticAckTimeout} seconds");
                    },
                    () => {
                        if (tcp_channel_state_.GetDiagnosticMessageStateContext().GetActiveState().GetState() ==
                            diagnosticState.kDiagnosticPositiveAckRecvd)
                        {
                            tcp_channel_state_.GetDiagnosticMessageStateContext().TransitionTo(
                                diagnosticState.kWaitForDiagnosticResponse);
                            // success
                            ref TransmissionResult inner_result = ref result;
                            inner_result = TransmissionResult.kTransmitOk;
                            Console.WriteLine($"Diagnostic Message Positive Ack received");
                        }
                        else
                        {
                            // failed with neg acknowledgement from server
                            ref TransmissionResult inner_result = ref result;
                            inner_result = TransmissionResult.kNegTransmitAckReceived;
                            tcp_channel_state_.GetDiagnosticMessageStateContext().TransitionTo(
                                diagnosticState.kDiagIdle);
                            Console.WriteLine($"Diagnostic Message Transmission Failed Neg Ack Received");
                        }
                    },
                    (int)Common_doip_types.kDoIPDiagnosticAckTimeout);
                }
                else
                {
                    // failed, do nothing
                    tcp_channel_state_.GetDiagnosticMessageStateContext().TransitionTo(diagnosticState.kDiagIdle);
                    Console.WriteLine($"Diagnostic Request Message Transmission Failed");
                }
            }
            else
            {
                // channel not in idle state
                result = TransmissionResult.kBusyProcessing;
                Console.WriteLine($"Diagnostic Message Transmission already in progress");
            }
            return result;
        }
    }
}

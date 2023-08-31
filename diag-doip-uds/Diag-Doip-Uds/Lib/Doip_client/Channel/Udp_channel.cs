using Diag_Doip_Uds.Lib.Doip_client.Handler;
using Diag_Doip_Uds.Lib.Doip_client.Sockets;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Udp;
using SynchronizedTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    /*
    @ Class Name        : UdpChannel
    @ Class Description : Class used to handle Doip Udp Channel
    */
    public class UdpChannel
    {
        // udp transport handler ref
        private UdpTransportHandler udp_transport_handler_;
        // udp socket handler broadcast
        private UdpSocketHandler udp_socket_handler_bcast_;
        // udp socket handler unicast
        private UdpSocketHandler udp_socket_handler_ucast_;
        // udp channel state
        private UdpChannelStateImpl udp_channel_state_ = new();
        // udp channel handler
        private UdpChannelHandlerImpl udp_channel_handler_;
        // Executor
        //private TaskExecutor task_executor_;  //todo : 封装一个执行器
        // sync timer
        private SyncTimer sync_timer_ = new();
        //ctor
        public UdpChannel(string _local_ip_address, UInt16 _port_num, UdpTransportHandler _udp_transport_handler)
        {
            udp_transport_handler_ = _udp_transport_handler;
            udp_socket_handler_bcast_ = new UdpSocketHandler(
                _local_ip_address, _port_num, PortType.kUdp_Broadcast, this);
            udp_socket_handler_ucast_ = new UdpSocketHandler(
                _local_ip_address, _port_num, PortType.kUdp_Unicast, this);
            udp_channel_handler_ = new(udp_socket_handler_bcast_, udp_socket_handler_ucast_, udp_transport_handler_, this);
        }

        // Initialize
        public InitializationResult Initialize()
        {
            InitializationResult ret_val = InitializationResult.kInitializeOk;
            return ret_val;
        }

        //Start
        public void Start()
        {
            udp_socket_handler_bcast_.Start();
            udp_socket_handler_ucast_.Start();
        }

        // Stop
        public void Stop()
        {
            udp_socket_handler_bcast_.Stop();
            udp_socket_handler_ucast_.Stop();
        }

        // function to handle read broadcast
        public void HandleMessageBroadcast(UdpMessageType _udp_rx_message)
        {
            udp_channel_handler_.HandleMessageBroadcast(_udp_rx_message);
        }

        // function to handle read unicast
        public void HandleMessageUnicast(UdpMessageType _udp_rx_message)
        {
            udp_channel_handler_.HandleMessage(_udp_rx_message);
        }

        // Function to trigger transmission of vehicle identification request
        public TransmissionResult Transmit(UdsMessage _message)
        {
            return udp_channel_handler_.Transmit(_message);
        }

        // Function to get the channel context
        public UdpChannelStateImpl GetChannelState() { return udp_channel_state_; }

        // Function to add job to executor
        public void SendVehicleInformationToUser()
        {
        }

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
    };
}

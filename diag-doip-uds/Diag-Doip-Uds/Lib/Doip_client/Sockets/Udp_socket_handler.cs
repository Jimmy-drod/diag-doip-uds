using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Sockets
{
    public class UdpSocketHandler
    {
        // local Ip address
        private string local_ip_address_ = string.Empty;
        // Host Ip address
        private string host_ip_address_ = string.Empty;
        // Host port number
        private UInt16 port_num_;
        // Port type
        private PortType port_type_;
        // udp socket
        private CreateUdpClientSocket udp_socket_;
        // store tcp channel reference
        private UdpChannel channel_;

        //ctor
        public UdpSocketHandler(string _local_ip_address, UInt16 _port_num, PortType _port_type,
                         UdpChannel _channel)
        {
            local_ip_address_ = _local_ip_address;
            port_num_ = _port_num;
            port_type_ = _port_type;
            channel_ = _channel;
            // create sockets and start receiving
            if (_port_type == PortType.kUdp_Broadcast)
            {
                udp_socket_ = new CreateUdpClientSocket(
                    local_ip_address_, port_num_, port_type_,
                    (UdpMessageType _udp_rx_message) => { channel_.HandleMessageBroadcast(_udp_rx_message); });
            }
            else
            {
                udp_socket_ = new CreateUdpClientSocket(
                    local_ip_address_, port_num_, port_type_,
                    (UdpMessageType _udp_rx_message) => { channel_.HandleMessageUnicast(_udp_rx_message); });
            }
        }

        //start
        public void Start() { udp_socket_.Open(); }

        //stop
        public void Stop() { udp_socket_.Destroy(); }

        // function to trigger transmission
        public bool Transmit(UdpMessageType _udp_tx_message)
        {
            return (udp_socket_.Transmit(_udp_tx_message));
        }
    }
}

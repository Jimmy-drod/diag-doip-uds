using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Sockets
{
    public class TcpSocketHandler
    {
        // local Ip address
        private string local_ip_address_ = string.Empty;
        // local port number
        private UInt16 local_port_num_;
        // tcp socket
        private CreateTcpClientSocket tcpSocket_;
        // store tcp channel reference
        private TcpChannel channel_;

        // ctor
        public TcpSocketHandler(string _localIpaddress, TcpChannel _channel)
        {
            local_ip_address_ = _localIpaddress;
            local_port_num_ = 0;
            channel_ = _channel;
            //create socket
            tcpSocket_ = new CreateTcpClientSocket(local_ip_address_, local_port_num_, 
                (TcpMessageType _tcpMessage) => {channel_.HandleMessage(_tcpMessage);
            });
        }

        // Start
        public void Start()
        {
        }

        // Stop
        public void Stop()
        {
        }

        // Connect to host
        public bool ConnectToHost(string _host_ip_address, UInt16 _host_port_num)
        {
            bool ret_val = false;
            if (tcpSocket_.Open()) { ret_val = tcpSocket_.ConnectToHost(_host_ip_address, _host_port_num); }
            return ret_val;
        }

        // Disconnect from host
        public bool DisconnectFromHost()
        {
            bool ret_val = false;
            if (tcpSocket_.DisconnectFromHost()) { ret_val = tcpSocket_.Destroy(); }
            return ret_val;
        }

        // Transmit function
        public bool Transmit(TcpMessageType _tcp_message)
        {
            return (tcpSocket_.Transmit(_tcp_message));
        }
    }
}

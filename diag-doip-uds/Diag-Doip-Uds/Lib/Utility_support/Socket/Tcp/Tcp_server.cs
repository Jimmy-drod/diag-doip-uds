using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp.CreateTcpServerSocket;
using System.Net.WebSockets;

namespace Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp
{
    // Tcp function template used for reception
    public delegate void TcpHandlerRead(TcpMessageType message);
    // Tcp Server connection class to create connection with client
    public class TcpServerConnection
    {
        // ctor
        public TcpServerConnection(TcpHandlerRead _tcp_handler_read)
        {
            tcp_handler_read_ = _tcp_handler_read;
        }

        // Get reference to underlying socket
        public System.Net.Sockets.Socket? GetSocket() { return tcp_socket_; }
        public void SetSocket(System.Net.Sockets.Socket _socket) { tcp_socket_ = _socket; }

        // function to transmit tcp message
        public bool Transmit(TcpMessageType _tcp_tx_message)
        {
            if(tcp_socket_ != null)
            {
                tcp_socket_.Send(_tcp_tx_message.TxBuffer_.ToArray());
                return true;
            }
            else
            {
                return false;
            }
        }

        // function to handle read
        public bool ReceivedMessage()
        {
            bool connection_closed = false;
            TcpMessageType tcp_rx_message = new();
            int read_next_bytes;

            try
            {
                if (tcp_socket_ != null)
                {
                    // start blocking read to read Header first
                    byte[] buffer = new byte[TcpMessageType.kDoipheadrSize];
                    if (tcp_socket_.Receive(buffer) == TcpMessageType.kDoipheadrSize)
                    {
                        tcp_rx_message.RxBuffer_.AddRange(buffer.ToList());
                        read_next_bytes = (int)((buffer[4] << 24) & 0xFF000000) |
                                          (int)((buffer[5] << 16) & 0x00FF0000) |
                                          (int)((buffer[6] << 8) & 0x0000FF00) |
                                          (int)((buffer[7] & 0x000000FF));

                        buffer = new byte[read_next_bytes];
                        if (tcp_socket_.Receive(buffer) == read_next_bytes)
                        {
                            tcp_rx_message.RxBuffer_.AddRange(buffer.ToList());

                            // all message received, transfer to upper layer
                            IPEndPoint? endPoint = tcp_socket_.RemoteEndPoint as IPEndPoint;
                            if (endPoint != null)
                            {
                                tcp_rx_message.Host_ip_address_ = endPoint.Address.ToString();
                                tcp_rx_message.Host_port_num_ = (ushort)endPoint.Port;

                                // send data to upper layer
                                tcp_handler_read_(tcp_rx_message);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                connection_closed = true;
            }
            return connection_closed;
        }

        // function to close the socket
        public bool Shutdown()
        {
            bool ret_val = false;

            if(tcp_socket_ != null)
            {
                tcp_socket_.Shutdown(SocketShutdown.Both);
                tcp_socket_.Close();
                tcp_socket_ = null;
            }

            return ret_val;
        }

        // tcp socket
        private System.Net.Sockets.Socket? tcp_socket_ = null;

        // handler read
        private TcpHandlerRead tcp_handler_read_;
    }

    public class CreateTcpServerSocket
    {
        // local Ip address
        private string local_ip_address_ = string.Empty;
        // local port number
        private UInt16 local_port_num_;
        // server socket
        private System.Net.Sockets.Socket server_socket_;
        public CreateTcpServerSocket(string _local_ip_address, UInt16 _local_port_num)
        {
            local_ip_address_ = _local_ip_address;
            local_port_num_ = _local_port_num;

            server_socket_ = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, local_port_num_);
            server_socket_.Bind(endPoint);
            server_socket_.Listen(10);//设置监听最大值
        }

        // Blocking function get a tcp connection
        public TcpServerConnection GetTcpServerConnection(TcpHandlerRead _tcp_handler_read)
        {
            TcpServerConnection tcp_connection = new(_tcp_handler_read);
            //var client = tcp_connection.GetSocket();
            var client = server_socket_.Accept();
            tcp_connection.SetSocket(client);
            return tcp_connection;
        }
    }
}

using Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Utility_support.Socket.Udp
{
    // Udp function template used for reception
    public delegate void UdpHandlerRead(UdpMessageType message);
    // Port Type
    public enum PortType : byte { kUdp_Broadcast = 0, kUdp_Unicast };
    public class CreateUdpClientSocket
    {
        // local Ip address
        private string local_ip_address_ = string.Empty;
        // local port number
        private UInt16 local_port_num_;
        // udp socket
        private System.Net.Sockets.Socket? udp_socket_;
        //private UdpClient udpClient_;

        // flag to terminate the thread
        //通过Interlocked.Exchange原子化读写，exit_request_调整为int型，0表示false，1表示true
        private /*bool*/int exit_request_;
        // flag th start the thread
        //通过Interlocked.Exchange原子化读写，running_调整为int型，0表示false，1表示true
        private /*bool*/int running_;
        // conditional variable to block the thread
        //private std::condition_variable cond_var_;
        // threading var
        //private Thread thread_;
        // locking critical section
        //private Mutex mutex_;
        // end points
        private EndPoint remote_endpoint_;
        // port type - broadcast / unicast
        private PortType port_type_;
        // Handler invoked during read operation
        private UdpHandlerRead udp_handler_read_;
        // Rx buffer
        private byte[] rxbuffer_ = new byte[UdpMessageType.kDoipUdpResSize];

        // ctor
        public CreateUdpClientSocket(string _local_ip_address, UInt16 _local_port_num, PortType port_type,
                              UdpHandlerRead udp_handler_read)
        {
            local_ip_address_ = _local_ip_address;
            local_port_num_ = _local_port_num;
            Interlocked.Exchange(ref exit_request_, 0);
            Interlocked.Exchange(ref running_, 0);
            port_type_ = port_type;
            udp_handler_read_ = udp_handler_read;
            remote_endpoint_ = new IPEndPoint(IPAddress.Any, 0);
            udp_socket_ = null;
            //udp_socket_ = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        // Function to Open the socket
        public bool Open()
        {
            bool retVal = false;
            try
            {
                udp_socket_ = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                // set broadcast option
                udp_socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                // reuse address
                udp_socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                if (port_type_ == PortType.kUdp_Broadcast)
                {
                    // Todo : change the hardcoded value of port number 13400
                    udp_socket_.Bind(new IPEndPoint(IPAddress.Any, 13400));
                }
                else
                {
                    //bind to local address and random port
                    udp_socket_.Bind(new IPEndPoint(IPAddress.Parse(local_ip_address_), local_port_num_));
                }

                IPEndPoint? endpoint = udp_socket_.LocalEndPoint as IPEndPoint;

                if (endpoint == null)
                {
                    Console.WriteLine("Udp Socket Bind failed");
                    return false;
                }

                Console.WriteLine($"Udp Socket Opened and bound to <{endpoint.Address.ToString()}, {endpoint.Port}>");
                // Update the port number with new one
                local_port_num_ = (ushort)endpoint.Port;
                // start reading
                Interlocked.Exchange(ref exit_request_, 1);
                //cond_var_.notify_all();
                retVal = true;
                // start async receive
                StartReceiving();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return retVal;
        }

        private void StartReceiving()
        {
            // 接收数据缓冲区
            byte[] receiveData = new byte[1024];

            if (udp_socket_ == null) return;
            // 开始异步接收数据
            udp_socket_.BeginReceiveFrom(receiveData, 0, receiveData.Length, SocketFlags.None, ref remote_endpoint_, HandleMessage, receiveData);
        }

        // Transmit
        public bool Transmit(UdpMessageType _udp_message)
        {
            bool ret_val = false;
            if (udp_socket_ == null) return ret_val;
            try
            {
                // Transmit to remote endpoints
                int send_size = udp_socket_.SendTo(_udp_message.Tx_buffer_.ToArray(), 
                    new IPEndPoint(IPAddress.Parse(_udp_message.Host_ip_address_), _udp_message.Host_port_num_));

                // Check for error
                if (send_size == _udp_message.Tx_buffer_.Count)
                {
                    // successful
                    IPEndPoint? endpoint = udp_socket_.LocalEndPoint as IPEndPoint;
                    if (endpoint == null)
                    {
                        Console.WriteLine("LocalEndPoint null");
                        return ret_val;
                    }
                    Console.WriteLine($"Udp message sent : <{endpoint.Address.ToString()}, {endpoint.Port}> -> " +
                        $"<{_udp_message.Host_ip_address_}, {_udp_message.Host_port_num_}>");
                    ret_val = true;
                    // start async receive
                    StartReceiving();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Udp message sending to <{_udp_message.Host_ip_address_}> " +
                    $"failed with error : {e.Message.ToString()}");
            }

            return ret_val;
        }

        // Function to destroy the socket
        public bool Destroy()
        {
            if (udp_socket_ != null)
            {
                Interlocked.Exchange(ref running_, 0);
                udp_socket_.Shutdown(SocketShutdown.Both);
                udp_socket_.Close();
                udp_socket_ = null;
            }
            return true;
        }
        // function invoked when datagram is received
        //private void HandleMessage(UdpErrorCodeType &error, std::size_t bytes_recvd)
        private void HandleMessage(IAsyncResult ar)
        {
            try
            {
                if (udp_socket_ == null || ar.AsyncState == null) return;

                // 结束异步接收，并获取接收到的数据长度
                int bytesReceived = udp_socket_.EndReceiveFrom(ar, ref remote_endpoint_);
                var remote_endpoint = (remote_endpoint_ as IPEndPoint);

                if (remote_endpoint == null)
                {
                    Console.WriteLine("remote_endpoint == null");
                    return;
                }

                if (!local_ip_address_ .Equals(remote_endpoint.Address.ToString()))
                {
                    UdpMessageType udp_rx_message = new UdpMessageType();
                    // Copy the data
                    byte[] receiveData = (byte[])ar.AsyncState;
                    //string receivedMessage = Encoding.ASCII.GetString(receiveData, 0, bytesReceived);
                    udp_rx_message.Rx_buffer_.AddRange(receiveData);
                    // fill the remote endpoints
                    udp_rx_message.Host_ip_address_ = remote_endpoint.Address.ToString();
                    udp_rx_message.Host_port_num_ = (ushort)remote_endpoint.Port;

                    // all message received, transfer to upper layer
                    //IPEndPoint? remote_endpoint = remote_endpoint_ as IPEndPoint;
                    //IPEndPoint? local_endpoint = udp_socket_.LocalEndPoint as IPEndPoint;

                    Console.WriteLine($"Udp Message received: <{remote_endpoint.Address.ToString()},{remote_endpoint.Port}>" +
                        $"-><{local_ip_address_},{local_port_num_}>");

                    // send data to upper layer
                    udp_handler_read_(udp_rx_message);
                    // start async receive
                    StartReceiving();
                }
                else
                {
                    Console.WriteLine($"Udp Message received from <{remote_endpoint.Address.ToString()}, {remote_endpoint.Port}>" +
                        $"ignored as received by self ip <{local_ip_address_}>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while receiving data: " + ex.Message);
            }
        }
    }
}

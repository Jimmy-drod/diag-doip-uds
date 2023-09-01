using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Diag_Doip_Uds.Lib.Doip_client.Common;

namespace Diag_Doip_Uds.Lib.Utility_support.Socket.Tcp
{
    public class CreateTcpClientSocket
    {
        // local Ip address
        private string local_ip_address_ = string.Empty;
        // local port number
        private UInt16 local_port_num_;
        // tcp socket
        private System.Net.Sockets.Socket? tcp_socket_;
        // flag to terminate the thread
        //private bool exit_request_;
        // flag th start the thread
        //private bool running_;
        // conditional variable to block the thread
        //private std::condition_variable cond_var_;
        // threading var
        private Thread? thread_ = null;
        // locking critical section
        //private Mutex mutex_;
        // Handler invoked during read operation
        private TcpHandlerRead tcp_handler_read_;
        //通过Interlocked.Exchange原子化读写，is_connected调整为int型，0表示false，1表示true
        private /*bool*/int is_connected;

        //ctor
        public CreateTcpClientSocket(string _local_ip_address, UInt16 _local_port_num, TcpHandlerRead _tcp_handler_read)
        {
            local_ip_address_ = _local_ip_address;
            local_port_num_ = _local_port_num;
            tcp_handler_read_ = _tcp_handler_read;
            tcp_socket_ = null;
            Interlocked.Exchange(ref is_connected, 0);
        }

        // Function to Open the socket
        public bool Open()
        {
            bool retVal = false;
            try
            {
                // 创建 TCP Socket
                tcp_socket_ = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 设置 Socket 为非阻塞模式
                tcp_socket_.Blocking = false;

                // 设置 Socket 选项，允许地址重用
                tcp_socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // 绑定到本地地址和随机端口
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(local_ip_address_), local_port_num_);
                tcp_socket_.Bind(localEndPoint);

                Console.WriteLine($"Tcp Socket opened and bound to <{localEndPoint.Address.ToString()}, {localEndPoint.Port}>");
                retVal = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return retVal;
        }

        // Function to Connect to host
        public bool ConnectToHost(string _host_ip_address, UInt16 _host_port_num)
        {
            bool ret_val = false;

            if (tcp_socket_ == null) return ret_val;
            try
            {
                IAsyncResult result = tcp_socket_.BeginConnect(IPAddress.Parse(_host_ip_address), _host_port_num, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(2000); // 等待2秒
                if (success)
                {
                    tcp_socket_.EndConnect(result);
                    // 启动接收数据的线程
                    Interlocked.Exchange(ref is_connected, 1);
                    thread_ = new Thread(HandleMessage);
                    thread_.Start();
                    ret_val = true;
                }
                else
                {
                    Console.WriteLine($"Tcp Socket connect to <{_host_ip_address}, {_host_port_num}> failed");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Tcp Socket connect to <{_host_ip_address}, {_host_port_num}> failed");
            }
            return ret_val;
        }

        // Function to Disconnect from host
        public bool DisconnectFromHost()
        {
            bool ret_val = false;

            if (tcp_socket_ != null)
            {
                tcp_socket_.Shutdown(SocketShutdown.Both);

                Interlocked.Exchange(ref is_connected, 0);
                thread_?.Join();
                ret_val = true;
            }

            return ret_val;
        }

        // Function to trigger transmission
        public bool Transmit(TcpMessageType _tcpMessage)
        {
            bool ret_val = false;
            if (tcp_socket_ == null) return ret_val;
            try
            {
                var send_length = tcp_socket_.Send(_tcpMessage.TxBuffer_.ToArray());
                if(send_length == _tcpMessage.TxBuffer_.Count)
                {
                    var remote_endpoint = tcp_socket_.RemoteEndPoint as IPEndPoint;
                    if(remote_endpoint != null)
                    {
                        Console.WriteLine($"Tcp message sent to {remote_endpoint.Address.ToString()}, {remote_endpoint.Port}>");
                        ret_val = true;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Tcp message sending failed with error : {ex.Message.ToString()}");
            }
            return ret_val;
        }

        // Function to destroy the socket
        public bool Destroy()
        {
            if (tcp_socket_ != null)
            {
                tcp_socket_.Shutdown(SocketShutdown.Both);
                tcp_socket_.Close();
                tcp_socket_ = null;

                Interlocked.Exchange(ref is_connected, 0);
                thread_?.Join();
            }
            return true;
        }
        // function to handle read
        private void HandleMessage()
        {
            if (tcp_socket_ == null) return;
            byte[] receiveBuffer = new byte[1024];

            while (is_connected == 1)
            {
                TcpMessageType tcp_rx_message = new();
                try
                {
                    if (tcp_socket_.Poll(50000, SelectMode.SelectRead))
                    {
                        //todo : make sure bytesRead > Common_doip_types.kDoipheadrSize
                        int bytesRead = tcp_socket_.Receive(receiveBuffer, Common_doip_types.kDoipheadrSize, 0);
                        if (bytesRead == Common_doip_types.kDoipheadrSize)
                        {
                            tcp_rx_message.RxBuffer_.AddRange(receiveBuffer.Take(Common_doip_types.kDoipheadrSize));
                            UInt32 read_next_bytes = ((UInt32)((UInt32)((tcp_rx_message.RxBuffer_[4] << 24) & 0xFF000000) |
                                  (UInt32)((tcp_rx_message.RxBuffer_[5] << 16) & 0x00FF0000) |
                                  (UInt32)((tcp_rx_message.RxBuffer_[6] << 8) & 0x0000FF00) |
                                  (UInt32)((tcp_rx_message.RxBuffer_[7] & 0x000000FF))));

                            //continue reading payload
                            Array.Resize(ref receiveBuffer, (int)read_next_bytes);
                            Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                            bytesRead = tcp_socket_.Receive(receiveBuffer, (int)read_next_bytes, 0);
                            if (bytesRead == read_next_bytes)
                            {
                                tcp_rx_message.RxBuffer_.AddRange(receiveBuffer.Take((int)read_next_bytes));

                                // all message received, transfer to upper layer
                                IPEndPoint? endpoint = tcp_socket_.RemoteEndPoint as IPEndPoint;
                                if (endpoint == null) return;
                                // fill the remote endpoints
                                tcp_rx_message.Host_ip_address_ = endpoint.Address.ToString();
                                tcp_rx_message.Host_port_num_ = (ushort)endpoint.Port;

                                Console.WriteLine($"Tcp Message received from <{endpoint.Address.ToString()}," +
                                    $"{endpoint.Port}>");
                                // send data to upper layer
                                tcp_handler_read_(tcp_rx_message);
                            }
                            else
                            {
                                Console.WriteLine($"Do not read data with payload length");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Do not read data with length Common_doip_types.kDoipheadrSize");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
                //Thread.Sleep(50);
            }
        }
    }
}

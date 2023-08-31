using Diag_Doip_Uds.Appl.Include;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds
{
    using IpAddress = String;
    public class UdsMessageImpl : IUdsMessage
    {
        // host ip address
        private IpAddress host_ip_address_;

        // store only UDS payload to be sent
        private List<byte> uds_payload_;
        // ctor
        public UdsMessageImpl(string _host_ip_address, List<byte> _payload)
        {
            host_ip_address_ = _host_ip_address;
            uds_payload_ = _payload;
        }
        public List<byte> GetPayload()
        {
            return uds_payload_;
        }
        public string GetHostIpAddress()
        {
            return host_ip_address_;
        }
    }
}

using Diag_Doip_Uds.Appl.Include;
using Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Dcm.Service
{
    using Address = UInt16;
    using PortNumber = UInt16;
    using MetaInfoMap = Dictionary<string, string>;
    // Ip address
    using IpAddress = String;
    public sealed class DmUdsMessage : UdsMessage
    {
        // ctor
        public DmUdsMessage(Address _sa, Address _ta, IpAddress _host_ip_address, List<byte> _payload)
            : base()
        {
            source_address_ = _sa;
            target_address_ = _ta;
            target_address_type_ = TargetAddressType.kPhysical;
            host_ip_address_ = _host_ip_address;
            uds_payload_ = _payload;
        }
        // SA
        private Address source_address_;

        // TA
        private Address target_address_;

        // TA type
        private TargetAddressType target_address_type_;

        // Host Ip Address
        private string host_ip_address_;

        // store only UDS payload to be sent
        private List<byte> uds_payload_;

        // add new metaInfo to this message.
        public override void AddMetaInfo(MetaInfoMap _meta_info) 
        {
            // Todo [Add meta info information]
        }

        // Get the UDS message data starting with the SID (A_Data as per ISO)
        public override List<byte> GetPayload(){ return uds_payload_; }

        // Get the source address of the uds message.
        public override Address GetSa() { return source_address_; }

        // Get the target address of the uds message.
        public override Address GetTa() { return target_address_; }

        // Get the target address type (phys/func) of the uds message.
        public override TargetAddressType GetTaType() { return target_address_type_; }

        // Get Host Ip address
        public override IpAddress GetHostIpAddress() { return host_ip_address_; }

        // Get Host port number
        public override PortNumber GetHostPortNumber() { return 13400; }
    };

    public sealed class DmUdsResponse :  IUdsMessage
    {
        public DmUdsResponse(List<byte> _payload)
        {
            uds_payload_ = _payload;
        }
        // store only UDS payload to be sent
        private List<byte> uds_payload_;
        // Host Ip Address
        IpAddress host_ip_address_ = string.Empty;

        // Get the UDS message data starting with the SID (A_Data as per ISO)
        public List<byte> GetPayload() { return uds_payload_; }

        // Get Host Ip address
        public IpAddress GetHostIpAddress(){ return host_ip_address_; }
    }
}

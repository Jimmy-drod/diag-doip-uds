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
    public sealed class VdMessage : UdsMessage
    {
        // ctor
        public VdMessage(byte _preselection_mode, List<byte> _preselection_value,
        string _host_ip_address) : base()
        {
            source_address_ = 0;
            target_address_ = 0;
            target_address_type = TargetAddressType.kPhysical;
            host_ip_address_ = _host_ip_address;
            vehicle_info_payload_ = SerializeVehicleInfoList(_preselection_mode, _preselection_value);
        }

        // default ctor
        public VdMessage() : base()
        {
            source_address_ = 0;
            target_address_ = 0;
            target_address_type = TargetAddressType.kPhysical;
            host_ip_address_ = string.Empty;
            vehicle_info_payload_ = new();
        }

        private List<byte> SerializeVehicleInfoList(byte _preselection_mode, List<byte> _preselection_value)
        {
            byte VehicleIdentificationHandler = 0;
            List<byte> payload = new List<byte> { VehicleIdentificationHandler, _preselection_mode };
            payload.AddRange(_preselection_value);
            return payload;
        }

        // SA
        private Address source_address_;

        // TA
        private Address target_address_;

        // TA type
        private TargetAddressType target_address_type;

        // Host Ip Address
        private IpAddress host_ip_address_;

        // store the vehicle info payload
        private List<byte> vehicle_info_payload_;

        // store the
        private MetaInfoMap meta_info_ = new();

        // add new metaInfo to this message.
        public override void AddMetaInfo(MetaInfoMap _meta_info)
        {
            // update meta info data
            if (_meta_info != null) {
                meta_info_ = _meta_info;
                host_ip_address_ = meta_info_["kRemoteIpAddress"];
            }
        }

        // Get the UDS message data starting with the SID (A_Data as per ISO)
        public override List<byte> GetPayload()  { return vehicle_info_payload_; }

        // Get the source address of the uds message.
        public override Address GetSa() { return source_address_; }

        // Get the target address of the uds message.
        public override Address GetTa() { return target_address_; }

        // Get the target address type (phys/func) of the uds message.
        public override TargetAddressType GetTaType() { return target_address_type; }

        // Get Host Ip address
        public override IpAddress GetHostIpAddress() { return host_ip_address_; }
        // Get Host port number
        public override PortNumber GetHostPortNumber() { return 13400; }
    };
}

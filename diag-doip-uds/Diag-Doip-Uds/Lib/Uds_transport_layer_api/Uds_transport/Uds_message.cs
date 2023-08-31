using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Uds_transport_layer_api.Uds_transport
{
    using Address = UInt16;
    using PortNumber = UInt16;
    using MetaInfoMap = Dictionary<string, string>;
    using IpAddress = String;
    public enum TargetAddressType : byte
    {
        kPhysical = 0,
        kFunctional = 1,
    }
    public abstract class UdsMessage
    {
        public UdsMessage() { }

        // add new metaInfo to this message.
        public abstract void AddMetaInfo(MetaInfoMap _meta_info);

        // Get the UDS message data starting with the SID (A_Data as per ISO)
        public abstract List<byte> GetPayload();

        // return the underlying buffer for write access
        //public abstract List<byte> GetPayload();

        // Get the source address of the uds message.
        public abstract Address GetSa();

        // Get the target address of the uds message.
        public abstract Address GetTa();

        // Get the target address type (phys/func) of the uds message.
        public abstract TargetAddressType GetTaType();

        // Get Host Ip address
        public abstract IpAddress GetHostIpAddress();

        // Get Host port number
        public abstract PortNumber GetHostPortNumber();
    }
}

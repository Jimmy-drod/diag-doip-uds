using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Include
{
    using IpAddress = String;
    /**
    * @brief    Class represents an UDS message exchanged between User of diag-client-lib and implementation of
    *           diag-client-lib on diagnostic request reception path or diagnostic response transmission path.
    *           UdsMessage provides the storage for UDS requests/responses.
    */
    public interface IUdsMessage
    {
        /**
        * @brief        Get the UDS message data starting with the SID (A_Data as per ISO)
        * @return       const ByteVector&
        *               The entire payload (A_Data)
        */
        public List<byte> GetPayload();

        /**
        * @brief        Get the remote ip address present
        * @return       IpAddress
        *               Ip address stored
        */
        public IpAddress GetHostIpAddress();
    };
}

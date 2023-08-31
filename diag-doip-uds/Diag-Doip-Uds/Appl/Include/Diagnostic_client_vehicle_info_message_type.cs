using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Include
{
    /**
    * @brief       Alias to collection of Vehicle info response
    */
    using VehicleInfoListResponseType = List<VehicleAddrInfoResponse>;
    /**
     * @brief       Structure containing available Vehicle Address Information
     */
    public class VehicleAddrInfoResponse
    {
        /**
         * @brief       IP address of the vehicle
         */
        public string Ip_address { get; set; } = string.Empty;

        /**
         * @brief       Logical address of the vehicle
         */
        public UInt16 Logical_address { get; set; }

        /**
         * @brief       VIN of the vehicle
         */
        public string VIN { get; set; } = string.Empty;

        /**
         * @brief       Entity Identification of the vehicle
         */
        public string EID { get; set; } = string.Empty;

        /**
         * @brief       Group Identification of the vehicle
         */
        public string GID { get; set; } = string.Empty;
    };

    /**
    * @brief       Struct containing Vehicle selection mode.
    */
    public class VehicleAddrInfoRequest
    {
        /**
         * @brief     Mode to be used during sending of Vehicle Identification request.
         *            0U : No preselection
         *            1U : DoIP Entities with given VIN
         *            2U : DoIP Entities with given EID
         */
        public byte Preselection_mode { get; set; } = 0;

        /**
         * @brief     Value to be used based on preselection mode.
         *            VIN when preselection_mode = 1U
         *            EID when preselection_mode = 2U
         *            Empty when preselection_mode = 0U
         */
        public string Preselection_value { get; set; } = string.Empty;
    };
    
    /**
    * @brief       Class provide storage of list of all available vehicle entity
    */
    public interface IVehicleInfoMessage
    {
        /**
        * @brief       Function to get the list of vehicle available in the network.
        * @return      VehicleInfoListResponseType
        *              Result returned
        */
        public VehicleInfoListResponseType GetVehicleList();
    };
}

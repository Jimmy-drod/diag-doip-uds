using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Include
{
    public class DiagClientHelper
    {
        /**
        * @brief       Function to get the instance of Diagnostic Client Object.
        *              This instance to be further used for all the functionalities.
        * @param[in]   diag_client_config_path
        *              path to diag client config file
        * @return      std::unique_ptr<diag::client::DiagClient>
        *              Unique pointer to diag client object
        * @remarks     Implemented requirements:
        *              DiagClientLib-Library-Support, DiagClientLib-ComParam-Settings
        */
        public static IDiagClient CreateDiagnosticClient(string _diag_client_config_path)
        {
            return new DiagClientImpl(_diag_client_config_path);
        }
    }
}

using Diag_Doip_Uds.Appl.Include;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Diag_Doip_Uds.Lib.Common.Common_header;

namespace Diag_Doip_Uds.Appl.Common
{
    /*
    @ Class Name        : DiagnosticManager
    @ Class Description : Parent class to create DCM and DEM class
    */
    public abstract class DiagnosticManager : IDisposable
    {
        // flag to terminate the main thread
        private bool exit_requested_;
        // conditional variable to block the thread
        private AutoResetEvent auto_reset_event = new(false);
        // For locking critical section of code
        private Mutex mutex_ = new();

        //ctor
        public DiagnosticManager() { exit_requested_ = false; }
        public void Dispose()
        {
            mutex_.WaitOne();
            try
            {
                exit_requested_ = true;
            }
            finally
            {
                mutex_.ReleaseMutex();
            }
            auto_reset_event.Set();
        }

        // main function
        public virtual void Main()
        {
            // Initialize the module
            Initialize();
            // Run the module
            Run();
            // Entering infinite loop
            while (!exit_requested_)
            {
                // Thread exited
                //mutex_.WaitOne();
                try
                {
                    auto_reset_event.WaitOne();
                }
                finally
                {
                    //mutex_.ReleaseMutex();
                }
            }
            // Shutdown Module
            Shutdown();
        }

        // signal shutdown
        public virtual void SignalShutdown()
        {
            mutex_.WaitOne();
            try
            {
                exit_requested_ = true;
            }
            finally
            {
                mutex_.ReleaseMutex();
            }
            auto_reset_event.Set();
        }

        // Initialize
        public abstract void Initialize();

        // Run
        public abstract void Run();

        // Shutdown
        public abstract void Shutdown();

        // Function to get the diagnostic client conversation
        public abstract IDiagClientConversation
        GetDiagnosticClientConversation( string _conversation_name);

        // Send Vehicle Identification Request and get response
        public abstract Pair<VehicleResponseResult,IVehicleInfoMessage>
        SendVehicleIdentificationRequest(VehicleAddrInfoRequest _vehicle_info_request);
    }
}

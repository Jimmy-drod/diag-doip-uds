using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Utility_support.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    // Vehicle discovery state
    public enum VehicleDiscoveryState : byte { kVdIdle = 0, kWaitForVehicleAnnouncement, kVdDoIPCtrlTimeout };
    // Vehicle Identification state
    public enum VehicleIdentificationState : byte
    {
        kViIdle = 0,
        kViSendVehicleIdentificationReq,
        kViWaitForVehicleIdentificationRes,
        kViDoIPCtrlTimeout
    };

    public class UdpChannelStateImpl
    {
        // routing activation state
        private StateContext<VehicleDiscoveryState> vehicle_discovery_state_context_;
        // diagnostic state
        private StateContext<VehicleIdentificationState> vehicle_identification_state_context_;
        // ctor
        public UdpChannelStateImpl()
        {
            vehicle_discovery_state_context_ = new StateContext<VehicleDiscoveryState>();
            vehicle_identification_state_context_ = new StateContext<VehicleIdentificationState>();

            // create and add state for vehicle discovery
            // kVdIdle
            GetVehicleDiscoveryStateContext().AddState(
                VehicleDiscoveryState.kVdIdle,
                new kVdIdle(VehicleDiscoveryState.kVdIdle));
            // kWaitForVehicleAnnouncement
            GetVehicleDiscoveryStateContext().AddState(
                VehicleDiscoveryState.kWaitForVehicleAnnouncement,
                new kWaitForVehicleAnnouncement(VehicleDiscoveryState.kWaitForVehicleAnnouncement));
            // kVdDoIPCtrlTimeout
            GetVehicleDiscoveryStateContext().AddState(
                VehicleDiscoveryState.kVdDoIPCtrlTimeout,
                new kVdDoIPCtrlTimeout(VehicleDiscoveryState.kVdDoIPCtrlTimeout));
            // Transit to idle
            GetVehicleDiscoveryStateContext().TransitionTo(VehicleDiscoveryState.kVdIdle);

            // create and add state for vehicle identification
            // kVdIdle
            GetVehicleIdentificationStateContext().AddState(
                VehicleIdentificationState.kViIdle, 
                new kViIdle(VehicleIdentificationState.kViIdle));
            // kViSendVehicleIdentificationReq
            GetVehicleIdentificationStateContext().AddState(
                VehicleIdentificationState.kViSendVehicleIdentificationReq,
                new kViSendVehicleIdentificationReq(VehicleIdentificationState.kViSendVehicleIdentificationReq));
            // kViWaitForVehicleIdentificationRes
            GetVehicleIdentificationStateContext().AddState(
                VehicleIdentificationState.kViWaitForVehicleIdentificationRes,
                new kViWaitForVehicleIdentificationRes(VehicleIdentificationState.kViWaitForVehicleIdentificationRes));
            // kViDoIPCtrlTimeout
            GetVehicleIdentificationStateContext().AddState(
                VehicleIdentificationState.kViDoIPCtrlTimeout,
                new kViDoIPCtrlTimeout(VehicleIdentificationState.kViDoIPCtrlTimeout));
            // Transit to idle
            GetVehicleIdentificationStateContext().TransitionTo(VehicleIdentificationState.kViIdle);
        }

        // Function to get Vehicle Discovery State context
        public StateContext<VehicleDiscoveryState> GetVehicleDiscoveryStateContext()
        {
            return vehicle_discovery_state_context_;
        }

        // Function to get Vehicle Identification State context
        public StateContext<VehicleIdentificationState> GetVehicleIdentificationStateContext()
        {
            return vehicle_identification_state_context_;
        }
    };

    public sealed class kVdIdle : State<VehicleDiscoveryState>
    {
        // ctor
        public kVdIdle(VehicleDiscoveryState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kWaitForVehicleAnnouncement : State<VehicleDiscoveryState>
    {
        // ctor
        public kWaitForVehicleAnnouncement(VehicleDiscoveryState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kVdDoIPCtrlTimeout : State<VehicleDiscoveryState>
    {
        // ctor
        public kVdDoIPCtrlTimeout(VehicleDiscoveryState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kViIdle : State<VehicleIdentificationState>
    {
        // ctor
        public kViIdle(VehicleIdentificationState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kViSendVehicleIdentificationReq : State<VehicleIdentificationState>
    {
        // ctor
        public kViSendVehicleIdentificationReq(VehicleIdentificationState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kViWaitForVehicleIdentificationRes : State<VehicleIdentificationState>
    {
        // ctor
        public kViWaitForVehicleIdentificationRes(VehicleIdentificationState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };

    public sealed class kViDoIPCtrlTimeout : State<VehicleIdentificationState>
    {
        // ctor
        public kViDoIPCtrlTimeout(VehicleIdentificationState _state) : base(_state)
        {
        }

        // start the state
        public override void Start()
        {
        }

        // Stop the state
        public override void Stop()
        {
        }

        // Handle invoked asynchronously
        public override void HandleMessage()
        {
        }
    };
}

using Diag_Doip_Uds.Lib.Doip_client.Channel;
using Diag_Doip_Uds.Lib.Utility_support.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Channel
{
    // routing activation state
    public enum routingActivationState : byte
    {
        kIdle = 0,
        kWaitForRoutingActivationRes,
        kRoutingActivationSuccessful,
        kRoutingActivationFailed
    };
    // Diagnostic state
    public enum diagnosticState : byte
    {
        kDiagIdle = 0,
        kSendDiagnosticReqFailed,
        kWaitForDiagnosticAck,
        kDiagnosticPositiveAckRecvd,
        kDiagnosticNegativeAckRecvd,
        kWaitForDiagnosticResponse,
        kDiagnosticFinalResRecvd
    };

    public class TcpChannelStateImpl
    {
        // ctor
        public TcpChannelStateImpl()
        {
            routing_activation_state_context_ = new StateContext<routingActivationState>();
            diagnostic_message_state_context_ = new StateContext<diagnosticState>();

            // create and add state for routing activation
            // kIdle
            GetRoutingActivationStateContext().AddState(
                routingActivationState.kIdle,
                new kIdle(routingActivationState.kIdle));
            // kWaitForRoutingActivationRes
            GetRoutingActivationStateContext().AddState(
                routingActivationState.kWaitForRoutingActivationRes,
                new kWaitForRoutingActivationRes(routingActivationState.kWaitForRoutingActivationRes));
            // kRoutingActivationSuccessful
            GetRoutingActivationStateContext().AddState(
                routingActivationState.kRoutingActivationSuccessful,
                new kRoutingActivationSuccessful(routingActivationState.kRoutingActivationSuccessful));
            // kRoutingActivationFailed
            GetRoutingActivationStateContext().AddState(
                routingActivationState.kRoutingActivationFailed,
                new kRoutingActivationFailed(routingActivationState.kRoutingActivationFailed));
            // transit to idle state
            GetRoutingActivationStateContext().TransitionTo(routingActivationState.kIdle);

            // create and add state for Diagnostic State
            // kDiagIdle
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kDiagIdle,
                new kDiagIdle(diagnosticState.kDiagIdle));
            // kWaitForDiagnosticAck
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kWaitForDiagnosticAck,
                new kWaitForDiagnosticAck(diagnosticState.kWaitForDiagnosticAck));
            // kSendDiagnosticReqFailed
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kSendDiagnosticReqFailed,
                new kSendDiagnosticReqFailed(diagnosticState.kSendDiagnosticReqFailed));
            // kDiagnosticPositiveAckRecvd
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kDiagnosticPositiveAckRecvd,
                new kDiagnosticPositiveAckRecvd(diagnosticState.kDiagnosticPositiveAckRecvd));
            // kDiagnosticNegativeAckRecvd
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kDiagnosticNegativeAckRecvd,
                new kDiagnosticNegativeAckRecvd(diagnosticState.kDiagnosticNegativeAckRecvd));
            // kWaitForDiagnosticResponse
            GetDiagnosticMessageStateContext().AddState(
                diagnosticState.kWaitForDiagnosticResponse,
                new kWaitForDiagnosticResponse(diagnosticState.kWaitForDiagnosticResponse));
            // transit to idle state
            GetDiagnosticMessageStateContext().TransitionTo(diagnosticState.kDiagIdle);
        }

        // Function to get the Routing Activation State context
        public StateContext<routingActivationState> GetRoutingActivationStateContext()
        {
            return routing_activation_state_context_;
        }

        // Function to get Diagnostic Message State context
        public StateContext<diagnosticState> GetDiagnosticMessageStateContext()
        {
            return diagnostic_message_state_context_;
        }

        // routing activation state
        private StateContext<routingActivationState> routing_activation_state_context_;
        // diagnostic state
        private StateContext<diagnosticState> diagnostic_message_state_context_;
    };

    public sealed class kIdle : State<routingActivationState>
    {
        // ctor
        public kIdle(routingActivationState _state) : base(_state)
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

    public sealed class kWaitForRoutingActivationRes : State<routingActivationState>
    {
        // ctor
        public kWaitForRoutingActivationRes(routingActivationState _state) : base(_state)
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

    public sealed class kRoutingActivationSuccessful : State<routingActivationState>
    {
        // ctor
        public kRoutingActivationSuccessful(routingActivationState _state) : base(_state)
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

    public sealed class kRoutingActivationFailed : State<routingActivationState>
    {
        // ctor
        public kRoutingActivationFailed(routingActivationState _state) : base(_state)
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

    public sealed class kDiagIdle : State<diagnosticState>
    {
        // ctor
        public kDiagIdle(diagnosticState _state) : base(_state)
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

    public sealed class kWaitForDiagnosticAck : State<diagnosticState>
    {
        // ctor
        public kWaitForDiagnosticAck(diagnosticState _state) : base(_state)
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

    public sealed class kSendDiagnosticReqFailed : State<diagnosticState>
    {
        // ctor
        public kSendDiagnosticReqFailed(diagnosticState _state) : base(_state)
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

    public sealed class kDiagnosticPositiveAckRecvd : State<diagnosticState>
    {
        // ctor
        public kDiagnosticPositiveAckRecvd(diagnosticState _state) : base(_state)
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

    public sealed class kDiagnosticNegativeAckRecvd : State<diagnosticState>
    {
        // ctor
        public kDiagnosticNegativeAckRecvd(diagnosticState _state) : base(_state)
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

    public sealed class kWaitForDiagnosticResponse : State<diagnosticState>
    {
        // ctor
        public kWaitForDiagnosticResponse(diagnosticState _state) : base(_state)
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

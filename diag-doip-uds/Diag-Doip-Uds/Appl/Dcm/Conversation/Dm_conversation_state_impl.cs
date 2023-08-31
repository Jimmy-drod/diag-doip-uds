using Diag_Doip_Uds.Appl.Dcm.Conversation;
using Diag_Doip_Uds.Lib.Utility_support.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Appl.Dcm.Conversation
{
    // Conversation States
    public enum ConversationState : byte
    {
        kIdle = 0x00,
        kDiagWaitForRes,
        kDiagStartP2StarTimer,
        kDiagRecvdPendingRes,
        kDiagRecvdFinalRes,
        kDiagSuccess,
    };

    public class ConversationStateImpl
    {
        // conversation state
        private StateContext<ConversationState> conversation_state_;
        // ctor
        public ConversationStateImpl()
        {
            conversation_state_ = new();
            // create and add state
            // kIdle
            GetConversationStateContext().AddState(ConversationState.kIdle,
                                                   new kIdle(ConversationState.kIdle));
            // kDiagWaitForRes
            GetConversationStateContext().AddState(ConversationState.kDiagWaitForRes,
                                                   new kDiagWaitForRes(ConversationState.kDiagWaitForRes));
            // kDiagStartP2StarTimer
            GetConversationStateContext().AddState(ConversationState.kDiagStartP2StarTimer,
                                                   new kDiagStartP2StarTimer(ConversationState.kDiagStartP2StarTimer));
            // kDiagRecvdPendingRes
            GetConversationStateContext().AddState(ConversationState.kDiagRecvdPendingRes,
                                                   new kDiagRecvdPendingRes(ConversationState.kDiagRecvdPendingRes));
            // kDiagRecvdFinalRes
            GetConversationStateContext().AddState(ConversationState.kDiagRecvdFinalRes,
                                                   new kDiagRecvdFinalRes(ConversationState.kDiagRecvdFinalRes));
            // kDiagSuccess
            GetConversationStateContext().AddState(ConversationState.kDiagSuccess,
                                                   new kDiagSuccess(ConversationState.kDiagSuccess));
            // transit to idle state
            GetConversationStateContext().TransitionTo(ConversationState.kIdle);
        }

        // Function to get the Conversation State context
        public StateContext<ConversationState> GetConversationStateContext()
        {
            return conversation_state_;
        }
    };

    public sealed class kIdle : State<ConversationState>
    {
        // ctor
        public kIdle(ConversationState _state) : base(_state)
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

    public sealed class kDiagWaitForRes : State<ConversationState>
    {
        // ctor
        public kDiagWaitForRes(ConversationState _state) : base(_state)
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

    public sealed class kDiagStartP2StarTimer : State<ConversationState>
    {
        // ctor
        public kDiagStartP2StarTimer(ConversationState _state) : base(_state)
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

    public sealed class kDiagRecvdPendingRes : State<ConversationState>
    {
        // ctor
        public kDiagRecvdPendingRes(ConversationState _state) : base(_state)
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

    public sealed class kDiagRecvdFinalRes : State<ConversationState>
    {
        // ctor
        public kDiagRecvdFinalRes(ConversationState _state) : base(_state)
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

    public sealed class kDiagSuccess : State<ConversationState>
    {
        // ctor
        public kDiagSuccess(ConversationState _state) : base(_state)
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

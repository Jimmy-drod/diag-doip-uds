using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Utility_support.Utility
{
    public abstract class State<EnumState>
    {
        protected EnumState state_;
        public State(EnumState _state)
        {
            state_ = _state;
        }
        public abstract void Start();
        public abstract void Stop();
        public abstract void HandleMessage();
        public EnumState GetState() { return state_; }
    }

    public class StateContext<EnumState> where EnumState : struct
    {
        // mutex to protect transition
        private static Mutex state_mutex_ = new();
        // pointer to store the active state
        private State<EnumState> current_state_;
        // mapping of state to state ref
        private Dictionary<EnumState, State<EnumState>> state_map_ = new();
        public StateContext()
        {
            current_state_ = null;
        }

        // Add the needed state
        public void AddState(EnumState _state, State<EnumState> _state_ptr)
        {
            state_map_.Add(_state, _state_ptr);
        }

        // Get the current state
        public State<EnumState> GetActiveState()
        {
            state_mutex_.WaitOne();
            try
            {
                return current_state_;
            }
            finally
            {
                state_mutex_.ReleaseMutex();
            }
        }

        // Function to transition state to provided state
        public void TransitionTo(EnumState state)
        {
            // stop the current state
            Stop();
            // Update to new state
            Update(state);
            // Start new state
            Start();
        }

        // Get Context
        public StateContext<EnumState> GetContext() { return this; }

        // Start the current state
        private void Start()
        {
            if (current_state_ != null) { current_state_.Start(); }
        }

        // Stop the current state
        private void Stop()
        {
            if (current_state_ != null) { current_state_.Stop(); }
        }

        // Update to new state
        private void Update(EnumState state)
        {
            state_mutex_.WaitOne();
            try
            {
                if (state_map_.ContainsKey(state))
                {
                    current_state_ = state_map_[state];
                }
                else
                {
                    // failure condition
                }
            }
            finally
            {
                state_mutex_.ReleaseMutex();
            }
        }
    }
}

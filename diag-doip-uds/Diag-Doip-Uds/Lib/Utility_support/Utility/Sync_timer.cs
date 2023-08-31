using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizedTimer
{
    // timer state
    public enum TimerState : byte { kIdle = 0, kCancelRequested, kTimeout };
    public class SyncTimer : IDisposable
    {
        private AutoResetEvent reset_event_ = new(false);
        private Timer? timer_ = null;
        private bool exit_request_;
        private bool start_running_;

        public SyncTimer()
        {
            exit_request_ = false;
            start_running_ = false;
        }

        public void Dispose()
        {
            try
            {
                exit_request_ = false;
                start_running_ = false;
                reset_event_.Set();
            }
            finally
            {
            }
        }

        public TimerState Start(int _msec)
        {
            try
            {
                TimerState timer_state = TimerState.kIdle;
                start_running_ = true;
                long expiry_timepoint = DateTime.Now.Ticks + _msec * 10000; //100ns
                timer_ = new Timer(TimerCallback, null, TimeSpan.FromMilliseconds(_msec), Timeout.InfiniteTimeSpan);
                reset_event_.WaitOne();
                timer_.Dispose();
                if (!exit_request_)
                {
                    long current_time = DateTime.Now.Ticks; //100ns
                    // check for expiry
                    if (current_time > expiry_timepoint)
                    {
                        // timeout
                        timer_state = TimerState.kTimeout;
                    }
                    else
                    {
                        if (!start_running_)
                        {
                            timer_state = TimerState.kCancelRequested;
                        }
                    }
                }
                return timer_state;
            }
            finally
            {
            }
        }

        public void Stop()
        {
            try
            {
                start_running_ = false;
                reset_event_.Set();
            }
            finally
            {
            }
        }

        private void TimerCallback(object? state)
        {
            reset_event_.Set();
        }
    }
}

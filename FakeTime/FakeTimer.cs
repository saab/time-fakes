using System;
using System.Timers;

namespace Saab.Time.Fakes
{
    public class FakeTimer : ITimer
    {
        private bool disposed;
        private double totalMillisecondsElapsed;

        private volatile int totalTicks;

        private volatile bool enabled;

        private double interval;

        public bool AutoReset { get; set; } = true;

        public bool Enabled
        {
            get => enabled;

            set
            {
                if (enabled != value)
                {
                    enabled = value;

                    // If stopping then reset.
                    if (!value)
                    {
                        totalTicks = 0;
                        totalMillisecondsElapsed = 0;
                    }
                }
            }
        }

        public double Interval
        {
            get => interval;
            set
            {
                if (value <= 0.0 || value > int.MaxValue)
                {
                    throw new ArgumentException($"The value must be greater than zero, and less than or equal to {int.MaxValue}");
                }

                interval = value;
            }
        }

        public event ElapsedEventHandler Elapsed;

        public event EventHandler Disposing;

        public void Advance(TimeSpan advanceInterval)
        {
            if (Enabled)
            {
                // Calculate the total elapsed time.
                totalMillisecondsElapsed += advanceInterval.TotalMilliseconds;

                // Calculate the number of events to trigger.
                // Note that the whole interval so far is used to ensure boundary conditions are satisfied
                // (the number of ticks already triggered is then deducted).
                var ticksToProgress = (int)Math.Floor(totalMillisecondsElapsed / Interval) - totalTicks;

                // Trigger the events.
                for (var i = 0; i < ticksToProgress; i++)
                {
                    // If we're still enabled
                    if (Enabled)
                    {
                        // Cache the number of events triggered. Do it here in case Timer enables/disables itself during callback.
                        totalTicks++;

                        if (!AutoReset)
                        {
                            Enabled = false;
                        }

                        // Note null arguments (this differs from a real timer).
                        Elapsed?.Invoke(this, null);
                    }
                }
            }
        }

        public TimeSpan TimeUntilNextTick
        {
            get
            {
                if (Enabled)
                {
                    var millisecondsAtLastTick = totalMillisecondsElapsed - (totalTicks * Interval);
                    return TimeSpan.FromMilliseconds(Interval - millisecondsAtLastTick);
                }
                else
                {
                    return TimeSpan.MaxValue;
                }
            }
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
            disposed = true;
        }

        public void Start()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(FakeTimer));
            }

            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }
    }
}
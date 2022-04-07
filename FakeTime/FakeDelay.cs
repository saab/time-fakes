using System;
using System.Threading;
using System.Threading.Tasks;

namespace Saab.Time
{
    public class FakeDelay
    {
        private readonly TaskCompletionSource<bool> tcs;

        public FakeDelay(TimeSpan delay, CancellationToken ct)
        {
            TimeUntilComplete = delay;
            tcs = new TaskCompletionSource<bool>();

            // Intentionally do not dispose the registration when FakeTime is disposed.
            // Otherwise the order in which things are disposed can cause the test to hang
            // due to delays not throwing when their cancellation token is cancelled.
            ct.Register(() =>
                {
                    tcs.TrySetCanceled();
                });
        }

        public EventHandler Completed;

        public TimeSpan TimeUntilComplete { get; set; }

        public Task Task { get => tcs.Task; }

        public void Advance(TimeSpan timeElapsed)
        {
            if (TimeUntilComplete <= timeElapsed)
            {
                tcs.TrySetResult(true);
                Completed?.Invoke(this, EventArgs.Empty);
            }

            TimeUntilComplete -= timeElapsed;
        }
    }
}
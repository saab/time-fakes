using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Saab.Time.Fakes
{
    public class FakeTime : IDisposable
    {
        private readonly List<FakeTimer> activeTimers = new List<FakeTimer>();
        private readonly List<FakeDelay> activeDelays = new List<FakeDelay>();

        public ITimer CreateTimer()
        {
            var timer = new FakeTimer();
            timer.Disposing += RemoveTimerFromActiveList;
            activeTimers.Add(timer);
            return timer;
        }

        public Task DelayAsync(TimeSpan delay, CancellationToken ct = default)
        {
            if (delay.TotalSeconds == 0)
            {
                return Task.CompletedTask;
            }

            var fakeDelay = new FakeDelay(delay, ct);
            activeDelays.Add(fakeDelay);
            fakeDelay.Completed += (s, e) =>
                {
                    activeDelays.Remove(fakeDelay);
                };

            return fakeDelay.Task;
        }

        public DateTime CurrentTime { get; set; } = new DateTime(2020, 10, 27, 12, 00, 00);

        public void AdvanceTime(TimeSpan timeSpan)
        {
            var timeRemaining = timeSpan;

            // Make sure we tick one timer at a time
            while (timeRemaining > TimeSpan.Zero)
            {
                // Take a copy of activeTimers since timers may disable themselves during their execution which would modify the list
                var localActiveTimers = activeTimers.ToArray();
                var localActiveDelays = activeDelays.ToArray();

                var nextTimerTick = localActiveTimers.Length > 0 ? localActiveTimers.Min(x => x.TimeUntilNextTick) : timeRemaining;
                var nextDelayTick = localActiveDelays.Length > 0 ? localActiveDelays.Min(x => x.TimeUntilComplete) : timeRemaining;
                var timeUntilNextEvent = TimeSpan.FromMilliseconds(Math.Min(nextTimerTick.TotalMilliseconds, nextDelayTick.TotalMilliseconds));
                if (timeUntilNextEvent > timeRemaining)
                {
                    timeUntilNextEvent = timeRemaining;
                }

                CurrentTime += timeUntilNextEvent;
                foreach (var timer in localActiveTimers)
                {
                    timer.Advance(timeUntilNextEvent);
                }

                foreach (var delay in localActiveDelays)
                {
                    delay.Advance(timeUntilNextEvent);
                }

                timeRemaining -= timeUntilNextEvent;
            }
        }

        public void Dispose()
        {
            activeDelays.Clear();
            activeTimers.Clear();
        }

        public ICancellationTokenSource CreateCancellationTokenSource(TimeSpan timeout)
        {
            return new FakeCancellationTokenSource(CreateTimer, timeout);
        }

        private void RemoveTimerFromActiveList(object sender, EventArgs e)
        {
            var timer = (FakeTimer)sender;
            activeTimers.Remove(timer);
            timer.Disposing -= RemoveTimerFromActiveList;
        }
    }
}
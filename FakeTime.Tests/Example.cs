using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace Saab.Time.Fakes.Tests
{
    public class Example
    {
        private readonly ILogger<Example> logger;
        private readonly UtcNow utcNow;
        private readonly CreateTimer createTimer;
        private readonly TaskDelay taskDelay;
        private readonly CreateCancellationTokenSource createCancellationTokenSource;

        public Example(ILogger<Example> logger, UtcNow utcNow, CreateTimer createTimer, TaskDelay taskDelay, CreateCancellationTokenSource createCancellationTokenSource)
        {
            this.logger = logger;
            this.utcNow = utcNow;
            this.createTimer = createTimer;
            this.taskDelay = taskDelay;
            this.createCancellationTokenSource = createCancellationTokenSource;
        }

        public async Task ExecuteAsync(TimeSpan timerTickPeriod, TimeSpan cancellationTimeout, TimeSpan delayTimeout, CancellationToken ct = default)
        {
            logger.LogTrace($"Started at {utcNow()}");

            // Using a Timer
            using var timer = createTimer();
            timer.Interval = timerTickPeriod.TotalMilliseconds;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            // Using a CancellationTokenSource
            using var cts = createCancellationTokenSource(cancellationTimeout);
            var tcs = new TaskCompletionSource<object>();
            using (cts.Token.Register(() => tcs.TrySetResult(new object())))
            {
                await tcs.Task;

                logger.LogTrace($"Task was cancelled at {utcNow()}");
            }

            // Using TaskDelay
            await taskDelay(delayTimeout, ct);

            logger.LogTrace($"Delay completed at {utcNow()}");
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            logger.LogTrace($"Timer elapsed at {utcNow()}");
        }
    }
}

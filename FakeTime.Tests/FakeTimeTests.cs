using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Saab.Time.Fakes.Tests
{
    public class FakeTimeTests
    {
        [Test]
        public async Task FakeDelay_WhenTimeElapses_CompletesAsync()
        {
            var time = new FakeTime();

            var delayTask = time.DelayAsync(TimeSpan.FromSeconds(5));

            delayTask.IsCompleted.ShouldBeFalse();

            // act
            time.AdvanceTime(TimeSpan.FromSeconds(5));

            await delayTask;
            delayTask.IsCompleted.ShouldBeTrue();
        }

        [Test]
        public async Task FakeDelay_WhenCancelled_ThrowsTaskCancelledExceptionAsync()
        {
            var time = new FakeTime();
            using var cts = new CancellationTokenSource();

            var delayTask = time.DelayAsync(TimeSpan.FromSeconds(5), cts.Token);

            delayTask.IsCompleted.ShouldBeFalse();

            // act
            cts.Cancel();

            await Should.ThrowAsync<TaskCanceledException>(() => delayTask);
        }
    }
}
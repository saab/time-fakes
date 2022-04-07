using System;
using NUnit.Framework;
using Shouldly;

namespace Saab.Time.Fakes.Tests
{
    public class FakeCancellationTokenSourceTests
    {
        [Test]
        public void CreateCancellationTokenSource_AdvanceLessThanCancelAfter_CancellationNotRequested()
        {
            var time = new FakeTime();
            using var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));
            cts.IsCancellationRequested.ShouldBeFalse();

            time.AdvanceTime(TimeSpan.FromMinutes(5));

            cts.IsCancellationRequested.ShouldBeFalse();
        }

        [Test]
        public void CreateCancellationTokenSource_AdvanceMoreThanCancelAfter_CancellationRequested()
        {
            var time = new FakeTime();
            using var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));
            cts.IsCancellationRequested.ShouldBeFalse();

            time.AdvanceTime(TimeSpan.FromMinutes(11));

            cts.IsCancellationRequested.ShouldBeTrue();
        }

        [Test]
        public void CreateCancellationTokenSource_ManualCancellationAndAdvanceMoreThanCancelAfter_CancellationRequested()
        {
            var time = new FakeTime();
            using var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));
            cts.IsCancellationRequested.ShouldBeFalse();

            cts.Cancel();
            time.AdvanceTime(TimeSpan.FromMinutes(11));

            cts.IsCancellationRequested.ShouldBeTrue();
        }

        [Test]
        public void CancelAfter_WhenNotCancelled_SchedulesCancel()
        {
            var time = new FakeTime();
            using var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));

            cts.CancelAfter(TimeSpan.FromMinutes(5));
            time.AdvanceTime(TimeSpan.FromMinutes(5));

            cts.IsCancellationRequested.ShouldBeTrue();
        }

        [Test]
        public void CancelAfter_WhenDisposed_ThrowsObjectDisposedException()
        {
            var time = new FakeTime();
            var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));

            cts.Dispose();

            Should.Throw<ObjectDisposedException>(() => cts.CancelAfter(TimeSpan.FromMinutes(5)));
        }

        [Test]
        public void CancelAfter_WhenCancelled_DoesNothing()
        {
            var time = new FakeTime();
            using var cts = time.CreateCancellationTokenSource(TimeSpan.FromMinutes(10));

            cts.Cancel();

            cts.CancelAfter(TimeSpan.FromMinutes(5));
            time.AdvanceTime(TimeSpan.FromMinutes(11));
        }
    }
}

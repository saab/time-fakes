using System;
using NUnit.Framework;
using Shouldly;

namespace Saab.Time.Fakes.Tests
{
    [TestFixture]
    public class FakeTimerTests
    {
        private int tickCount;

        [SetUp]
        public void Setup()
        {
            tickCount = 0;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Advance_WhenNotStartedAndAdvancedPastInterval_ShouldNotTick(bool autoReset)
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset, millisecondInterval: 1000);

            // Act.
            testSubject.Advance(TimeSpan.FromMilliseconds(2050));

            // Assert.
            tickCount.ShouldBe(0);
        }

        [Test]
        public void Advance_WhenAdvancedMultipleIntervals_WithAutoReset_ShouldTickMultipleTimes()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset: true, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(5000));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(5);
        }

        [Test]
        public void Advance_WhenAdvancedPastTwiceTheInterval_WithNoAutoReset_ShouldTickOnce()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset: false, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(5500));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(1);
        }

        [Test]
        public void Advance_WhenCalledMultipleTimes_WithAutoReset_ShouldAllowForIntervalRemainders()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset: true, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(1500));
            testSubject.Advance(TimeSpan.FromMilliseconds(2500));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(4);
        }

        [Test]
        public void Advance_WhenNotAdvancedPastInterval_WithNoAutoReset_ShouldNotTick()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset: false, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(999));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Advance_WhenAdvancedToInterval_ShouldTickOnce(bool autoReset)
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(autoReset, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(1000));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(1);
        }

        [Test]
        public void AdvanceNoneAutoResetEvent_WhenTimeSpanCoversMultipleTicks_TicksOnce()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(false, millisecondInterval: 1000);

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(2000));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(1);
        }

        [Test]
        public void AdvanceNoneAutoResetEvent_WhenTimerStartsItselfDuringTickAndTimeSpanCovers2Ticks_TicksTwice()
        {
            // Arrange.
            using FakeTimer testSubject = CreateTestSubject(false, millisecondInterval: 1000);
            testSubject.Elapsed += (s, e) => testSubject.Start();

            // Act.
            testSubject.Start();
            testSubject.Advance(TimeSpan.FromMilliseconds(2000));
            testSubject.Stop();

            // Assert.
            tickCount.ShouldBe(2);
        }

        public FakeTimer CreateTestSubject(bool autoReset, int millisecondInterval)
        {
            var fakeTimer = new FakeTimer { AutoReset = autoReset, Interval = millisecondInterval };
            fakeTimer.Elapsed += (_, _) => tickCount++;
            return fakeTimer;
        }
    }
}
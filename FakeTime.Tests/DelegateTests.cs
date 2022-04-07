using NUnit.Framework;

namespace Saab.Time.Fakes.Tests
{
    public class DelegateTests
    {
        [Test]
        public void Delegates_Always_MatchFakeMethods()
        {
            using var fakeTime = new FakeTime();

            UtcNow utcNow = () => fakeTime.CurrentTime;
            CreateCancellationTokenSource createCancellationTokenSource = fakeTime.CreateCancellationTokenSource;
            TaskDelay taskDelay = fakeTime.DelayAsync;
            CreateTimer createTimer = fakeTime.CreateTimer;

            Assert.IsNotNull(utcNow);
            Assert.IsNotNull(createCancellationTokenSource);
            Assert.IsNotNull(taskDelay);
            Assert.IsNotNull(createTimer);
        }
    }
}

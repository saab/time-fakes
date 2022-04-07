using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Saab.Time.Fakes.Tests
{
    public sealed class ExampleTests : IDisposable
    {
        FakeTime fakeTime = new ();

        [SetUp]
        public void SetUp()
        {
            fakeTime = new FakeTime
            {
                CurrentTime = new DateTime(2022, 1, 1, 0, 0, 0)
            };
        }

        [Test]
        public async Task Execute_Always_RunsFastEvenAsync()
        {
            // arrange
            var example =
                new Example(
                    NullLogger<Example>.Instance,
                    () => fakeTime.CurrentTime,
                    fakeTime.CreateTimer,
                    fakeTime.DelayAsync,
                    fakeTime.CreateCancellationTokenSource);

            var executeTask = example.ExecuteAsync(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));

            // act
            fakeTime.AdvanceTime(TimeSpan.FromSeconds(15));

            // assert
            await executeTask;
        }

        [TearDown]
        public void TearDown()
        {
            fakeTime?.Dispose();
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}

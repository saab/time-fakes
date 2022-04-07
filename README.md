# fake-time
Fake Time is a set of utilities for controlling the passage of time during a test run.

Useful scenarios for this library are:
* I am using a CancellationToken which should be automatically cancelled in X seconds
* I am using a timer, and I want to see how my class behaves as time progresses
* I need to call Task.Delay as part of my workflow

In all of the scenarios above, we would like our test run to be fast and predictable.

## Using the library
We use dependancy injection to allow us control the passage of time during a test run. Delegates are provided to allow the fake to be injected during a test run, and the real implementation to be injected for production used.

### Using FakeTime in your tests
Declare an instance of FakeTime for your test run:
```csharp
    FakeTime fakeTime = new FakeTime();
```

Set the current time (optional)
```csharp
    fakeTime.CurrentTime = new DateTime(2022, 1, 1, 0, 0, 0);
```

Move the time on in your test when you wish
```csharp
    // arrange
    var delayTask = faketime.DelayAsync(TimeSpan.FromSeconds(5));

    // act
    fakeTime.AdvanceTime(TimeSpan.FromSeconds(5));

    // assert
    await delayTask;
```

A more complex example using a real class
```csharp
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
```

### Using FakeTime in your classes
Inject the required functions into your class, if you wanted to inject everything it would look something like this.
```csharp
    public Example(ILogger<Example> logger, UtcNow utcNow, CreateTimer createTimer, TaskDelay taskDelay, CreateCancellationTokenSource createCancellationTokenSource)
    {
        this.logger = logger;
        this.utcNow = utcNow;
        this.createTimer = createTimer;
        this.taskDelay = taskDelay;
        this.createCancellationTokenSource = createCancellationTokenSource;
    }
```

#### Using timer
```csharp
    public void StartTimer(TimeSpan timerTickPeriod)
    {
        this.timer = createTimer(); // remember to dispose this as appropriate
        timer.Interval = timerTickPeriod.TotalMilliseconds;
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        logger.LogTrace($"Timer elapsed at {utcNow()}");
    }
```

#### Using CancellationTokenSource
```csharp
    public async Task UseCancellationToken(TimeSpan cancellationTimeout)
    {
        using var cts = createCancellationTokenSource(cancellationTimeout);
        var tcs = new TaskCompletionSource<object>();
        using (cts.Token.Register(() => tcs.TrySetResult(new object())))
        {
            await tcs.Task;

            logger.LogTrace($"Task was cancelled at {utcNow()}");
        }
    }
```

#### Using TaskDelay
```csharp
    public async Task UseTaskDelay(TimeSpan delayTimeout, CancellationToken ct = default)
    {
        // Using TaskDelay
        await taskDelay(delayTimeout, ct);

        logger.LogTrace($"Delay completed at {utcNow()}");
    }
```

### Registering for a test run
```csharp
    private FakeTime FakeTime { get; } = new FakeTime();

    private static void AddTime(this IServiceCollection services)
    {
        services.AddSingleton<UtcNow>(() => FakeTime.CurrentTime);
        services.AddSingleton<CreateTimer>(FakeTime.CreateTimer);
        services.AddSingleton<TaskDelay>(FakeTime.DelayAsync);
        services.AddSingleton<CreateCancellationTokenSource>(FakeTime.CreateCancellationTokenSource);
    }
```

### Registering for production
```csharp
    private static void AddTime(this IServiceCollection services)
    {
        services.AddSingleton<UtcNow>(() => DateTime.UtcNow);
        services.AddSingleton<CreateTimer>(() => new SystemTimer());
        services.AddSingleton<TaskDelay>((t, ct) => Task.Delay(t, ct));
        services.AddSingleton<CreateCancellationTokenSource>(t => new CancellationTokenSourceWrapper(t));
    }
```

## Timers
Due to the large number of implementations of Timer in .NET, we have chosen to use an abstraction and allow the user to implement the real ITimer. The ITimer interface maps exactly to the System.Timers.Timer class, and so if you're only using that implementation of Timer it can be implemented in 1 line of code:
```csharp
public class SystemTimer : System.Timers.Timer, ITimer { }
```

## CancellationTokenSource
At this time a wrapper for CancellationTokenSource is not provided. This can be implemented in 1 line of code.
```csharp
public class CancellationTokenSourceWrapper : CancellationTokenSource, ICancellationTokenSource { }
```

## Limitations
* The Fake CancellationTokenSource ```CancelAfter(Int32)``` method does not support 0 or -1 values like the real CancellationTokenSource

## Gotchas
* Setting FakeTime.CurrentTime does not have any effect on the other Fakes being used, this is by design. Use the ```FakeTime.AdvanceTime(TimeSpan)``` method to advance time and see the effect on the fakes.
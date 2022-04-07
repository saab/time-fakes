using System;
using System.Threading;

namespace Saab.Time.Fakes
{
    public class FakeCancellationTokenSource : CancellationTokenSource, ICancellationTokenSource
    {
        private readonly CreateTimer createTimer;
        private bool disposed;
        private volatile ITimer timer;

        public FakeCancellationTokenSource(CreateTimer createTimer)
            : base()
        {
            this.createTimer = createTimer;
        }

        public FakeCancellationTokenSource(CreateTimer createTimer, TimeSpan delay)
            : this(createTimer)
        {
            CancelAfter(delay);
        }

        public new void CancelAfter(TimeSpan delay)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;

            // Deviation from logic in real CancellationToken
            if (totalMilliseconds <= 0 || totalMilliseconds > Int32.MaxValue)
            ////if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            CancelAfter((int)delay.TotalMilliseconds);
        }

        public new void CancelAfter(int millisecondsDelay)
        {
            ThrowIfDisposed();

            // Deviation from logic in real CancellationToken
            if (millisecondsDelay <= 0)
            ////if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            }

            if (IsCancellationRequested)
            {
                return;
            }

            if (timer == null)
            {
                var newTimer = createTimer();
                newTimer.Elapsed += (s, e) => Cancel();

                if (Interlocked.CompareExchange(ref timer, newTimer, null) != null)
                {
                    // Another thread created the timer at the same time. Dispose the one we created.
                    newTimer.Dispose();
                }
            }

            try
            {
                timer.Stop();
                timer.Interval = millisecondsDelay;
                timer.Start();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public new void Cancel()
        {
            Cancel(false);
        }

        public new void Cancel(bool throwOnFirstException)
        {
            ThrowIfDisposed();

            timer?.Dispose();
            timer = null;

            base.Cancel(throwOnFirstException);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
                timer = null;

                disposed = true;
            }

            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}

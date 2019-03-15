using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Put this directly into the the https://github.com/dougdellolio/coinbasepro-csharp/tree/master/CoinbasePro/Network namespace
// ReSharper disable once CheckNamespace
namespace CoinbasePro.Network.HttpClient
{
    /// <inheritdoc />
    ///  Allows a limited number of acquisitions during a timespan
    public class TimeSpanSemaphore : IDisposable
    {
        private readonly SemaphoreSlim _pool;

        // the time span for the max number of callers
        private readonly TimeSpan _resetSpan;

        // keep track of the release times
        private readonly Queue<DateTime> _releaseTimes;

        // protect release time queue
        private readonly object _queueLock = new object();

        public TimeSpanSemaphore(int maxCount, TimeSpan resetSpan)
        {
            _pool = new SemaphoreSlim(maxCount, maxCount);
            _resetSpan = resetSpan;

            // initialize queue with old timestamps
            _releaseTimes = new Queue<DateTime>(maxCount);
            for (var i = 0; i<maxCount; i++)
            {
                _releaseTimes.Enqueue(DateTime.MinValue);
            }
        }

        /// 
        /// Blocks the current thread until it can enter the semaphore, while observing a CancellationToken
        private void Wait(CancellationToken cancelToken)
        {
            // will throw if token is cancelled
            _pool.Wait(cancelToken);

            // get the oldest release from the queue
            DateTime oldestRelease;
            lock (_queueLock)
            {
                oldestRelease = _releaseTimes.Dequeue();
            }

            // sleep until the time since the previous release equals the reset period
            var now = DateTime.UtcNow;
            var windowReset = oldestRelease.Add(_resetSpan);
            if (windowReset < now) return;

            var sleepMilliseconds = Math.Max(
                (int) (windowReset.Subtract(now).Ticks / TimeSpan.TicksPerMillisecond),
                1); // sleep at least 1ms to be sure next window has started

            Debug.WriteLine($"Waiting {sleepMilliseconds} ms for TimeSpanSemaphore limit to reset.");

            var cancelled = cancelToken.WaitHandle.WaitOne(sleepMilliseconds);
            if (!cancelled) return;

            Release();
            cancelToken.ThrowIfCancellationRequested();
        }

        /// 
        /// Exits the semaphore
        /// 
        private void Release()
        {
            lock (_queueLock)
            {
                _releaseTimes.Enqueue(DateTime.UtcNow);
            }

            _pool.Release();
        }

        ///// <summary>
        ///// Runs an action after entering the semaphore (if the CancellationToken is not canceled)
        ///// </summary>
        //public void Run(Action action, CancellationToken cancelToken)
        //{
        //    // will throw if token is cancelled, but will auto-release lock
        //    Wait(cancelToken);

        //    try
        //    {
        //        action();
        //    }
        //    finally
        //    {
        //        Release();
        //    }
        //}

        public Task<T> RunAsync<T, T1>(Func<T1, CancellationToken, Task<T>> sendAsync, T1 request, CancellationToken cancellationToken)
        {
            // will throw if token is cancelled, but will auto-release lock
            Wait(cancellationToken);

            try
            {
                return sendAsync(request, cancellationToken);
            }
            finally
            {
                Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _pool.Dispose();
        }

    }
}

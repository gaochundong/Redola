using System;
using System.Threading;
using System.Threading.Tasks;

namespace Redola.Rpc
{
    public class CountableRateLimiter : IRateLimiter, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public CountableRateLimiter()
            : this(Environment.ProcessorCount)
        {
        }

        public CountableRateLimiter(int limitCount)
        {
            if (limitCount <= 0)
                throw new ArgumentOutOfRangeException("limitCount");

            _semaphore = new SemaphoreSlim(limitCount, limitCount);
        }

        public int CurrentCount { get { return _semaphore.CurrentCount; } }

        #region Wait

        public void Wait()
        {
            _semaphore.Wait();
        }

        public void Wait(CancellationToken cancellationToken)
        {
            _semaphore.Wait(cancellationToken);
        }

        public bool Wait(TimeSpan timeout)
        {
            return _semaphore.Wait(timeout);
        }

        public bool Wait(int millisecondsTimeout)
        {
            return _semaphore.Wait(millisecondsTimeout);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _semaphore.Wait(timeout, cancellationToken);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return _semaphore.Wait(millisecondsTimeout, cancellationToken);
        }

        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();
        }

        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            return await _semaphore.WaitAsync(timeout);
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout)
        {
            return await _semaphore.WaitAsync(millisecondsTimeout);
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
        }

        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await _semaphore.WaitAsync(timeout, cancellationToken);
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken);
        }

        #endregion

        #region Release

        public int Release()
        {
            return _semaphore.Release();
        }

        public int Release(int releaseCount)
        {
            return _semaphore.Release(releaseCount);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _semaphore.Dispose();
                    }
                    catch { }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}

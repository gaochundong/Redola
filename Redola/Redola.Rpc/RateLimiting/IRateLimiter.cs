using System;
using System.Threading.Tasks;

namespace Redola.Rpc
{
    public interface IRateLimiter
    {
        int CurrentCount { get; }

        void Wait();
        bool Wait(TimeSpan timeout);
        bool Wait(int millisecondsTimeout);

        Task WaitAsync();
        Task<bool> WaitAsync(TimeSpan timeout);
        Task<bool> WaitAsync(int millisecondsTimeout);

        int Release();
    }
}

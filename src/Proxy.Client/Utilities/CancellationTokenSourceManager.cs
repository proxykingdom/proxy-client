using System;
using System.Threading;

namespace Proxy.Client.Utilities
{
    /// <summary>
    /// Manager class for the Cancellation Token Source. 
    /// This was can be used to re-use the same CancellationTokenSource struct to save memory.
    /// </summary>
    public sealed class CancellationTokenSourceManager : IDisposable
    {
        /// <summary>
        /// Cancellation Token.
        /// </summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Creates an instance of the Cancellation Token Source Manager.
        /// </summary>
        public CancellationTokenSourceManager()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts the Cancellation Token Source timeout.
        /// </summary>
        /// <param name="timeout">Timeout to cancel.</param>
        public void Start(int timeout) => _cancellationTokenSource.CancelAfter(timeout);

        /// <summary>
        /// Stops the Cancellation Token Source timeout.
        /// </summary>
        public void Stop() => _cancellationTokenSource.CancelAfter(System.Threading.Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Cancels the Cancellation Token Source..
        /// </summary>
        public void Cancel() => _cancellationTokenSource.Cancel();

        /// <summary>
        /// Disposes the Cancellation Token Source.
        /// </summary>
        public void Dispose() => _cancellationTokenSource.Dispose();
    }
}

using System;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    /// <summary>
    /// Task Extension Class.
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Executes the task provided with a specific timeout value.
        /// </summary>
        /// <typeparam name="T">Task Type.</typeparam>
        /// <param name="task">The task to apply timeout on.</param>
        /// <param name="client">The Proxy Client firing the task.</param>
        /// <param name="totalTimeout">Total Request Timeout in ms.</param>
        /// <param name="cancellationTokenSourceManager">Cancellation Token Source manager.</param>
        /// <returns>Current task result if it completes within the specified timeout, or TimeoutException if it times out.</returns>
        public static async Task<T> ExecuteTaskWithTimeout<T>(this Task<T> task, BaseProxyClient client, int totalTimeout, CancellationTokenSourceManager cancellationTokenSourceManager)
        {
            if (await Task.WhenAny(task, Task.Delay(totalTimeout)) == task)
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }

                return task.Result;
            }
            else
            {
                cancellationTokenSourceManager?.Cancel();
                client.IsFaulted = true;

                throw new TimeoutException();
            }
        }
    }
}

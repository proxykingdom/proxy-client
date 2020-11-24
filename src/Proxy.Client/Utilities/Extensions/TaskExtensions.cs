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
        /// <param name="client">Current Proxy Client.</param>
        /// <param name="timeout">Timeout in ms.</param>
        /// <returns>Current task result if it completes within the specified timeout, or TimeoutException if it times out.</returns>
        public static async Task<T> ExecuteTaskWithTimeout<T>(this Task<T> task, IProxyClient client, int timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return task.Result;
            }
            else
            {
                client.Dispose();
                throw new TimeoutException();
            }
        }
    }
}

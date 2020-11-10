using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    /// <summary>
    /// Helper class to measure the execution time of the functions passed.
    /// </summary>
    internal static class TimingHelper
    {
        /// <summary>
        /// Measures the time a given action takes.
        /// </summary>
        /// <param name="atn">Action to be measured.</param>
        /// <returns>Time in Milliseconds</returns>
        public static float Measure(Action atn)
        {
            var stopwatch = Stopwatch.StartNew();

            atn();

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measures the time a given function takes.
        /// </summary>
        /// <typeparam name="T">Result Type returned by the function.</typeparam>
        /// <param name="fn">Function to be measured.</param>
        /// <returns>Time in Milliseconds and a response</returns>
        public static (float time, T response) Measure<T>(Func<T> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = fn();

            stopwatch.Stop();

            return (stopwatch.ElapsedMilliseconds, response);
        }

        /// <summary>
        /// Asynchronously measures the time a function takes.
        /// </summary>
        /// <param name="fn">Function to be measured.</param>
        /// <returns>Time in Milliseconds</returns>
        public static async Task<float> MeasureAsync(Func<Task> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            await fn();

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Asynchronously measures the time a function takes.
        /// </summary>
        /// <typeparam name="T">Result Type returned by the function.</typeparam>
        /// <param name="fn">Function to be measured.</param>
        /// <returns>Time in Milliseconds and a response</returns>
        public static async Task<(float time, T response)> MeasureAsync<T>(Func<Task<T>> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await fn();

            stopwatch.Stop();

            return (stopwatch.ElapsedMilliseconds, response);
        }
    }
}

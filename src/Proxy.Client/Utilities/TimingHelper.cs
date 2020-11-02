using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class TimingHelper
    {
        public static float Measure(Action atn)
        {
            var stopwatch = Stopwatch.StartNew();

            atn();

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        public static (float time, T response) Measure<T>(Func<T> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = fn();

            stopwatch.Stop();

            return (stopwatch.ElapsedMilliseconds, response);
        }

        public static async Task<float> MeasureAsync(Func<Task> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            await fn();

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        public static async Task<(float time, T response)> MeasureAsync<T>(Func<Task<T>> fn)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await fn();

            stopwatch.Stop();

            return (stopwatch.ElapsedMilliseconds, response);
        }
    }
}

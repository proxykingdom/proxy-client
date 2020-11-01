using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class TimingExtensions
    {
        private static readonly Stopwatch _stopwatch;

        static TimingExtensions()
        {
            _stopwatch = new Stopwatch();
        }

        public static float Measure(this Action atn)
        {
            _stopwatch.Start();

            atn();

            _stopwatch.Stop();

            return _stopwatch.ElapsedMilliseconds;
        }

        public static (float time, T response) Measure<T>(this Func<T> fn)
        {
            _stopwatch.Start();
            
            var response = fn();

            _stopwatch.Stop();

            return (_stopwatch.ElapsedMilliseconds, response);
        }

        public static async Task<float> MeasureAsync(this Func<Task> fn)
        {
            _stopwatch.Start();

            await fn();

            _stopwatch.Stop();

            return _stopwatch.ElapsedMilliseconds;
        }

        public static async Task<(float time, T response)> MeasureAsync<T>(this Func<Task<T>> fn)
        {
            _stopwatch.Start();

            var response = await fn();

            _stopwatch.Stop();

            return (_stopwatch.ElapsedMilliseconds, response);
        }
    }
}

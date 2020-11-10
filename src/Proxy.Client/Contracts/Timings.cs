namespace Proxy.Client.Contracts
{
    /// <summary>
    /// Time Measurements class.
    /// </summary>
    public struct Timings
    {
        /// <summary>
        /// Connet time in ms.
        /// </summary>
        public float ConnectTime { get; }

        /// <summary>
        /// Response time in ms.
        /// </summary>
        public float ResponseTime { get; }

        /// <summary>
        /// Time to first byte in ms.
        /// </summary>
        public float FirstByteTime { get; }

        private Timings(float connectTime, float responseTime, float firstByteTime)
        {
            ConnectTime = connectTime;
            ResponseTime = responseTime;
            FirstByteTime = firstByteTime;
        }

        /// <summary>
        /// Creates a Timings object instance.
        /// </summary>
        /// <param name="connectTime">Connect time in ms.</param>
        /// <param name="responseTime">Response time in ms.</param>
        /// <param name="firstByteTime">Time to first byte in ms.</param>
        /// <returns>Timings</returns>
        public static Timings Create(float connectTime, float responseTime, float firstByteTime)
        {
            return new Timings(connectTime, responseTime, firstByteTime);
        }
    }
}

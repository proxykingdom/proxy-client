namespace Proxy.Client.Contracts
{
    public struct Timings
    {
        public float ConnectTime { get; internal set; }
        public float ResponseTime { get; internal set; }
        public float FirstByteTime { get; internal set; }

        private Timings(float connectTime, float responseTime, float firstByteTime)
        {
            ConnectTime = connectTime;
            ResponseTime = responseTime;
            FirstByteTime = firstByteTime;
        }

        public static Timings Create(float connectTime, float responseTime, float firstByteTime)
        {
            return new Timings(connectTime, responseTime, firstByteTime);
        }
    }
}

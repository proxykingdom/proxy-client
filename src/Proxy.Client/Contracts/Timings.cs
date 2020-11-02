namespace Proxy.Client.Contracts
{
    public class Timings
    {
        public float ConnectTime { get; internal set; }
        public float ResponseTime { get; internal set; }
        public float FirstByteTime { get; internal set; }
    }
}

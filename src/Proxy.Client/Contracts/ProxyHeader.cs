namespace Proxy.Client.Contracts
{
    public class ProxyHeader
    {
        public string Name { get; }
        public string Value { get; }

        private ProxyHeader (string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static ProxyHeader Create(string name, string value)
        {
            return new ProxyHeader(name, value);
        }
    }
}

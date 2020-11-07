namespace Proxy.Client.Contracts
{
    public class ResponseHeader
    {
        public string Name { get; }
        public string Value { get; }

        private ResponseHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static ResponseHeader Create(string name, string value)
        {
            return new ResponseHeader(name, value);
        }
    }
}

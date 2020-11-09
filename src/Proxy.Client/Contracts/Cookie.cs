namespace Proxy.Client.Contracts
{
    public class Cookie
    {
        public string Name { get; }
        public string Value { get; }

        private Cookie(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static Cookie Create(string name, string value)
        {
            return new Cookie(name, value);
        }
    }
}

namespace Proxy.Client.Contracts
{
    /// <summary>
    /// Proxy Header Class.
    /// </summary>
    public class ProxyHeader
    {
        /// <summary>
        /// Proxy Header Key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Proxy Header Keu Value.
        /// </summary>
        public string Value { get; }

        private ProxyHeader (string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Create a proxy header object instance.
        /// </summary>
        /// <param name="name">Proxy Header Key.</param>
        /// <param name="value">Proxy Header Keu Value.</param>
        /// <returns></returns>
        public static ProxyHeader Create(string name, string value)
        {
            return new ProxyHeader(name, value);
        }
    }
}

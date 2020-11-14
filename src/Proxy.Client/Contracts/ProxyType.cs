namespace Proxy.Client.Contracts
{
    /// <summary>
    /// Enum that defines the type of proxy.
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// HTTP Proxy Type
        /// </summary>
        HTTP = 0,

        /// <summary>
        /// SOCKS4 Proxy Type.
        /// </summary>
        SOCKS4 = 1,

        /// <summary>
        /// SOCKS5 Proxy Type.
        /// </summary>
        SOCKS5 = 2
    }
}

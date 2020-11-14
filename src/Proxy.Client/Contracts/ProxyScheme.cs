namespace Proxy.Client.Contracts
{
    /// <summary>
    /// Proxy Protocol Enum.
    /// </summary>
    public enum ProxyScheme
    {
        /// <summary>
        /// No SSL.
        /// </summary>
        HTTP = 0,

        /// <summary>
        /// With SSL.
        /// </summary>
        HTTPS = 1
    }
}

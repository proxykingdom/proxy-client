using System;

namespace Proxy.Client.Exceptions
{
    /// <summary>
    /// Proxy Exception class.
    /// </summary>
    internal class ProxyException : Exception
    {
        /// <summary>
        /// Creates a proxy exception given an error message.
        /// </summary>
        /// <param name="message">Proxy error message.</param>
        public ProxyException(string message) : base(message) {}

        /// <summary>
        /// Creates a proxy exception given an error message and exception.
        /// </summary>
        /// <param name="message">Proxy error message.</param>
        /// <param name="innerException">Inner Exception.</param>
        public ProxyException(string message, Exception innerException) : base(message, innerException) {}
    }
}

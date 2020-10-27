using System;

namespace Proxy.Client.Exceptions
{
    internal class ProxyException : Exception
    {
        public ProxyException(string message) : base(message) {}
    }
}

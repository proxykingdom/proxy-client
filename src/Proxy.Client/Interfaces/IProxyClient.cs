using Proxy.Client.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proxy.Client
{
    internal interface IProxyClient
    {
        string ProxyHost { get; }
        int ProxyPort { get; }

        ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);
        Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);

    }
}

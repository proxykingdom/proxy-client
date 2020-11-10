using Proxy.Client.Contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Proxy Client Interface
    /// </summary>
    internal interface IProxyClient : IDisposable
    {
        /// <summary>
        /// Host name or IP address of the proxy server.
        /// </summary>
        string ProxyHost { get; }

        /// <summary>
        /// Port used to connect to the proxy server.
        /// </summary>
        int ProxyPort { get; }

        /// <summary>
        /// Host name or IP address of the destination server.
        /// </summary>
        string DestinationHost { get; }

        /// <summary>
        /// Port used to connect to the destination server.
        /// </summary>
        int DestinationPort { get; }

        /// <summary>
        /// Indicates if the underlying socket is already connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost"></param>
        /// <param name="destinationPort"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <param name="isSsl"></param>
        /// <returns>Proxy Response</returns>
        ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost"></param>
        /// <param name="destinationPort"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <param name="isSsl"></param>
        /// <returns>Proxy Response</returns>
        Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);
    }
}

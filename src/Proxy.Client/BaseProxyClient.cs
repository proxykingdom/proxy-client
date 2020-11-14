using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Base proxy client class containing the shared logic to be implemented in all derived proxy clients.
    /// </summary>
    public abstract class BaseProxyClient : IProxyClient
    {
        /// <summary>
        /// Host name or IP address of the proxy server.
        /// </summary>
        public string ProxyHost { get; protected set; }

        /// <summary>
        /// Port used to connect to the proxy server.
        /// </summary>
        public int ProxyPort { get; protected set; }

        /// <summary>
        /// The type of proxy.
        /// </summary>
        public ProxyType ProxyType { get; protected set; }

        /// <summary>
        /// Host name or IP address of the destination server.
        /// </summary>
        public string DestinationHost { get; private set; }

        /// <summary>
        /// Port used to connect to the destination server.
        /// </summary>
        public int DestinationPort { get; private set; }

        /// <summary>
        /// Socket used to send and receive requests.
        /// </summary>
        protected internal Socket Socket { get; private set; }

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Get(string destinationHost, int destinationPort, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Post(string destinationHost, int destinationPort, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the POST request.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Put(string destinationHost, int destinationPort, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the PUT request.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> PutAsync(string destinationHost, int destinationPort, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false);

        /// <summary>
        /// Disposes the socket dependencies.
        /// </summary>
        public virtual void Dispose()
        {
            Socket?.Close();
        }

        /// <summary>
        /// Sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract (ProxyResponse response, float firstByteTime) SendGetCommand(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Asynchronously sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Asynchronously sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract (ProxyResponse response, float firstByteTime) SendPutCommand(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Asynchronously sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal abstract Task<(ProxyResponse response, float firstByteTime)> SendPutCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies, bool isSsl);

        /// <summary>
        /// Handles the given request based on if the proxy client is connected or not.
        /// </summary>
        /// <param name="connectNegotiationFn">Performs connection negotations with the destination server.</param>
        /// <param name="requestFn">Sends the request on the underlying socket.</param>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port to be used to connect to the destination server.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        protected internal ProxyResponse HandleRequest(Action connectNegotiationFn,
            Func<(ProxyResponse response, float firstByteTime)> requestFn, string destinationHost, int destinationPort, bool isSsl)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                float connectTime = 0;

                var previousDestinationHost = DestinationHost;

                DestinationHost = destinationHost;
                DestinationPort = destinationPort;

                if (Socket == null)
                {
                    connectTime = CreateSocket();
                }
                else if (IsDispose(previousDestinationHost, isSsl))
                {
                    Dispose();
                    connectTime = CreateSocket();
                }

                var (requestTime, innerResult) = TimingHelper.Measure(() =>
                {
                    return requestFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + requestTime, connectTime + innerResult.firstByteTime);

                return innerResult.response;
            }
            catch (Exception ex)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed with Exception: {ex}"));
            }

            float CreateSocket()
            {
                return TimingHelper.Measure(() =>
                {
                    Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    Socket.Connect(ProxyHost, ProxyPort);
                    connectNegotiationFn();
                });
            }
        }

        /// <summary>
        /// Asynchronously handles the given request based on if the proxy client is connected or not.
        /// </summary>
        /// <param name="connectNegotiationFn">Performs connection negotations with the destination server.</param>
        /// <param name="requestedFn">Sends the request on the underlying socket.</param>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port to be used to connect to the destination server.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
        protected internal async Task<ProxyResponse> HandleRequestAsync(Func<Task> connectNegotiationFn, 
            Func<Task<(ProxyResponse response, float firstByteTime)>> requestedFn, string destinationHost, int destinationPort, bool isSsl)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                float connectTime = 0;

                var previousDestinationHost = DestinationHost;

                DestinationHost = destinationHost;
                DestinationPort = destinationPort;

                if (Socket == null)
                {
                    connectTime = await CreateSocketAsync();
                }
                else if (IsDispose(previousDestinationHost, isSsl))
                {
                    Dispose();
                    connectTime = await CreateSocketAsync();
                }

                var (requestTime, innerResult) = await TimingHelper.MeasureAsync(() =>
                {
                    return requestedFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + requestTime, connectTime + innerResult.firstByteTime);

                return innerResult.response;
            }
            catch (Exception ex)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed with Exception: {ex}"));
            }

            Task<float> CreateSocketAsync()
            {
                return TimingHelper.MeasureAsync(async () =>
                {
                    Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    await Socket.ConnectAsync(ProxyHost, ProxyPort);
                    await connectNegotiationFn();
                });
            }
        }

        private bool IsDispose(string previousDestinationHost, bool isSsl) 
            => !Socket.Connected || ((ProxyType != ProxyType.HTTP || isSsl) && !previousDestinationHost.Equals(DestinationHost));
    }
}

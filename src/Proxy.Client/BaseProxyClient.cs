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
        /// Host name or IP address of the destination server.
        /// </summary>
        public string DestinationHost { get; private set; }

        /// <summary>
        /// Port used to connect to the destination server.
        /// </summary>
        public int DestinationPort { get; private set; }

        /// <summary>
        /// Indicates if the underlying socket is already connected.
        /// </summary>
        public bool IsConnected { get; private set; }

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
        /// <param name="notConnectedAtn">Action to be executed when the socket is not connected.</param>
        /// <param name="connectedFn">Function to be executed when the socket is connected.</param>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port to be used to connect to the destination server.</param>
        /// <returns>Proxy Response</returns>
        protected internal ProxyResponse HandleRequest(Action notConnectedAtn, Func<(ProxyResponse response, float firstByteTime)> connectedFn,
            string destinationHost, int destinationPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                float connectTime = 0;

                if (!IsConnected)
                {
                    DestinationHost = destinationHost;
                    DestinationPort = destinationPort;

                    connectTime = TimingHelper.Measure(() =>
                    {
                        Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        Socket.Connect(ProxyHost, ProxyPort);
                        notConnectedAtn();
                    });

                    IsConnected = true;
                }

                var (time, innerResult) = TimingHelper.Measure(() =>
                {
                    return connectedFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + time, connectTime + innerResult.firstByteTime);

                return innerResult.response;
            }
            catch (Exception)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed."));
            }
        }

        /// <summary>
        /// Asynchronously handles the given request based on if the proxy client is connected or not.
        /// </summary>
        /// <param name="notConnectedFn">Function to be executed when the socket is not connected.</param>
        /// <param name="connectedFn">Function to be executed when the socket is connected.</param>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port to be used to connect to the destination server.</param>
        /// <returns>Proxy Response</returns>
        protected internal async Task<ProxyResponse> HandleRequestAsync(Func<Task> notConnectedFn, Func<Task<(ProxyResponse response, float firstByteTime)>> connectedFn,
            string destinationHost, int destinationPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                float connectTime = 0;

                if (!IsConnected)
                {
                    DestinationHost = destinationHost;
                    DestinationPort = destinationPort;

                    connectTime = await TimingHelper.MeasureAsync(async () =>
                    {
                        Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        await Socket.ConnectAsync(ProxyHost, ProxyPort);
                        await notConnectedFn();
                    });

                    IsConnected = true;
                }

                var (time, innerResult) = await TimingHelper.MeasureAsync(async () =>
                {
                    return await connectedFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + time, connectTime + innerResult.firstByteTime);

                return innerResult.response;
            }
            catch (Exception ex)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed with Exception: {ex}"));
            }
        }

        public virtual void Dispose()
        {
            Socket?.Close();
        }
    }
}

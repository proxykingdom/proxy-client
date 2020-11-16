using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
        /// The type of proxy.
        /// </summary>
        public ProxyScheme Scheme { get; private set; }

        /// <summary>
        /// Host name or IP address of the destination server.
        /// </summary>
        public string DestinationHost { get; private set; }

        /// <summary>
        /// Port used to connect to the destination server.
        /// </summary>
        public int DestinationPort { get; private set; }

        /// <summary>
        /// URL Query.
        /// </summary>
        public string UrlQuery { get; private set; }

        /// <summary>
        /// Underlying socket used to send and receive requests.
        /// </summary>
        protected internal Socket Socket { get; private set; }

        /// <summary>
        /// Stream for SSL reads and writes on the underlying socket.
        /// </summary>
        protected internal SslStream SslStream { get; private set; }

        /// <summary>
        /// Destination URI object.
        /// </summary>
        protected internal Uri DestinationUri { get; private set; }

        /// <summary>
        /// Indicates whether the destination server explicitly closes the underlying connection.
        /// </summary>
        protected internal bool IsConnectionClosed { get; private set; }

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Get(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> GetAsync(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Post(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST request.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> PostAsync(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public abstract ProxyResponse Put(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT request.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public abstract Task<ProxyResponse> PutAsync(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null);

        /// <summary>
        /// Disposes the socket dependencies.
        /// </summary>
        public virtual void Dispose()
        {
            Socket?.Close();
            SslStream?.Close();
        }

        /// <summary>
        /// Connects to the Destination Server.
        /// </summary>
        protected internal abstract void SendConnectCommand();

        /// <summary>
        /// Asynchronously connects to the Destination Server.
        /// </summary>
        protected internal abstract Task SendConnectCommandAsync();

        /// <summary>
        /// Handles the given request based on if the proxy client is connected or not.
        /// </summary>
        /// <param name="connectNegotiationFn">Performs connection negotations with the destination server.</param>
        /// <param name="requestFn">Sends the request on the underlying socket.</param>
        /// <param name="url">Destination Url.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <returns>Proxy Response</returns>
        protected internal ProxyResponse HandleRequest(Action connectNegotiationFn,
            Func<(ProxyResponse response, float firstByteTime)> requestFn, string url, bool isKeepAlive)
        {
            try
            {
                var (cachedDestinationHost, cachedScheme) = ParseAndReturnCachedItems(url);
                float connectTime = 0;

                if (Socket == null)
                {
                    connectTime = CreateSocket();
                }
                else if (IsDispose(cachedDestinationHost, cachedScheme, isKeepAlive))
                {
                    Dispose();
                    connectTime = CreateSocket();
                }

                var (requestTime, innerResult) = TimingHelper.Measure(() =>
                {
                    return requestFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + requestTime, connectTime + innerResult.firstByteTime);

                CheckConnectionHeader(innerResult.response.Headers);

                return innerResult.response;
            }
            catch (Exception ex)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed."), ex);
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
        /// <param name="url">Destination Url.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <returns>Proxy Response</returns>
        protected internal async Task<ProxyResponse> HandleRequestAsync(Func<Task> connectNegotiationFn, 
            Func<Task<(ProxyResponse response, float firstByteTime)>> requestedFn, string url, bool isKeepAlive)
        {
            try
            {
                var (cachedDestinationHost, cachedScheme) = ParseAndReturnCachedItems(url);
                float connectTime = 0;

                if (Socket == null)
                {
                    connectTime = await CreateSocketAsync();
                }
                else if (IsDispose(cachedDestinationHost, cachedScheme, isKeepAlive))
                {
                    Dispose();
                    connectTime = await CreateSocketAsync();
                }

                var (requestTime, innerResult) = await TimingHelper.MeasureAsync(() =>
                {
                    return requestedFn();
                });

                innerResult.response.Timings = Timings.Create(connectTime, connectTime + requestTime, connectTime + innerResult.firstByteTime);

                CheckConnectionHeader(innerResult.response.Headers);

                return innerResult.response;
            }
            catch (Exception ex)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed."), ex);
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

        /// <summary>
        /// Sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal (ProxyResponse response, float firstByteTime) SendGetCommand(bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, isKeepAlive, headers, cookies);
            return HandleRequestCommand(writeBuffer);
        }

        /// <summary>
        /// Asynchronously sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, isKeepAlive, headers, cookies);
            return HandleRequestCommandAsync(writeBuffer);
        }

        /// <summary>
        /// Sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal (ProxyResponse response, float firstByteTime) SendPostCommand(string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, body, isKeepAlive, headers, cookies);
            return HandleRequestCommand(writeBuffer);
        }

        /// <summary>
        /// Asynchronously sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, body, isKeepAlive, headers, cookies);
            return HandleRequestCommandAsync(writeBuffer);
        }

        /// <summary>
        /// Sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal (ProxyResponse response, float firstByteTime) SendPutCommand(string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, body, isKeepAlive, headers, cookies);
            return HandleRequestCommand(writeBuffer);
        }

        /// <summary>
        /// Asynchronously sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal Task<(ProxyResponse response, float firstByteTime)> SendPutCommandAsync(string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, DestinationUri.Authority, body, isKeepAlive, headers, cookies);
            return HandleRequestCommandAsync(writeBuffer);
        }

        /// <summary>
        /// Performs the SSL Handshake with the Destination Server.
        /// </summary>
        protected internal void HandleSslHandshake()
        {
            var networkStream = new NetworkStream(Socket);
            SslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            SslStream.AuthenticateAsClient(DestinationHost);
        }

        /// <summary>
        /// Asynchronously performs the SSL Handshake with the Destination Server.
        /// </summary>
        /// <returns></returns>
        protected internal async Task HandleSslHandshakeAsync()
        {
            var networkStream = new NetworkStream(Socket);
            SslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            await SslStream.AuthenticateAsClientAsync(DestinationHost);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None;

        private (ProxyResponse response, float firstByteTime) HandleRequestCommand(byte[] writeBuffer)
        {
            string response;
            float firstByteTime;

            if (Scheme == ProxyScheme.HTTPS)
            {
                SslStream.Write(writeBuffer);
                (response, firstByteTime) = SslStream.ReceiveAll();
            }
            else
            {
                Socket.Send(writeBuffer);
                (response, firstByteTime) = Socket.ReceiveAll();
            }

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        private async Task<(ProxyResponse response, float firstByteTime)> HandleRequestCommandAsync(byte[] writeBuffer)
        {
            string response;
            float firstByteTime;

            if (Scheme == ProxyScheme.HTTPS)
            {
                await SslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                (response, firstByteTime) = await SslStream.ReceiveAllAsync();
            }
            else
            {
                await Socket.SendAsync(writeBuffer);
                (response, firstByteTime) = await Socket.ReceiveAllAsync();
            }

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        private (string cachedDestHost, string cachedScheme) ParseAndReturnCachedItems(string url)
        {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
                throw new ProxyException($"Invalid URL provided: {url}.");

            var cachedDestinationHost = DestinationHost;
            var cachedScheme = DestinationUri?.Scheme;

            DestinationUri = uriResult;

            Scheme = (ProxyScheme)Enum.Parse(typeof(ProxyScheme), uriResult.Scheme, true);
            UrlQuery = uriResult.PathAndQuery;
            DestinationHost = uriResult.Host;
            DestinationPort = uriResult.Port;

            return (cachedDestinationHost, cachedScheme);
        }

        private bool IsDispose(string cachedDestHost, string cachedScheme, bool isKeepAlive)
        {
            return IsConnectionClosed || !isKeepAlive || (ProxyType == ProxyType.HTTP && Scheme == ProxyScheme.HTTP
                ? !cachedScheme.Equals(DestinationUri.Scheme)
                : !cachedDestHost.Equals(DestinationHost) || !cachedScheme.Equals(DestinationUri.Scheme));
        }

        private void CheckConnectionHeader(IEnumerable<ProxyHeader> headers)
        {
            var connectionHeader = headers.Where(x => x.Name.Equals(RequestConstants.CONNECTION_HEADER) || x.Name.Equals(RequestConstants.PROXY_CONNECTION_HEADER)).SingleOrDefault();

            if (connectionHeader != null)
            {
                IsConnectionClosed = !connectionHeader.Value.ToLower().Equals("keep-alive");
            }
        }

        
        /*
        private bool IsDDispose(string cachedDestHost, string cachedScheme, bool isKeepAlive)
        {
            if (IsConnectionClosed || !isKeepAlive)
            {
                return true;
            }

            if (ProxyType == ProxyType.HTTP)
            {
                if (Scheme == ProxyScheme.HTTP)
                {
                    return !cachedScheme.Equals(DestinationUri.Scheme);
                }
                else
                {
                    return !cachedDestHost.Equals(DestinationHost) || !cachedScheme.Equals(DestinationUri.Scheme);
                }
            }
            else
            {
                if (!cachedDestHost.Equals(DestinationHost) || !cachedScheme.Equals(DestinationUri.Scheme))
                {
                    return true;
                }
                return false;
            }
        }
        */
    }
}

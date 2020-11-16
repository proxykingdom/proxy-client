using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Http connection proxy class. This class implements the Http standard proxy protocol.
    /// </summary>
    public sealed class HttpProxyClient : BaseProxyClient
    {
        /// <summary>
        /// Proxy Username used to connect to the Proxy Server.
        /// </summary>
        public string ProxyUsername { get; }

        /// <summary>
        /// Proxy Password used to connect to the Proxy Server.
        /// </summary>
        public string ProxyPassword { get; }

        /// <summary>
        /// Creates a Http proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
        public HttpProxyClient(string proxyHost, int proxyPort)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyType = ProxyType.HTTP;
        }

        /// <summary>
        /// Creates a Http proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
        /// <param name="proxyUsername">Proxy Username used to connect to the Proxy Server.</param>
        /// <param name="proxyPassword">Proxy Password used to connect to the Proxy Server.</param>
        public HttpProxyClient(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            if (String.IsNullOrEmpty(proxyUsername))
                throw new ArgumentNullException("proxyUsername");

            if (proxyPassword == null)
                throw new ArgumentNullException("proxyPassword");

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUsername = proxyUsername;
            ProxyPassword = proxyPassword;
            ProxyType = ProxyType.HTTP;
        }

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Get(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => 
            {
                SendConnectCommand();
            },
            () =>
            {
                return SendGetCommand(isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> GetAsync(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() => 
            {
                return Task.CompletedTask; 
            }, () =>
            {
                return SendGetCommandAsync(isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Post(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => { },
            () => 
            {
                return SendPostCommand(body, isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PostAsync(string url, string body, bool isKeepAlive =true,IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            }, () =>
            {
                return SendPostCommandAsync(body, isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Put(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => { },
            () =>
            {
                return SendPutCommand(body, isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PutAsync(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            }, () =>
            {
                return SendPutCommandAsync(body, isKeepAlive, headers, cookies);
            }, url, isKeepAlive);
        }

        /// <summary>
        /// Connects to the Destination Host.
        /// </summary>
        protected internal override void SendConnectCommand()
        {
            if (Scheme == ProxyScheme.HTTP)
                return;

            var writeBuffer = String.IsNullOrEmpty(ProxyUsername)
                ? CommandHelper.ConnectCommand(DestinationUri.Host)
                : CommandHelper.ConnectProxyAuthCommand(DestinationUri.Host, ProxyUsername, ProxyPassword);

            Socket.Send(writeBuffer);

            var readBuffer = new byte[50];
            Socket.Receive(readBuffer);

            var readString = Encoding.ASCII.GetString(readBuffer);
            var statusRegexMatch = Regex.Match(readString, RequestConstants.STATUS_CODE_PATTERN).Value;

            if (String.IsNullOrEmpty(statusRegexMatch))
                throw new ProxyException("Connect command failed.");

            var statusNumber = Convert.ToInt32(statusRegexMatch, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            if (status != HttpStatusCode.OK)
                throw new ProxyException($"Connect command to Destination Server returned {status}.");

            HandleSslHandshake();
        }

        /// <summary>
        /// Asynchronously connects to the Destination Host.
        /// </summary>
        protected internal override async Task SendConnectCommandAsync()
        {
            if (Scheme == ProxyScheme.HTTP)
                return;

            var writeBuffer = String.IsNullOrEmpty(ProxyUsername)
                ? CommandHelper.ConnectCommand(DestinationUri.Host)
                : CommandHelper.ConnectProxyAuthCommand(DestinationUri.Host, ProxyUsername, ProxyPassword);

            await Socket.SendAsync(writeBuffer);

            var readBuffer = new byte[10];
            await Socket.ReceiveAsync(readBuffer, readBuffer.Length);

            var readString = Encoding.ASCII.GetString(readBuffer);
            var statusRegexMatch = Regex.Match(readString, RequestConstants.STATUS_CODE_PATTERN).Value;

            if (String.IsNullOrEmpty(statusRegexMatch))
                throw new ProxyException("Connect command failed.");

            var statusNumber = Convert.ToInt32(statusRegexMatch, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            if (status != HttpStatusCode.OK)
                throw new ProxyException($"Connect command to Destination Server returned {status}.");

            await HandleSslHandshakeAsync();
        }
    }
}

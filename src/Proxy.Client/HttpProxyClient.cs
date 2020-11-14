using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Http connection proxy class. This class implements the Http standard proxy protocol.
    /// </summary>
    public sealed class HttpProxyClient : BaseProxyClient
    {
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
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Get(string url, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => { },
            () =>
            {
                return SendGetCommand(headers, cookies);
            }, url);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> GetAsync(string url, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() => 
            {
                return Task.CompletedTask; 
            }, () =>
            {
                return SendGetCommandAsync(headers, cookies);
            }, url);
        }

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Post(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => { },
            () => 
            {
                return SendPostCommand(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PostAsync(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            }, () =>
            {
                return SendPostCommandAsync(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Put(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() => { },
            () =>
            {
                return SendPutCommand(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PutAsync(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            }, () =>
            {
                return SendPutCommandAsync(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);

            Socket.SendAsync(writeBuffer);
            var (response, firstByteTime) = Socket.ReceiveAll();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        /// <summary>
        /// Asynchronously sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);

            await Socket.SendAsync(writeBuffer);
            var (response, firstByteTime) = await Socket.ReceiveAllAsync();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        /// <summary>
        /// Sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);

            Socket.SendAsync(writeBuffer);
            var (response, firstByteTime) = Socket.ReceiveAll();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        /// <summary>
        /// Asynchronously sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);

            await Socket.SendAsync(writeBuffer);
            var (response, firstByteTime) = await Socket.ReceiveAllAsync();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        /// <summary>
        /// Sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendPutCommand(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);

            Socket.SendAsync(writeBuffer);
            var (response, firstByteTime) = Socket.ReceiveAll();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        /// <summary>
        /// Asynchronously sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPutCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);

            await Socket.SendAsync(writeBuffer);
            var(response, firstByteTime) = await Socket.ReceiveAllAsync();

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }
    }
}

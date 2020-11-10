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
        }

        public override ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return HandleRequest(() => { },
            () =>
            {
                return SendGetCommand(headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return await HandleRequestAsync(() => 
            {
                return Task.CompletedTask; 
            },
            async () =>
            {
                return await SendGetCommandAsync(headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return HandleRequest(() => { },
            () => 
            {
                return SendPostCommand(body, headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return await HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            },
            async () =>
            {
                return await SendPostCommandAsync(body, headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            }, isSsl);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return await HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            }, isSsl);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            }, isSsl);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return await HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            }, isSsl);
        }

        #region Private Methods
        private (ProxyResponse response, float firstByteTime) HandleRequestCommand(Func<string, (string response, float firstByteTime)> fn, bool isSsl)
        {
            var ssl = isSsl ? RequestConstants.SSL : RequestConstants.NO_SSL;

            var (response, firstByteTime) = fn(ssl);

            return (ResponseBuilderHelper.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        private async Task<(ProxyResponse response, float firstByteTime)> HandleRequestCommandAsync(Func<string, Task<(string response, float firstByteTime)>> fn, bool isSsl)
        {
            var ssl = isSsl ? RequestConstants.SSL : RequestConstants.NO_SSL;

            var (response, firstByteTime) = await fn(ssl);

            return (ResponseBuilderHelper.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }
        #endregion
    }
}

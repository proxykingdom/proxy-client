using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.Client
{
    public sealed class HttpProxyClient : BaseProxyClient
    {
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

        public override ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return HandleRequest(() => { },
            () =>
            {
                return SendGetCommand(headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return await HandleRequestAsync(() => 
            {
                return Task.CompletedTask; 
            },
            async () =>
            {
                return await SendGetCommandAsync(headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return HandleRequest(() => { },
            () => 
            {
                return SendPostCommand(body, headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return await HandleRequestAsync(() =>
            {
                return Task.CompletedTask;
            },
           async () =>
           {
               return await SendPostCommandAsync(body, headers, isSsl);
           }, destinationHost, destinationPort);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IDictionary<string, string> headers, bool isSsl)
        {
            var writeBuffer = isSsl
                ? RequestHelper.GetCommand(DestinationHost, RequestConstants.SSL, headers)
                : RequestHelper.GetCommand(DestinationHost, RequestConstants.NO_SSL, headers);

            Socket.Send(writeBuffer);

            var (response, firstByteTime) = Socket.ReceiveAll();

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IDictionary<string, string> headers, bool isSsl)
        {
            var writeBuffer = isSsl
               ? RequestHelper.GetCommand(DestinationHost, RequestConstants.SSL, headers)
               : RequestHelper.GetCommand(DestinationHost, RequestConstants.NO_SSL, headers);

            await Socket.SendAsync(writeBuffer);

            var (response, firstByteTime) = await Socket.ReceiveAllAsync();

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IDictionary<string, string> headers, bool isSsl)
        {
            var writeBuffer = isSsl
                 ? RequestHelper.PostCommand(body, DestinationHost, RequestConstants.SSL, headers)
                 : RequestHelper.PostCommand(body, DestinationHost, RequestConstants.NO_SSL, headers);

            Socket.Send(writeBuffer);

            var (response, firstByteTime) = Socket.ReceiveAll();

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IDictionary<string, string> headers, bool isSsl)
        {
            var writeBuffer = isSsl
                ? RequestHelper.PostCommand(body, DestinationHost, RequestConstants.SSL, headers)
                : RequestHelper.PostCommand(body, DestinationHost, RequestConstants.NO_SSL, headers);

            await Socket.SendAsync(writeBuffer);

            var (response, firstByteTime) = await Socket.ReceiveAllAsync();

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }
    }
}

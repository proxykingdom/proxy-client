using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client
{
    public abstract class BaseProxyClient : IDisposable
    {
        protected internal Socket Socket { get; private set; }

        protected internal abstract void SendConnectCommand(string destinationHost, int destinationPort);
        protected internal abstract Task SendConnectCommandAsync(string destinationHost, int destinationPort);
        protected internal abstract void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort);

        protected internal ProxyResponse HandleRequest(Func<ProxyResponse> fn, 
            string destinationHost, int destinationPort,
            string proxyHost, int proxyPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                Socket.Connect(proxyHost, proxyPort);

                return fn();
            }
            catch (Exception)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {proxyHost} on port {proxyPort} failed."));
            }
        }

        protected internal async Task<ProxyResponse> HandleRequestAsync(Func<Task<ProxyResponse>> fn,
            string destinationHost, int destinationPort,
            string proxyHost, int proxyPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                await Socket.ConnectAsync(proxyHost, proxyPort);

                return await fn();
            }
            catch (Exception)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {proxyHost} on port {proxyPort} failed."));
            }
        }

        protected internal ProxyResponse SendGetCommand(string destinationHost, IDictionary<string, string> headers, bool isSsl)
        {
            Socket.Send(CreateGetCommand(destinationHost, headers, isSsl), SocketFlags.None);

            var responseBuffer = new byte[Socket.ReceiveBufferSize];
            var bytesRec = Socket.Receive(responseBuffer, SocketFlags.None);

            var response = Encoding.UTF8.GetString(responseBuffer);

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        protected internal async Task<ProxyResponse> SendGetCommandAsync(string destinationHost, IDictionary<string, string> headers, bool isSsl)
        {
            await Socket.SendAsync(CreateGetCommand(destinationHost, headers, isSsl), SocketFlags.None);

            var responseBuffer = new byte[20000];
            await Socket.ReceiveAsync(responseBuffer, SocketFlags.None);

            var response = Encoding.UTF8.GetString(responseBuffer);

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        private static byte[] CreateGetCommand(string destinationHost, IDictionary<string, string> headers, bool isSsl)
        {
            var ssl = isSsl ? "https" : "http";

            var request = headers != null
                ? $"GET {ssl}://{destinationHost}/ HTTP/1.1\r\n" + $"{headers.Select(x => $"{x.Key}: {x.Value}")}\r\n"
                : $"GET {ssl}://{destinationHost}/ HTTP/1.1\r\n\r\n";

            return Encoding.ASCII.GetBytes(request);
        }

        public void Dispose()
        {
            Socket.Close();
        }
    }
}

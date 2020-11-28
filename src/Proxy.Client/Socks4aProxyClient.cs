using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Utilities.Extensions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Socks4a connection proxy class. This class implements the Socks4a standard proxy protocol.
    /// </summary>
    public class Socks4aProxyClient : Socks4ProxyClient
    {
        /// <summary>
        /// Creates a Socks4a proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to the proxy server.</param>
        public Socks4aProxyClient(string proxyHost, int proxyPort) : base(proxyHost, proxyPort) => ProxyType = ProxyType.SOCKS4A;

        /// <summary>
        /// Creates a Socks4a proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to the proxy server.</param>
        /// <param name="proxyUserId">Proxy user identification information.</param>
        public Socks4aProxyClient(string proxyHost, int proxyPort, string proxyUserId) : base(proxyHost, proxyPort, proxyUserId) => ProxyType = ProxyType.SOCKS4A;

        /// <summary>
        /// Connects to the Destination Server.
        /// </summary>
        protected internal override void SendConnectCommand()
        {
            var destinationAddress = new byte[4] { 0, 0, 0, 1 };
            var destinationPort = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);
            var hostBytes = Encoding.ASCII.GetBytes(DestinationHost);

            var request = GetCommandRequest(destinationAddress, destinationPort, userIdBytes, hostBytes);

            Socket.Send(request, SocketFlags.None);

            var response = new byte[8];
            Socket.Receive(response, SocketFlags.None);

            if (response[1] != Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_GRANTED)
                HandleProxyCommandError(response);

            if (Scheme == ProxyScheme.HTTPS)
                HandleSslHandshake();
        }

        /// <summary>
        /// Asynchronously connects to the Destination Server.
        /// </summary>
        protected internal override async Task SendConnectCommandAsync()
        {
            var destinationAddress = new byte[4] { 0, 0, 0, 1 };
            var destinationPort = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);
            var hostBytes = Encoding.ASCII.GetBytes(DestinationHost);

            var request = GetCommandRequest(destinationAddress, destinationPort, userIdBytes, hostBytes);

            await Socket.SendAsync(request, WriteTimeout, CancellationTokenSourceManager);

            var response = new byte[8];
            await Socket.ReceiveAsync(response, ReadTimeout, CancellationTokenSourceManager);

            if (response[1] != Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_GRANTED)
                HandleProxyCommandError(response);

            if (Scheme == ProxyScheme.HTTPS)
                await HandleSslHandshakeAsync();
        }

        #region Private Methods
        private static byte[] GetCommandRequest(byte[] destinationAddressBytes, byte[] destinationPortBytes, byte[] userIdBytes, byte[] hostBytes)
        {
            var request = new byte[10 + userIdBytes.Length + hostBytes.Length];

            request[0] = Socks4Constants.SOCKS4_VERSION_NUMBER;
            request[1] = Socks4Constants.SOCKS4_CMD_CONNECT;
            destinationPortBytes.CopyTo(request, 2);
            destinationAddressBytes.CopyTo(request, 4);
            userIdBytes.CopyTo(request, 8);
            request[8 + userIdBytes.Length] = 0x00;
            hostBytes.CopyTo(request, 9 + userIdBytes.Length);
            request[9 + userIdBytes.Length + hostBytes.Length] = 0x00;

            return request;
        }
        #endregion
    }
}

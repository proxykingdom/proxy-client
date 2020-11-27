using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Socks4 connection proxy class. This class implements the Socks4 standard proxy protocol.
    /// </summary>
    public class Socks4ProxyClient : BaseProxyClient
    {
        /// <summary>
        /// Proxy user identification information.
        /// </summary>
        public string ProxyUserId { get; }

        /// <summary>
        /// Creates a Socks4 proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
        public Socks4ProxyClient(string proxyHost, int proxyPort)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUserId = string.Empty;
            ProxyType = ProxyType.SOCKS4;
        }

        /// <summary>
        /// Creates a Socks4 proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
        /// <param name="proxyUserId">Proxy user identification information.</param>
        public Socks4ProxyClient(string proxyHost, int proxyPort, string proxyUserId)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            if (proxyUserId == null)
                throw new ArgumentNullException(nameof(proxyUserId));

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUserId = proxyUserId;
            ProxyType = ProxyType.SOCKS4;
        }

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Get(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendGetCommand(isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, readTimeout, writeTimeout);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="totalTimeout">Total Request Timeout in ms.</param>
        /// <param name="connectTimeout">Connect Timeout in ms.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> GetAsync(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int totalTimeout = 60000, int connectTimeout = 45000, int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
            }, () =>
            {
                return SendGetCommandAsync(isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, connectTimeout, readTimeout, writeTimeout).ExecuteTaskWithTimeout(this, totalTimeout, CancellationTokenSourceManager);
        }

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Post(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendPostCommand(body, isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, readTimeout, writeTimeout);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST request.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="totalTimeout">Total Request Timeout in ms.</param>
        /// <param name="connectTimeout">Connect Timeout in ms.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PostAsync(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int totalTimeout = 60000, int connectTimeout = 45000, int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
            }, () =>
            {
                return SendPostCommandAsync(body, isKeepAlive, headers, cookies);
            },
            url, isKeepAlive, connectTimeout, readTimeout, writeTimeout).ExecuteTaskWithTimeout(this, totalTimeout, CancellationTokenSourceManager);
        }

        /// <summary>
        /// Connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Put(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendPutCommand(body, isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, readTimeout, writeTimeout);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT request.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <param name="totalTimeout">Total Request Timeout in ms.</param>
        /// <param name="connectTimeout">Connect Timeout in ms.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PutAsync(string url, string body, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int totalTimeout = 60000, int connectTimeout = 45000, int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
            }, () =>
            {
                return SendPutCommandAsync(body, isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, connectTimeout, readTimeout, writeTimeout).ExecuteTaskWithTimeout(this, totalTimeout, CancellationTokenSourceManager);
        }

        /// <summary>
        /// Connects to the proxy client, sends the DELETE command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the DELETE command.</param>
        /// <param name="cookies">Cookies to be sent with the DELETE command.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Delete(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendDeleteCommand(isKeepAlive, headers, cookies);
            }, 
            url, isKeepAlive, readTimeout, writeTimeout);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the DELETE command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the DELETE command.</param>
        /// <param name="cookies">Cookies to be sent with the DELETE command.</param>
        /// <param name="totalTimeout">Total Request Timeout in ms.</param>
        /// <param name="connectTimeout">Connect Timeout in ms.</param>
        /// <param name="readTimeout">Read Timeout in ms.</param>
        /// <param name="writeTimeout">Write Timeout in ms.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> DeleteAsync(string url, bool isKeepAlive = true, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null,
            int totalTimeout = 60000, int connectTimeout = 45000, int readTimeout = 10000, int writeTimeout = 10000)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
            }, () =>
            {
                return SendDeleteCommandAsync(isKeepAlive, headers, cookies);
            },
            url, isKeepAlive, connectTimeout, readTimeout, writeTimeout).ExecuteTaskWithTimeout(this, totalTimeout, CancellationTokenSourceManager);
        }

        /// <summary>
        /// Connects to the Destination Server.
        /// </summary>
        protected internal override void SendConnectCommand()
        {
            var destinationAddressBytes = GetDestinationAddressBytes();
            var destinationPortBytes = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, userIdBytes);

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
            var destinationAddressBytes = await GetDestinationAddressBytesAsync();
            var destinationPortBytes = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, userIdBytes);

            await Socket.SendAsync(request, WriteTimeout, CancellationTokenSourceManager);

            var response = new byte[8];
            await Socket.ReceiveAsync(response, ReadTimeout, CancellationTokenSourceManager);

            if (response[1] != Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_GRANTED)
                HandleProxyCommandError(response);

            if (Scheme == ProxyScheme.HTTPS)
                await HandleSslHandshakeAsync();
        }

        /// <summary>
        /// Gets the Destination Port in bytes.
        /// </summary>
        /// <returns>Destination Port Bytes</returns>
        protected byte[] GetDestinationPortBytes() =>
           new byte[2]
           {
                Convert.ToByte(DestinationPort / 256),
                Convert.ToByte(DestinationPort % 256)
           };

        /// <summary>
        /// Handles the command request response error.
        /// </summary>
        /// <param name="response">Command Request Response</param>
        protected void HandleProxyCommandError(byte[] response)
        {
            var replyCode = response[1];
            string proxyErrorText = replyCode switch
            {
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_OR_FAILED => "connection request was rejected or failed",
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD => "connection request was rejected because SOCKS destination cannot connect to identd on the client",
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD => "connection request rejected because the client program and identd report different user-ids",
                _ => String.Format(CultureInfo.InvariantCulture, "proxy client received an unknown reply with the code value '{0}' from the proxy destination", replyCode.ToString(CultureInfo.InvariantCulture)),
            };

            throw new ProxyException(String.Format(CultureInfo.InvariantCulture, $"Proxy error: {proxyErrorText} for destination host {DestinationHost} port number {DestinationPort}."));
        }

        #region Private Methods
        private byte[] GetDestinationAddressBytes()
        {
            try
            {
                var hostAddress = Dns.GetHostAddresses(DestinationHost).First();
                return hostAddress.GetAddressBytes();
            }
            catch (Exception)
            {
                throw new ProxyException($"No such known host for: {DestinationHost}.");
            }
        }

        private async Task<byte[]> GetDestinationAddressBytesAsync()
        {
            try
            {
                var hostAddress = await Dns.GetHostAddressesAsync(DestinationHost);
                return hostAddress.First().GetAddressBytes();
            }
            catch (Exception)
            {
                throw new ProxyException($"No such known host for: {DestinationHost}.");
            }
        }

        private byte[] GetCommandRequest(byte[] destinationAddressBytes, byte[] destinationPortBytes, byte[] userIdBytes)
        {
            var request = new byte[9 + userIdBytes.Length];

            request[0] = Socks4Constants.SOCKS4_VERSION_NUMBER;
            request[1] = Socks4Constants.SOCKS4_CMD_CONNECT;
            destinationPortBytes.CopyTo(request, 2);
            destinationAddressBytes.CopyTo(request, 4);
            userIdBytes.CopyTo(request, 8);
            request[8 + userIdBytes.Length] = 0x00;

            return request;
        }
        #endregion
    }
}

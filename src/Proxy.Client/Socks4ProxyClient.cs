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
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client
{
    /// <summary>
    /// Socks4 connection proxy class. This class implements the Socks4 standard proxy protocol.
    /// </summary>
    public sealed class Socks4ProxyClient : BaseProxyClient
    {
        /// <summary>
        /// Proxy user identification information.
        /// </summary>
        public string ProxyUserId { get; }

        /// <summary>
        /// Stream used for SSL connections.
        /// </summary>
        private SslStream _sslStream;

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
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response</returns>
        public override ProxyResponse Get(string url, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
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
                return SendConnectCommandAsync();
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
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendPostCommand(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the POST request.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PostAsync(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
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
            return HandleRequest(() =>
            {
                SendConnectCommand();
            }, () =>
            {
                return SendPutCommand(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the PUT command to the destination server and returns the response.
        /// </summary>
        /// <param name="url">Destination URL.</param>
        /// <param name="body">Body to be sent with the PUT request.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response</returns>
        public override Task<ProxyResponse> PutAsync(string url, string body, IEnumerable<ProxyHeader> headers = null, IEnumerable<Cookie> cookies = null)
        {
            return HandleRequestAsync(() =>
            {
                return SendConnectCommandAsync();
            }, () =>
            {
                return SendPutCommandAsync(body, headers, cookies);
            }, url);
        }

        /// <summary>
        /// Disposes the socket dependencies.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _sslStream?.Dispose();
        }

        /// <summary>
        /// Sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleRequestCommand((ssl) => 
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);
                _sslStream.Write(writeBuffer);
               return _sslStream.ReceiveAll();
            },
            (ssl) => 
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            });
        }

        /// <summary>
        /// Asynchronously sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                return await _sslStream.ReceiveAllAsync();
            },
            async (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationUri.AbsoluteUri, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            });
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
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                _sslStream.Write(writeBuffer);
                return _sslStream.ReceiveAll();
            },
            (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            });
        }

        /// <summary>
        /// Asynchronously sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                return await _sslStream.ReceiveAllAsync();
            },
            async (ssl) => 
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            });
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
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                _sslStream.Write(writeBuffer);
                return _sslStream.ReceiveAll();
            },
           (ssl) =>
           {
               var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
               Socket.Send(writeBuffer);
               return Socket.ReceiveAll();
           });
        }

        /// <summary>
        /// Asynchronously sends the PUT command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override Task<(ProxyResponse response, float firstByteTime)> SendPutCommandAsync(string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                return await _sslStream.ReceiveAllAsync();
            },
            async (ssl) =>
            {
                var writeBuffer = CommandHelper.PutCommand(DestinationUri.AbsoluteUri, body, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            });
        }

        #region Private Methods
        private void SendConnectCommand()
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

        private async Task SendConnectCommandAsync()
        {
            var destinationAddressBytes = await GetDestinationAddressBytesAsync();
            var destinationPortBytes = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, userIdBytes);

            await Socket.SendAsync(request);

            var response = new byte[8];
            await Socket.ReceiveAsync(response, response.Length);

            if (response[1] != Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_GRANTED)
                HandleProxyCommandError(response);

            if (Scheme == ProxyScheme.HTTPS)
                await HandleSslHandshakeAsync();
        }

        private void HandleProxyCommandError(byte[] response)
        {
            var replyCode = response[1];
            string proxyErrorText = replyCode switch
            {
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_OR_FAILED => "connection request was rejected or failed",
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD => "connection request was rejected because SOCKS destination cannot connect to identd on the client",
                Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD => "connection request rejected because the client program and identd report different user-ids",
                _ => String.Format(CultureInfo.InvariantCulture, "proxy client received an unknown reply with the code value '{0}' from the proxy destination", replyCode.ToString(CultureInfo.InvariantCulture)),
            };
            var exceptionMsg = String.Format(CultureInfo.InvariantCulture, $"Proxy error: {proxyErrorText} for destination host {DestinationHost} port number {DestinationPort}.");

            throw new ProxyException(exceptionMsg);
        }

        private byte[] GetDestinationAddressBytes()
        {
            try
            {
                var hostAddress = Dns.GetHostAddresses(DestinationHost).First();
                return hostAddress.GetAddressBytes();
            }
            catch (Exception)
            {
                throw new ProxyException($"No such known host for: {DestinationHost}");
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
                throw new ProxyException($"No such known host for: {DestinationHost}");
            }
        }

        private byte[] GetDestinationPortBytes() =>
            new byte[2]
            {
                Convert.ToByte(DestinationPort / 256),
                Convert.ToByte(DestinationPort % 256)
            };

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

        private (ProxyResponse response, float firstByteTime) HandleRequestCommand(Func<string, (string response, float firstByteTime)> sslFn, Func<string, (string response, float firstByteTime)> noSslFn)
        {
            var (response, firstByteTime) = Scheme == ProxyScheme.HTTPS
                ? sslFn(DestinationUri.Scheme)
                : noSslFn(DestinationUri.Scheme);

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        private async Task<(ProxyResponse response, float firstByteTime)> HandleRequestCommandAsync(Func<string, Task<(string response, float firstByteTime)>> sslFn, Func<string, Task<(string response, float firstByteTime)>> noSslFn)
        {
            var (response, firstByteTime) = Scheme == ProxyScheme.HTTPS
                ? await sslFn(DestinationUri.Scheme)
                : await noSslFn(DestinationUri.Scheme);

            return (ResponseBuilderHelper.BuildProxyResponse(response, DestinationUri), firstByteTime);
        }

        #region Ssl Methods
        private void HandleSslHandshake()
        {
            var networkStream = new NetworkStream(Socket);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            _sslStream.AuthenticateAsClient(DestinationHost);
        }

        private async Task HandleSslHandshakeAsync()
        {
            var networkStream = new NetworkStream(Socket);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            await _sslStream.AuthenticateAsClientAsync(DestinationHost);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None;
        #endregion
        #endregion
    }
}

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
    public sealed class Socks4ProxyClient : BaseProxyClient
    {
        public string ProxyUserId { get; }

        private SslStream _sslStream;

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
        }

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
        }

        public override ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand(isSsl);
            }, () =>
            {
                return SendGetCommand(headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return await HandleRequestAsync(async () =>
            {
                await SendConnectCommandAsync(isSsl);
            }, async () =>
            {
                return await SendGetCommandAsync(headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return HandleRequest(() =>
            {
                SendConnectCommand(isSsl);
            }, () =>
            {
                return SendPostCommand(body, headers, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return await HandleRequestAsync(async () =>
            {
                await SendConnectCommandAsync(isSsl);
            }, async () =>
            {
                return await SendPostCommandAsync(body, headers, isSsl);
            }, destinationHost, destinationPort);
        }

        private void SendConnectCommand(bool isSsl)
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

            if (isSsl)
                HandleSslHandshake();
        }

        private async Task SendConnectCommandAsync(bool isSsl)
        {
            var destinationAddressBytes = await GetDestinationAddressBytesAsync();
            var destinationPortBytes = GetDestinationPortBytes();
            var userIdBytes = Encoding.ASCII.GetBytes(ProxyUserId);

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, userIdBytes);

            await Socket.SendAsync(request, SocketFlags.None);

            var response = new byte[8];
            await Socket.ReceiveAsync(response, SocketFlags.None);

            if (response[1] != Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_GRANTED)
                HandleProxyCommandError(response);

            if (isSsl)
                await HandleSslHandshakeAsync();
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IDictionary<string, string> headers, bool isSsl)
        {
            string response;
            float firstByteTime;

            if (isSsl)
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestConstants.SSL, headers);
                _sslStream.Write(writeBuffer);

                (response, firstByteTime) = _sslStream.ReceiveAll();
            }
            else
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestConstants.NO_SSL, headers);
                Socket.Send(writeBuffer);

                (response, firstByteTime) = Socket.ReceiveAll(SocketFlags.None);
            }

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IDictionary<string, string> headers, bool isSsl)
        {
            string response;
            float firstByteTime;

            if (isSsl)
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestConstants.SSL, headers);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                (response, firstByteTime) = await _sslStream.ReceiveAllAsync();
            }
            else
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestConstants.NO_SSL, headers);
                await Socket.SendAsync(writeBuffer, SocketFlags.None);

                (response, firstByteTime) = await Socket.ReceiveAllAsync(SocketFlags.None);
            }

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IDictionary<string, string> headers, bool isSsl)
        {
            string response;
            float firstByteTime;

            if (isSsl)
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestConstants.SSL, headers);
                _sslStream.Write(writeBuffer);

                (response, firstByteTime) = _sslStream.ReceiveAll();
            }
            else
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestConstants.NO_SSL, headers);
                Socket.Send(writeBuffer);

                (response, firstByteTime) = Socket.ReceiveAll(SocketFlags.None);
            }

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IDictionary<string, string> headers, bool isSsl)
        {
            string response;
            float firstByteTime;

            if (isSsl)
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestConstants.SSL, headers);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                (response, firstByteTime) = await _sslStream.ReceiveAllAsync();
            }
            else
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestConstants.NO_SSL, headers);
                await Socket.SendAsync(writeBuffer, SocketFlags.None);

                (response, firstByteTime) = await Socket.ReceiveAllAsync(SocketFlags.None);
            }

            return (ResponseBuilder.BuildProxyResponse(response), firstByteTime);
        }

        private void HandleProxyCommandError(byte[] response)
        {
            var replyCode = response[1];

            string proxyErrorText;

            switch (replyCode)
            {
                case Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_OR_FAILED:
                    proxyErrorText = "connection request was rejected or failed";
                    break;
                case Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD:
                    proxyErrorText = "connection request was rejected because SOCKS destination cannot connect to identd on the client";
                    break;
                case Socks4Constants.SOCKS4_CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD:
                    proxyErrorText = "connection request rejected because the client program and identd report different user-ids";
                    break;
                default:
                    proxyErrorText = String.Format(CultureInfo.InvariantCulture, "proxy client received an unknown reply with the code value '{0}' from the proxy destination", replyCode.ToString(CultureInfo.InvariantCulture));
                    break;
            }

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

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                  SslPolicyErrors sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None ? true : false;
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _sslStream.Dispose();
        }
    }
}

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
    public sealed class Socks5ProxyClient : BaseProxyClient
    {
        public string ProxyUsername { get; }
        public string ProxyPassword { get; }

        private static byte[] AuthRequest =>
            new byte[4]
            {
                Socks5Constants.SOCKS5_VERSION_NUMBER,
                Socks5Constants.SOCKS5_AUTH_NUMBER_OF_AUTH_METHODS_SUPPORTED,
                Socks5Constants.SOCKS5_AUTH_METHOD_NO_AUTHENTICATION_REQUIRED,
                Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD
            };
        private byte[] AuthCrendetials
        {
            get
            {
                var credentials = new byte[ProxyUsername.Length + ProxyPassword.Length + 3];

                credentials[0] = 0x01;
                credentials[1] = (byte)ProxyUsername.Length;
                Array.Copy(Encoding.ASCII.GetBytes(ProxyUsername), 0, credentials, 2, ProxyUsername.Length);
                credentials[ProxyUsername.Length + 2] = (byte)ProxyPassword.Length;
                Array.Copy(Encoding.ASCII.GetBytes(ProxyPassword), 0, credentials, ProxyUsername.Length + 3, ProxyPassword.Length);

                return credentials;
            }
        }
        private SocksAuthentication _proxyAuthMethod;
        private SslStream _sslStream;

        public Socks5ProxyClient(string proxyHost, int proxyPort)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
        }

        public Socks5ProxyClient(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            if (String.IsNullOrEmpty(proxyHost))
                throw new ArgumentNullException(nameof(proxyHost));

            if (proxyPort <= 0 || proxyPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(proxyPort),
                    "Proxy port must be greater than zero and less than 65535");

            if (proxyUsername == null)
                throw new ArgumentNullException(nameof(proxyUsername));

            if (proxyPassword == null)
                throw new ArgumentNullException(nameof(proxyPassword));

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUsername = proxyUsername;
            ProxyPassword = proxyPassword;
        }

        public override ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return HandleRequest(() =>
            {
                DetermineClientAuthMethod();
                NegotiateServerAuthMethod();
                SendConnectCommand(isSsl);
            }, () => 
            {
                return SendGetCommand(headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override async Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return await HandleRequestAsync(async () =>
            {
                DetermineClientAuthMethod();
                await NegotiateServerAuthMethodAsync();
                await SendConnectCommandAsync(isSsl);
            }, async () => 
            {
                return await SendGetCommandAsync(headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return HandleRequest(() => 
            {
                DetermineClientAuthMethod();
                NegotiateServerAuthMethod();
                SendConnectCommand(isSsl);
            }, () => 
            {
                return SendPostCommand(body, headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        public override Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, IEnumerable<Cookie> cookies = null, bool isSsl = false)
        {
            return HandleRequestAsync(async () => 
            {
                DetermineClientAuthMethod();
                await NegotiateServerAuthMethodAsync();
                await SendConnectCommandAsync(isSsl);
            }, async () => 
            {
                return await SendPostCommandAsync(body, headers, cookies, isSsl);
            }, destinationHost, destinationPort);
        }

        private void SendConnectCommand(bool isSsl)
        {
            var addressType = GetDestinationAddressType();
            var destinationAddressBytes = GetDestinationAddressBytes(addressType);
            var destinationPortBytes = GetDestinationPortBytes();

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, addressType);

            Socket.Send(request, SocketFlags.None);

            var response = new byte[10];
            Socket.Receive(response, SocketFlags.None);

            var replyCode = response[1];

            if (replyCode != Socks5Constants.SOCKS5_CMD_REPLY_SUCCEEDED)
                HandleProxyCommandError(response);

            if (isSsl)
                HandleSslHandshake();
        }

        private async Task SendConnectCommandAsync(bool isSsl)
        {
            var addressType = GetDestinationAddressType();
            var destinationAddressBytes = await GetDestinationAddressBytesAsync(addressType);
            var destinationPortBytes = GetDestinationPortBytes();

            var request = GetCommandRequest(destinationAddressBytes, destinationPortBytes, addressType);

            await Socket.SendAsync(request);

            var response = new byte[10];
            await Socket.ReceiveAsync(response, response.Length);

            var replyCode = response[1];

            if (replyCode != Socks5Constants.SOCKS5_CMD_REPLY_SUCCEEDED)
                HandleProxyCommandError(response);

            if (isSsl)
                await HandleSslHandshakeAsync();
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;

                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);

                _sslStream.Write(writeBuffer);

                (response, firstByteTime) = _sslStream.ReceiveAll();
            }
            else
            {
                ssl = RequestConstants.NO_SSL;

                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);

                Socket.Send(writeBuffer);

                (response, firstByteTime) = Socket.ReceiveAll();
            }

            return (ResponseBuilder.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;

                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);

                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                (response, firstByteTime) = await _sslStream.ReceiveAllAsync();
            }
            else
            {
                ssl = RequestConstants.NO_SSL;

                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);

                await Socket.SendAsync(writeBuffer);

                (response, firstByteTime) = await Socket.ReceiveAllAsync();
            }

            return (ResponseBuilder.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;

                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);

                _sslStream.Write(writeBuffer);

                (response, firstByteTime) = _sslStream.ReceiveAll();
            }
            else
            {
                ssl = RequestConstants.NO_SSL;

                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);

                Socket.Send(writeBuffer);

                (response, firstByteTime) = Socket.ReceiveAll();
            }

            return (ResponseBuilder.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;

                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);

                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                (response, firstByteTime) = await _sslStream.ReceiveAllAsync();
            }
            else
            {
                ssl = RequestConstants.NO_SSL;

                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);

                await Socket.SendAsync(writeBuffer);

                (response, firstByteTime) = await Socket.ReceiveAllAsync();
            }

            return (ResponseBuilder.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        private void HandleProxyCommandError(byte[] response)
        {
            var replyCode = response[1];

            string proxyErrorText;

            switch (replyCode)
            {
                case Socks5Constants.SOCKS5_CMD_REPLY_GENERAL_SOCKS_SERVER_FAILURE:
                    proxyErrorText = "a general socks destination failure occurred";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_CONNECTION_NOT_ALLOWED_BY_RULESET:
                    proxyErrorText = "the connection is not allowed by proxy destination rule set";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_NETWORK_UNREACHABLE:
                    proxyErrorText = "the network was unreachable";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_HOST_UNREACHABLE:
                    proxyErrorText = "the host was unreachable";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_CONNECTION_REFUSED:
                    proxyErrorText = "the connection was refused by the remote network";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_TTL_EXPIRED:
                    proxyErrorText = "the time to live (TTL) has expired";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_COMMAND_NOT_SUPPORTED:
                    proxyErrorText = "the command issued by the proxy client is not supported by the proxy destination";
                    break;
                case Socks5Constants.SOCKS5_CMD_REPLY_ADDRESS_TYPE_NOT_SUPPORTED:
                    proxyErrorText = "the address type specified is not supported";
                    break;
                default:
                    proxyErrorText = String.Format(CultureInfo.InvariantCulture, "an unknown SOCKS reply with the code value '{0}' was received", replyCode.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            var responseText = response != null ? response.HexEncode() : string.Empty;
            var exceptionMsg = String.Format(CultureInfo.InvariantCulture, $"Proxy error: {proxyErrorText} for destination host {DestinationHost} port number {DestinationPort}.  Server response (hex): {responseText}.");

            throw new ProxyException(exceptionMsg);
        }

        private void DetermineClientAuthMethod() =>
            _proxyAuthMethod = ProxyUsername != null && ProxyPassword != null
                ? SocksAuthentication.UsernamePassword
                : SocksAuthentication.None;

        private void NegotiateServerAuthMethod()
        {
            Socket.Send(AuthRequest, SocketFlags.None);

            var response = new byte[2];
            Socket.Receive(response, SocketFlags.None);

            var acceptedAuthMethod = response[1];

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_REPLY_NO_ACCEPTABLE_METHODS)
            {
                Socket.Close();
                throw new ProxyException("The proxy destination does not accept the supported proxy client authentication methods.");
            }

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD && _proxyAuthMethod == SocksAuthentication.None)
            {
                Socket.Close();
                throw new ProxyException("The proxy destination requires a username and password for authentication. " +
                    "If you received this error attempting to connect to the Tor network provide an string empty value for ProxyUserName and ProxyPassword.");
            }

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD)
            {
                Socket.Send(AuthCrendetials, SocketFlags.None);

                var crResponse = new byte[2];
                Socket.Receive(crResponse, SocketFlags.None);

                if (crResponse[1] != 0)
                {
                    Socket.Close();
                    throw new ProxyException("Proxy authentification failure! The proxy server has reported that the userid and/or password is not valid.");
                }
            }
        }

        private async Task NegotiateServerAuthMethodAsync()
        {
            await Socket.SendAsync(AuthRequest);

            var responseBuffer = new byte[2];
            await Socket.ReceiveAsync(responseBuffer, responseBuffer.Length);

            var acceptedAuthMethod = responseBuffer[1];

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_REPLY_NO_ACCEPTABLE_METHODS)
            {
                Socket.Close();
                throw new ProxyException("The proxy destination does not accept the supported proxy client authentication methods.");
            }

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD && _proxyAuthMethod == SocksAuthentication.None)
            {
                Socket.Close();
                throw new ProxyException("The proxy destination requires a username and password for authentication. " +
                    "If you received this error attempting to connect to the Tor network provide an string empty value for ProxyUserName and ProxyPassword.");
            }

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD)
            {
                await Socket.SendAsync(AuthCrendetials);

                var crResponse = new byte[2];
                await Socket.ReceiveAsync(crResponse, crResponse.Length);

                Console.WriteLine("SOCKS5_AUTH_METHOD_USERNAME_PASSWORD");

                if (crResponse[1] != 0)
                {
                    Socket.Close();
                    throw new ProxyException("Proxy authentification failure! The proxy server has reported that the userid and/or password is not valid.");
                }
            }
        }

        #region SendCommand Byte Private Methods
        private byte GetDestinationAddressType()
        {
            bool result = IPAddress.TryParse(DestinationHost, out var ipAddress);

            if (!result)
                return Socks5Constants.SOCKS5_ADDRTYPE_DOMAIN_NAME;

            switch(ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return Socks5Constants.SOCKS5_ADDRTYPE_IPV4;
                case AddressFamily.InterNetworkV6:
                    return Socks5Constants.SOCKS5_ADDRTYPE_IPV6;
                default:
                    throw new Exception(String.Format(CultureInfo.InvariantCulture,
                        $"The host addess {DestinationHost} of type '{Enum.GetName(typeof(AddressFamily), ipAddress.AddressFamily)}' is not a supported address type.  The supported types are InterNetwork and InterNetworkV6."));
            }
        }

        private byte[] GetDestinationAddressBytes(byte addressType)
        {
            switch (addressType)
            {
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV4:
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV6:
                    try
                    {
                        var hostAddress = Dns.GetHostAddresses(DestinationHost).First();
                        return hostAddress.GetAddressBytes();
                    }
                    catch (Exception)
                    {
                        throw new ProxyException($"No such known host for: {DestinationHost}");
                    }
                case Socks5Constants.SOCKS5_ADDRTYPE_DOMAIN_NAME:
                    byte[] bytes = new byte[DestinationHost.Length + 1];
                    bytes[0] = Convert.ToByte(DestinationHost.Length);
                    Encoding.ASCII.GetBytes(DestinationHost).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private async Task<byte[]> GetDestinationAddressBytesAsync(byte addressType)
        {
            switch (addressType)
            {
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV4:
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV6:
                    try
                    {
                        var hostAddress = await Dns.GetHostAddressesAsync(DestinationHost);
                        return hostAddress.First().GetAddressBytes();
                    }
                    catch (Exception)
                    {
                        throw new ProxyException($"No such known host for: {DestinationHost}");
                    }
                case Socks5Constants.SOCKS5_ADDRTYPE_DOMAIN_NAME:
                    byte[] bytes = new byte[DestinationHost.Length + 1];
                    bytes[0] = Convert.ToByte(DestinationHost.Length);
                    Encoding.ASCII.GetBytes(DestinationHost).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private byte[] GetDestinationPortBytes() =>
            new byte[2]
            {
                Convert.ToByte(DestinationPort / 256),
                Convert.ToByte(DestinationPort % 256)
            };

        private byte[] GetCommandRequest(byte[] destinationAddressBytes, byte[] destinationPortBytes, byte addressType)
        {
            var request = new byte[4 + destinationAddressBytes.Length + 2];
            
            request[0] = Socks5Constants.SOCKS5_VERSION_NUMBER;
            request[1] = Socks5Constants.SOCKS5_CMD_CONNECT;
            request[2] = Socks5Constants.SOCKS5_RESERVED;
            request[3] = addressType;
            destinationAddressBytes.CopyTo(request, 4);
            destinationPortBytes.CopyTo(request, 4 + destinationAddressBytes.Length);

            return request;
        }
        #endregion

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
            _sslStream?.Dispose();
        }
    }
}

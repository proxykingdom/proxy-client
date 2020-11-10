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
    /// Socks5 connection proxy class. This class implements the Socks5 standard proxy protocol.
    /// </summary>
    public sealed class Socks5ProxyClient : BaseProxyClient
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
        /// Authentication Request used to negotiate with the Proxy Server.
        /// </summary>
        private static byte[] AuthRequest =>
            new byte[4]
            {
                Socks5Constants.SOCKS5_VERSION_NUMBER,
                Socks5Constants.SOCKS5_AUTH_NUMBER_OF_AUTH_METHODS_SUPPORTED,
                Socks5Constants.SOCKS5_AUTH_METHOD_NO_AUTHENTICATION_REQUIRED,
                Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD
            };

        /// <summary>
        /// Authentication Credentials used to authenticate with the Proxy Server.
        /// </summary>
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

        /// <summary>
        /// Type of Socks5 Authentication.
        /// </summary>
        private Socks5Authentication _proxyAuthMethod;

        /// <summary>
        /// Stream used for SSL connections.
        /// </summary>
        private SslStream _sslStream;

        /// <summary>
        /// Creates a Socks5 proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
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

        /// <summary>
        /// Creates a Socks5 proxy client object.
        /// </summary>
        /// <param name="proxyHost">Host name or IP address of the proxy server.</param>
        /// <param name="proxyPort">Port used to connect to proxy server.</param>
        /// <param name="proxyUsername">Proxy Username used to connec to the Proxy Server.</param>
        /// <param name="proxyPassword">Proxy Password used to connec to the Proxy Server.</param>
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

        /// <summary>
        /// Connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
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

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the GET command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
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

        /// <summary>
        /// Connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
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

        /// <summary>
        /// Asynchronously connects to the proxy client, sends the POST command to the destination server and returns the response.
        /// </summary>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <param name="destinationPort">Port used to connect to the destination server.</param>
        /// <param name="body">Body to be sent with the POST request.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response</returns>
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

        /// <summary>
        /// Sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendGetCommand(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                _sslStream.Write(writeBuffer);
                return _sslStream.ReceiveAll();
            },
            (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            }, isSsl);
        }

        /// <summary>
        /// Asynchronously sends the GET command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be sent with the GET command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendGetCommandAsync(IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return await HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                return await _sslStream.ReceiveAllAsync();
            },
            async (ssl) =>
            {
                var writeBuffer = CommandHelper.GetCommand(DestinationHost, ssl, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            }, isSsl);
        }

        /// <summary>
        /// Sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override (ProxyResponse response, float firstByteTime) SendPostCommand(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return HandleRequestCommand((ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                _sslStream.Write(writeBuffer);
                return _sslStream.ReceiveAll();
            },
            (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                Socket.Send(writeBuffer);
                return Socket.ReceiveAll();
            }, isSsl);
        }

        /// <summary>
        /// Asynchronously sends the POST command to the destination server, and creates the proxy response.
        /// </summary>
        /// <param name="body">Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <param name="isSsl">Indicates if the request will be http or https.</param>
        /// <returns>Proxy Response with the time to first byte</returns>
        protected internal override async Task<(ProxyResponse response, float firstByteTime)> SendPostCommandAsync(string body, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, bool isSsl)
        {
            return await HandleRequestCommandAsync(async (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                return await _sslStream.ReceiveAllAsync();
            },
            async (ssl) =>
            {
                var writeBuffer = CommandHelper.PostCommand(DestinationHost, body, ssl, headers, cookies);
                await Socket.SendAsync(writeBuffer);
                return await Socket.ReceiveAllAsync();
            }, isSsl);
        }

        #region Private Methods
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

        private void HandleProxyCommandError(byte[] response)
        {
            var replyCode = response[1];
            string proxyErrorText = replyCode switch
            {
                Socks5Constants.SOCKS5_CMD_REPLY_GENERAL_SOCKS_SERVER_FAILURE => "a general socks destination failure occurred",
                Socks5Constants.SOCKS5_CMD_REPLY_CONNECTION_NOT_ALLOWED_BY_RULESET => "the connection is not allowed by proxy destination rule set",
                Socks5Constants.SOCKS5_CMD_REPLY_NETWORK_UNREACHABLE => "the network was unreachable",
                Socks5Constants.SOCKS5_CMD_REPLY_HOST_UNREACHABLE => "the host was unreachable",
                Socks5Constants.SOCKS5_CMD_REPLY_CONNECTION_REFUSED => "the connection was refused by the remote network",
                Socks5Constants.SOCKS5_CMD_REPLY_TTL_EXPIRED => "the time to live (TTL) has expired",
                Socks5Constants.SOCKS5_CMD_REPLY_COMMAND_NOT_SUPPORTED => "the command issued by the proxy client is not supported by the proxy destination",
                Socks5Constants.SOCKS5_CMD_REPLY_ADDRESS_TYPE_NOT_SUPPORTED => "the address type specified is not supported",
                _ => String.Format(CultureInfo.InvariantCulture, "an unknown SOCKS reply with the code value '{0}' was received", replyCode.ToString(CultureInfo.InvariantCulture)),
            };
            var responseText = response != null ? HexEncode(response) : string.Empty;
            var exceptionMsg = String.Format(CultureInfo.InvariantCulture, $"Proxy error: {proxyErrorText} for destination host {DestinationHost} port number {DestinationPort}.  Server response (hex): {responseText}.");

            throw new ProxyException(exceptionMsg);
        }

        private (ProxyResponse response, float firstByteTime) HandleRequestCommand(Func<string, (string response, float firstByteTime)> sslFn, Func<string, (string response, float firstByteTime)> noSslFn, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;
                (response, firstByteTime) = sslFn(ssl);
            }
            else
            {
                ssl = RequestConstants.NO_SSL;
                (response, firstByteTime) = noSslFn(ssl);
            }

            return (ResponseBuilderHelper.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        private async Task<(ProxyResponse response, float firstByteTime)> HandleRequestCommandAsync(Func<string, Task<(string response, float firstByteTime)>> sslFn, Func<string, Task<(string response, float firstByteTime)>> noSslFn, bool isSsl)
        {
            string ssl;
            string response;
            float firstByteTime;

            if (isSsl)
            {
                ssl = RequestConstants.SSL;
                (response, firstByteTime) = await sslFn(ssl);
            }
            else
            {
                ssl = RequestConstants.NO_SSL;
                (response, firstByteTime) = await noSslFn(ssl);
            }

            return (ResponseBuilderHelper.BuildProxyResponse(response, ssl, DestinationHost), firstByteTime);
        }

        private void DetermineClientAuthMethod() =>
            _proxyAuthMethod = ProxyUsername != null && ProxyPassword != null
                ? Socks5Authentication.UsernamePassword
                : Socks5Authentication.None;

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

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD && _proxyAuthMethod == Socks5Authentication.None)
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

            if (acceptedAuthMethod == Socks5Constants.SOCKS5_AUTH_METHOD_USERNAME_PASSWORD && _proxyAuthMethod == Socks5Authentication.None)
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

            return ipAddress.AddressFamily switch
            {
                AddressFamily.InterNetwork => Socks5Constants.SOCKS5_ADDRTYPE_IPV4,
                AddressFamily.InterNetworkV6 => Socks5Constants.SOCKS5_ADDRTYPE_IPV6,
                _ => throw new Exception(String.Format(CultureInfo.InvariantCulture,
                            $"The host addess {DestinationHost} of type '{Enum.GetName(typeof(AddressFamily), ipAddress.AddressFamily)}' is not a supported address type. " +
                            $"The supported types are InterNetwork and InterNetworkV6.")),
            };
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

        private static string HexEncode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var buffer = new StringBuilder(data.Length * 2);

            for (int i = 0; i < data.Length; i++)
            {
                buffer.Append(data[i].ToString("x").PadLeft(2, '0'));
            }

            return buffer.ToString();
        }

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
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _sslStream?.Dispose();
        }
    }
}

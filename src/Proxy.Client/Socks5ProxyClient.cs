﻿using Proxy.Client.Contracts;
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
    public sealed class Socks5ProxyClient : BaseProxyClient, IProxyClient
    {
        public string ProxyHost { get; }
        public int ProxyPort { get; }
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
                throw new ArgumentNullException(nameof(ProxyPassword));

            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUsername = proxyUsername;
            ProxyPassword = proxyPassword;
        }

        public ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return HandleRequest(() =>
            {
                DetermineClientAuthMethod();
                NegotiateServerAuthMethod();
                SendConnectCommand(destinationHost, destinationPort);
                return SendGetCommand(destinationHost, headers, isSsl);

            }, destinationHost, destinationPort, ProxyHost, ProxyPort);
        }

        public async Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false)
        {
            return await HandleRequestAsync(async () =>
            {
                DetermineClientAuthMethod();
                await NegotiateServerAuthMethodAsync();
                await SendConnectCommandAsync(destinationHost, destinationPort);
                return await SendGetCommandAsync(destinationHost, headers, isSsl);

            }, destinationHost, destinationPort, ProxyHost, ProxyPort);
        }

        protected internal override void SendConnectCommand(string destinationHost, int destinationPort)
        {
            var command = Socks5Constants.SOCKS5_CMD_CONNECT;

            var addressType = GetDestinationAddressType(destinationHost);
            var destinationAddressBytes = GetDestinationAddressBytes(addressType, destinationHost);
            var destinationPortBytes = GetDestinationPortBytes(destinationPort);

            var request = GetCommandRequest(command, destinationAddressBytes, destinationPortBytes, addressType);

            Socket.Send(request, SocketFlags.None);

            var response = new byte[255]; //maybe put to 20 max
            Socket.Receive(response, SocketFlags.None);

            var replyCode = response[1];

            if (replyCode != Socks5Constants.SOCKS5_CMD_REPLY_SUCCEEDED)
                HandleProxyCommandError(response, destinationHost, destinationPort);
        }

        protected internal override async Task SendConnectCommandAsync(string destinationHost, int destinationPort)
        {
            var command = Socks5Constants.SOCKS5_CMD_CONNECT;

            var addressType = GetDestinationAddressType(destinationHost);
            var destinationAddressBytes = await GetDestinationAddressBytesAsync(addressType, destinationHost);
            var destinationPortBytes = GetDestinationPortBytes(destinationPort);

            var request = GetCommandRequest(command, destinationAddressBytes, destinationPortBytes, addressType);

            await Socket.SendAsync(request, SocketFlags.None);

            var response = new byte[255];
            await Socket.ReceiveAsync(response, SocketFlags.None);

            var replyCode = response[1];

            if (replyCode != Socks5Constants.SOCKS5_CMD_REPLY_SUCCEEDED)
                HandleProxyCommandError(response, destinationHost, destinationPort);
        }

        protected internal override void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort)
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
            var exceptionMsg = String.Format(CultureInfo.InvariantCulture, $"Proxy error: {proxyErrorText} for destination host {destinationHost} port number {destinationPort}.  Server response (hex): {responseText}.");

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
            await Socket.SendAsync(AuthRequest, SocketFlags.None);

            var response = new byte[2];
            await Socket.ReceiveAsync(response, SocketFlags.None);

            var acceptedAuthMethod = response[1];

            if (acceptedAuthMethod == Socks5Constants. SOCKS5_AUTH_METHOD_REPLY_NO_ACCEPTABLE_METHODS)
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
                await Socket.SendAsync(AuthCrendetials, SocketFlags.None);

                var crResponse = new byte[2];
                await Socket.ReceiveAsync(crResponse, SocketFlags.None);

                if (crResponse[1] != 0)
                {
                    Socket.Close();
                    throw new ProxyException("Proxy authentification failure! The proxy server has reported that the userid and/or password is not valid.");
                }
            }
        }

        #region SendCommand Byte Private Methods
        private byte GetDestinationAddressType(string destinationHost)
        {
            bool result = IPAddress.TryParse(destinationHost, out var ipAddress);

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
                        $"The host addess {destinationHost} of type '{Enum.GetName(typeof(AddressFamily), ipAddress.AddressFamily)}' is not a supported address type.  The supported types are InterNetwork and InterNetworkV6."));
            }
        }

        private byte[] GetDestinationAddressBytes(byte addressType, string destinationHost)
        {
            switch (addressType)
            {
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV4:
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV6:
                    var hostAddress = Dns.GetHostAddresses(destinationHost).FirstOrDefault();
                    return hostAddress.GetAddressBytes();
                case Socks5Constants.SOCKS5_ADDRTYPE_DOMAIN_NAME:
                    byte[] bytes = new byte[destinationHost.Length + 1];
                    bytes[0] = Convert.ToByte(destinationHost.Length);
                    Encoding.ASCII.GetBytes(destinationHost).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private async Task<byte[]> GetDestinationAddressBytesAsync(byte addressType, string destinationHost)
        {
            switch (addressType)
            {
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV4:
                case Socks5Constants.SOCKS5_ADDRTYPE_IPV6:
                    var hostAddress = await Dns.GetHostAddressesAsync(destinationHost);
                    return hostAddress.FirstOrDefault().GetAddressBytes();
                case Socks5Constants.SOCKS5_ADDRTYPE_DOMAIN_NAME:
                    byte[] bytes = new byte[destinationHost.Length + 1];
                    bytes[0] = Convert.ToByte(destinationHost.Length);
                    Encoding.ASCII.GetBytes(destinationHost).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private byte[] GetDestinationPortBytes(int destinationPort) =>
            new byte[2]
            {
                Convert.ToByte(destinationPort / 256),
                Convert.ToByte(destinationPort % 256)
            };

        private byte[] GetCommandRequest(byte command, byte[] destinationAddressBytes, byte[] destinationPortBytes, byte addressType)
        {
            var request = new byte[4 + destinationAddressBytes.Length + 2];
            
            request[0] = Socks5Constants.SOCKS5_VERSION_NUMBER;
            request[1] = command;
            request[2] = Socks5Constants.SOCKS5_RESERVED;
            request[3] = addressType;
            destinationAddressBytes.CopyTo(request, 4);
            destinationPortBytes.CopyTo(request, 4 + destinationAddressBytes.Length);

            return request;
        }

        #endregion
    }
}
using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using Proxy.Client.Utilities;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client
{
    public abstract class BaseProxyClient : IProxyClient
    {
        public string ProxyHost { get; protected set; }
        public int ProxyPort { get; protected set; }

        protected internal Socket Socket { get; private set; }
        protected internal bool IsConnected { get; private set; }
        protected internal string DestinationHost { get; private set; }
        protected internal int DestinationPort { get; private set; }

        private SslStream _sslStream;

        public abstract ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);
        public abstract Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);
        public abstract ProxyResponse Post(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false);
        public abstract Task<ProxyResponse> PostAsync(string destinationHost, int destinationPort, string body, IDictionary<string, string> headers = null, bool isSsl = false);

        protected internal abstract void SendConnectCommand();
        protected internal abstract Task SendConnectCommandAsync();
        protected internal abstract void HandleProxyCommandError(byte[] response);

        protected internal ProxyResponse HandleRequest(Action notConnectedAtn, Func<ProxyResponse> connectedFn,
            string destinationHost, int destinationPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                if (!IsConnected)
                {
                    DestinationHost = destinationHost;
                    DestinationPort = destinationPort;

                    Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    Socket.Connect(ProxyHost, ProxyPort);

                    notConnectedAtn();

                    IsConnected = true;
                }

                return connectedFn();
            }
            catch (Exception)
            {
                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed."));
            }
        }

        protected internal async Task<ProxyResponse> HandleRequestAsync(Func<Task> notConnectedFn, Func<Task<ProxyResponse>> connectedFn,
            string destinationHost, int destinationPort)
        {
            try
            {
                if (String.IsNullOrEmpty(destinationHost))
                    throw new ArgumentNullException(nameof(destinationHost));

                if (destinationPort <= 0 || destinationPort > 65535)
                    throw new ArgumentOutOfRangeException(nameof(destinationPort),
                        "Destination port must be greater than zero and less than 65535");

                if (!IsConnected)
                {
                    DestinationHost = destinationHost;
                    DestinationPort = destinationPort;

                    Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    await Socket.ConnectAsync(ProxyHost, ProxyPort);

                    await notConnectedFn();

                    IsConnected = true;
                }

                return await connectedFn();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                throw new ProxyException(String.Format(CultureInfo.InvariantCulture,
                    $"Connection to proxy host {ProxyHost} on port {ProxyPort} failed."));
            }
        }

        protected internal ProxyResponse SendGetCommand(IDictionary<string, string> headers, bool isSsl)
        {
            string response;

            if (isSsl)
            {
                HandleSslHandshake(DestinationHost);

                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestHelper.Ssl, headers);
                _sslStream.Write(writeBuffer);

                response = _sslStream.ReadString(Socket);
            }
            else
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestHelper.NoSsl, headers);
                Socket.Send(writeBuffer);

                response = Socket.ReceiveAll(SocketFlags.None);
            }
            

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        protected internal async Task<ProxyResponse> SendGetCommandAsync(IDictionary<string, string> headers, bool isSsl)
        {
            string response;

            if (isSsl)
            {
                await HandleSslHandshakeAsync();

                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestHelper.Ssl, headers);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                response = await _sslStream.ReadStringAsync(Socket);
            }
            else
            {
                var writeBuffer = RequestHelper.GetCommand(DestinationHost, RequestHelper.NoSsl, headers);
                await Socket.SendAsync(writeBuffer, SocketFlags.None);

                response = await Socket.ReceiveAllAsync(SocketFlags.None);
            }

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        protected internal ProxyResponse SendPostCommand(string body, IDictionary<string, string> headers, bool isSsl)
        {
            string response;

            if (isSsl)
            {
                HandleSslHandshake(DestinationHost);

                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestHelper.Ssl, headers);
                _sslStream.Write(writeBuffer);

                response = _sslStream.ReadString(Socket);
            }
            else
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestHelper.NoSsl, headers);
                Socket.Send(writeBuffer);

                response = Socket.ReceiveAll(SocketFlags.None);
            }

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        protected internal async Task<ProxyResponse> SendPostCommandAsync(string body, IDictionary<string, string> headers, bool isSsl)
        {
            string response;

            if (isSsl)
            {
                await HandleSslHandshakeAsync();

                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestHelper.Ssl, headers);
                _sslStream.Write(writeBuffer);

                response = _sslStream.ReadString(Socket);
            }
            else
            {
                var writeBuffer = RequestHelper.PostCommand(DestinationHost, body, RequestHelper.NoSsl, headers);
                await Socket.SendAsync(writeBuffer, SocketFlags.None);

                response = await Socket.ReceiveAllAsync(SocketFlags.None);
            }

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        #region Ssl Methods
        private void HandleSslHandshake(string destinationHost)
        {
            var networkStream = new NetworkStream(Socket);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            _sslStream.AuthenticateAsClient(destinationHost);
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

        public void Dispose()
        {
            Socket?.Close();
            _sslStream?.Dispose();
        }
    }
}

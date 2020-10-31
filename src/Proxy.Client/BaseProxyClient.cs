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

        protected internal abstract void SendConnectCommand();
        protected internal abstract Task SendConnectCommandAsync(string destinationHost, int destinationPort);
        protected internal abstract void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort);

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
                _sslStream.Write(CreateGetCommand(DestinationHost, headers, isSsl));

                response = _sslStream.ReadAll(Socket);
            }
            else
            {
                Socket.Send(CreateGetCommand(DestinationHost, headers, isSsl), SocketFlags.None);

                response = Socket.ReceiveAll(SocketFlags.None);
            }
            

            if (response.StartsWith("\0\0"))
                throw new ProxyException("Response is empty");

            return ResponseBuilder.BuildProxyResponse(response);
        }

        protected internal async Task<ProxyResponse> SendGetCommandAsync(string destinationHost, IDictionary<string, string> headers, bool isSsl)
        {
            string response;

            if (isSsl)
            {
                await HandSslHandshakeAsync(destinationHost);

                var writeBuffer = CreateGetCommand(destinationHost, headers, isSsl);
                await _sslStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                response = await _sslStream.ReadAllAsync(Socket);
            }
            else
            {
                await Socket.SendAsync(CreateGetCommand(destinationHost, headers, isSsl), SocketFlags.None);

                response = await Socket.ReceiveAllAsync(SocketFlags.None);
            }

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

        #region Ssl Methods
        private void HandleSslHandshake(string destinationHost)
        {
            var networkStream = new NetworkStream(Socket);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            _sslStream.AuthenticateAsClient(destinationHost);
        }

        private async Task HandSslHandshakeAsync(string destinationHost)
        {
            var networkStream = new NetworkStream(Socket);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            await _sslStream.AuthenticateAsClientAsync(destinationHost);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                  SslPolicyErrors sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None ? true : false;

        #endregion

        public void Dispose()
        {
            Socket?.Close();
            _sslStream?.Dispose();
        }

        public abstract ProxyResponse Get(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);
        public abstract Task<ProxyResponse> GetAsync(string destinationHost, int destinationPort, IDictionary<string, string> headers = null, bool isSsl = false);
    }
}

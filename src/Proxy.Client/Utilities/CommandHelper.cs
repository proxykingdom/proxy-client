using Proxy.Client.Contracts;
using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Proxy.Client.Utilities
{
    /// <summary>
    /// Helper class to create the request commands.
    /// </summary>
    internal static class CommandHelper
    {
        /// <summary>
        /// Creates the raw GET bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL.</param>
        /// <param name="host">Destination Host.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be send with the GET command.</param>
        /// <returns>Raw GET bytes request</returns>
        public static byte[] GetCommand(string absoluteUri, string host, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((keepAliveString, headerString, cookieString) =>
            {
                return $"GET {absoluteUri} HTTP/1.1\r\n" +
                       $"Host: {host}\r\n" +
                       headerString +
                       $"{keepAliveString}\r\n" +
                       $"{cookieString}\r\n";
            }, isKeepAlive, headers, cookies);
        }

        /// <summary>
        /// Creates the raw CONNECT bytes request.
        /// </summary>
        /// <param name="host">Destination Host.</param>
        /// <returns>Raw CONNECT bytes request</returns>
        public static byte[] ConnectCommand(string host)
        {
            var cmd = $"CONNECT {host}:443 HTTP/1.1\r\nHost: {host}:443\r\n\r\n";
            return Encoding.ASCII.GetBytes(cmd);
        }

        /// <summary>
        /// Creates the raw CONNECT bytes request with Basic Proxy Authentication.
        /// </summary>
        /// <param name="host">Destination Host.</param>
        /// <param name="proxyUsername">Proxy Username used to connect to the Proxy Server.</param>
        /// <param name="proxyPassword">Proxy Password used to connect to the Proxy Server.</param>
        /// <returns>Raw CONNECT bytes request</returns>
        public static byte[] ConnectProxyAuthCommand(string host, string proxyUsername, string proxyPassword)
        {
            var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{proxyUsername}:{proxyPassword}"));
            var cmd = $"CONNECT {host}:443 HTTP/1.1\r\nHost: {host}:443\r\nProxy-Authorization: Basic {base64Credentials}\r\n\r\n";
            return Encoding.ASCII.GetBytes(cmd);
        }

        /// <summary>
        /// Creates the raw POST bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL.</param>
        /// <param name="host">Destination Host.</param>
        /// <param name="body">Request Body to be sent with the POST command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Raw POST bytes request</returns>
        public static byte[] PostCommand(string absoluteUri, string host, string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((keepAliveString, headerString, cookieString) =>
            {
                return $"POST {absoluteUri} HTTP/1.1\r\n" +
                       $"Host: {host}\r\n" +
                       $"Content-Length: {body.Length}\r\n" +
                       headerString + 
                       $"{keepAliveString}\r\n" + 
                       $"{cookieString}\r\n" + 
                       body;
            }, isKeepAlive, headers, cookies);
        }

        /// <summary>
        /// Creates the raw PUT bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL.</param>
        /// <param name="host">Destination Host.</param>
        /// <param name="body">Request Body to be sent with the PUT command.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Raw PUT bytes request</returns>
        public static byte[] PutCommand(string absoluteUri, string host, string body, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((keepAliveString, headerString, cookieString) =>
            {
                return $"PUT {absoluteUri} HTTP/1.1" +
                       $"Host: {host}\r\n" +
                       $"Content-Length: {body.Length}" +
                       headerString +
                       $"{keepAliveString}\r\n" +
                       $"{cookieString}\r\n" +
                       body;
            }, isKeepAlive, headers, cookies);
        }

        /// <summary>
        /// Creates the raw DELETE bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL.</param>
        /// <param name="host">Destination Host.</param>
        /// <param name="isKeepAlive">Indicates whether the connetion is to be disposed or kept alive.</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be send with the GET command.</param>
        /// <returns>Raw DELETE bytes request</returns>
        public static byte[] DeleteCommand(string absoluteUri, string host, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((keepAliveString, headerString, cookieString) =>
            {
                return $"DELETE {absoluteUri} HTTP/1.1" +
                       $"Host: {host}\r\n" +
                       headerString +
                       $"{keepAliveString}\r\n" +
                       $"{cookieString}\r\n";
            }, isKeepAlive, headers, cookies);
        }

        private static byte[] HandleCommand(Func<string, string, string, string> fn, bool isKeepAlive, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var keepAliveString = isKeepAlive
                ? "Connection: keep-alive"
                : "Connection: close";

            var headerString = headers != null
                ? headers.ConcatenateHeaders()
                : string.Empty;

            var cookieString = cookies != null
                ? cookies.ConcatenateCookies() + Environment.NewLine
                : string.Empty;

            var command = fn(keepAliveString, headerString, cookieString);

            return Encoding.ASCII.GetBytes(command);
        }
    }
}

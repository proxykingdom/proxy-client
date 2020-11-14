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
        /// <param name="absoluteUri">Full destination request URL</param>
        /// <param name="headers">Headers to be sent with the GET command.</param>
        /// <param name="cookies">Cookies to be send with the GET command.</param>
        /// <returns>Raw GET bytes request</returns>
        public static byte[] GetCommand(string absoluteUri, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((headerString, cookieString) =>
            {
                var cmd = new StringBuilder();

                cmd.AppendLine($"GET {absoluteUri} HTTP/1.1");
                cmd.Append(headerString);
                cmd.AppendLine(cookieString);

                return cmd.ToString();
            }, headers, cookies);
        }

        /// <summary>
        /// Creates the raw POST bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL</param>
        /// <param name="body">Request Body to be sent with the POST command.</param>
        /// <param name="headers">Headers to be sent with the POST command.</param>
        /// <param name="cookies">Cookies to be sent with the POST command.</param>
        /// <returns>Raw POST bytes request</returns>
        public static byte[] PostCommand(string absoluteUri, string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((headerString, cookieString) =>
            {
                var cmd = new StringBuilder();

                cmd.AppendLine($"POST {absoluteUri} HTTP/1.1");
                cmd.AppendLine($"Content-Length: {body.Length}");
                cmd.Append(headerString);
                cmd.AppendLine(cookieString);
                cmd.Append(body);

                return cmd.ToString();
            }, headers, cookies);
        }

        /// <summary>
        /// Creates the raw PUT bytes request.
        /// </summary>
        /// <param name="absoluteUri">Full destination request URL</param>
        /// <param name="body">Request Body to be sent with the PUT command.</param>
        /// <param name="headers">Headers to be sent with the PUT command.</param>
        /// <param name="cookies">Cookies to be sent with the PUT command.</param>
        /// <returns>Raw PUT bytes request</returns>
        public static byte[] PutCommand(string absoluteUri, string body, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((headerString, cookieString) =>
            {
                var cmd = new StringBuilder();

                cmd.AppendLine($"PUT {absoluteUri} HTTP/1.1");
                cmd.AppendLine($"Content-Length: {body.Length}");
                cmd.Append(headerString);
                cmd.AppendLine(cookieString);
                cmd.Append(body);

                return cmd.ToString();
            }, headers, cookies);
        }

        private static byte[] HandleCommand(Func<string, string, string> fn, IEnumerable<ProxyHeader> headers, IEnumerable<Cookie> cookies)
        {
            var headerString = headers != null
                ? headers.ConcatenateHeaders()
                : string.Empty;

            var cookieString = cookies != null
                ? cookies.ConcatenateCookies() + Environment.NewLine
                : string.Empty;

            var command = fn(headerString, cookieString);

            return Encoding.ASCII.GetBytes(command);
        }
    }
}

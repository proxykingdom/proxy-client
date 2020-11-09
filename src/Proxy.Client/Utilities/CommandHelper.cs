using Proxy.Client.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Proxy.Client.Utilities
{
    public static class CommandHelper
    {
        public static byte[] GetCommand(string destHost, string ssl, IDictionary<string, string> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((headerString, cookieString) =>
            {
                var cmdString = new StringBuilder();

                cmdString.AppendLine($"GET {ssl}://{destHost}/ HTTP/1.1");
                cmdString.Append(headerString);
                cmdString.AppendLine(cookieString);

                return cmdString.ToString();
            }, headers, cookies);
        }

        public static byte[] PostCommand(string destHost, string body, string ssl, IDictionary<string, string> headers, IEnumerable<Cookie> cookies)
        {
            return HandleCommand((headerString, cookieString) =>
            {
                var cmdString = new StringBuilder();

                cmdString.AppendLine($"POST {ssl}://{destHost}/ HTTP/1.1");
                cmdString.AppendLine($"Content-Length: {body.Length}");
                cmdString.Append(headerString);
                cmdString.AppendLine(cookieString);
                cmdString.Append(body);

                return cmdString.ToString();
            }, headers, cookies);
        }

        public static byte[] HandleCommand(Func<string, string, string> fn, IDictionary<string, string> headers, IEnumerable<Cookie> cookies)
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

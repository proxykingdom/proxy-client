using Proxy.Client.Utilities.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Proxy.Client.Utilities
{
    public static class RequestHelper
    {
        public static byte[] GetCommand(string destHost, string ssl, IDictionary<string, string> headers)
        {
            var command = headers == null 
                ? $"GET {ssl}://{destHost}/ HTTP/1.1\r\n\r\n"
                : $"GET {ssl}://{destHost}/ HTTP/1.1\r\n {headers.ConcatenateHeadersKvp()}\r\n";

            return Encoding.ASCII.GetBytes(command);
        }

        public static byte[] PostCommand(string destHost, string body, string ssl, IDictionary<string, string> headers)
        {
            var command = headers == null 
                ? $"POST {ssl}://{destHost}/ HTTP/1.1\r\nContent-Length: {body.Length}\r\n\r\n{body}"
                : $"POST {ssl}://{destHost}/ HTTP/1.1\r\nContent-Length: {body.Length}\r\n{headers.ConcatenateHeadersKvp()}\r\n{body}";

            return Encoding.ASCII.GetBytes(command);
        }
    }
}

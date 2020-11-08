using Proxy.Client.Contracts.Constants;
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
                ? $"GET {ssl}://{destHost}/ HTTP/1.1{RequestConstants.CONTENT_SEPERATOR}"
                : $"GET {ssl}://{destHost}/ HTTP/1.1{RequestConstants.NEW_LINE} {headers.ConcatenateHeadersKvp()}{RequestConstants.NEW_LINE}";

            return Encoding.ASCII.GetBytes(command);
        }

        public static byte[] PostCommand(string destHost, string body, string ssl, IDictionary<string, string> headers)
        {
            var command = headers == null 
                ? $"POST {ssl}://{destHost}/ HTTP/1.1{RequestConstants.NEW_LINE}Content-Length: {body.Length}{RequestConstants.CONTENT_SEPERATOR}{body}"
                : $"POST {ssl}://{destHost}/ HTTP/1.1{RequestConstants.NEW_LINE}Content-Length: {body.Length}{RequestConstants.NEW_LINE}{headers.ConcatenateHeadersKvp()}{RequestConstants.NEW_LINE}{body}";

            return Encoding.ASCII.GetBytes(command);
        }
    }
}

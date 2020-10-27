using System.Collections.Generic;
using System.Net;

namespace Proxy.Client.Contracts
{
    public class ProxyResponse
    {
        public HttpStatusCode StatusCode { get; }
        public IDictionary<string, string> ResponseHeaders { get; }
        public string Content { get; }

        private ProxyResponse(HttpStatusCode statusCode, IDictionary<string, string> responseHeaders, string content)
        {
            StatusCode = statusCode;
            ResponseHeaders = responseHeaders;
            Content = content;
        }

        public static ProxyResponse Create(HttpStatusCode statusCode, IDictionary<string, string> responseHeaders, string content)
        {
            return new ProxyResponse(statusCode, responseHeaders, content);
        }
    }
}

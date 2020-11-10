using System.Collections.Generic;
using System.Net;

namespace Proxy.Client.Contracts
{
    public class ProxyResponse
    {
        public HttpStatusCode StatusCode { get; }
        public IDictionary<string, string> Headers { get; }
        public IEnumerable<Cookie> Cookies { get; }
        public string Content { get; }
        public Timings Timings { get; internal set; }

        private ProxyResponse(HttpStatusCode statusCode, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, string content)
        {
            StatusCode = statusCode;
            Headers = headers;
            Cookies = cookies;
            Content = content;
        }

        public static ProxyResponse Create(HttpStatusCode statusCode, IDictionary<string, string> headers, IEnumerable<Cookie> cookies, string content)
        {
            return new ProxyResponse(statusCode, headers, cookies, content);
        }
    }
}

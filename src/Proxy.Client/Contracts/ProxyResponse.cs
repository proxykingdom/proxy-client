using System.Collections.Generic;
using System.Net;

namespace Proxy.Client.Contracts
{
    /// <summary>
    /// Proxy Response class.
    /// </summary>
    public class ProxyResponse
    {
        /// <summary>
        /// Response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Response headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; }
        
        /// <summary>
        /// Response cookies.
        /// </summary>
        public IEnumerable<Cookie> Cookies { get; }

        /// <summary>
        /// Response content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Proxy time measurements.
        /// </summary>
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

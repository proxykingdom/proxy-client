using Proxy.Client.Contracts;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Proxy.Client.Utilities
{
    public static class ResponseBuilder
    {
        public static ProxyResponse BuildProxyResponse(string response)
        {
            var splitResponse = response.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

            var responseHeadersHtml = splitResponse[0];

            var subResponseHeadersHtml = responseHeadersHtml.Substring(0, responseHeadersHtml.Length - 4);
            var statusWithHeaders = subResponseHeadersHtml.Split(new[] {"\r\n"}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, @"\d\d\d").Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var headerArray = statusWithHeaders.Skip(1).ToArray();
            var headers = headerArray.Select(header => header.Split(':')).ToDictionary(key => key[0], value => value[1].Trim());

            return new ProxyResponse
            {
                StatusCode = status,
                ResponseHeaders = headers,
                Content = splitResponse[1],
                Timings = new Timings()
            };
        }
    }
}

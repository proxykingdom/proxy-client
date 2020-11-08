using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
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
            var splitResponse = response.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var responseHeadersHtml = splitResponse[0];
            var statusWithHeaders = responseHeadersHtml.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, RequestConstants.STATUS_CODE_PATTERN).Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var headerArray = statusWithHeaders.Skip(1).ToArray();
            var headers = headerArray.Select(header =>
            {
                var splitValues = header.Split(new[] { ":" }, 2, StringSplitOptions.None);
                return ResponseHeader.Create(splitValues[0], splitValues[1].Trim());
            });

            return new ProxyResponse
            {
                StatusCode = status,
                ResponseHeaders = headers,
                Content = splitResponse[1]
            };
        }
    }
}

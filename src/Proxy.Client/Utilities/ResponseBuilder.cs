using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Cookie = Proxy.Client.Contracts.Cookie;

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

            var headerArray = statusWithHeaders.Skip(1);
            var headerDict = new Dictionary<string, string>();
            var cookies = new List<Cookie>();

            foreach (var header in headerArray)
            {
                var headerPair = header.Split(':');

                if (headerPair[0].Contains(RequestConstants.COOKIE_HEADER))
                    cookies.Add(Cookie.Create(headerPair[0], headerPair[2]));

                headerDict.Add(headerPair[0], headerPair[1]);
            }

            return new ProxyResponse
            {
                StatusCode = status,
                Headers = headerDict,
                Cookies = cookies,
                Content = splitResponse[1]
            };
        }
    }
}

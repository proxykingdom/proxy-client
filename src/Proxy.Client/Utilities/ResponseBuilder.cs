using Proxy.Client.Contracts;
using Proxy.Client.Contracts.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Proxy.Client.Utilities
{
    public static class ResponseBuilder
    {
        public static ProxyResponse BuildProxyResponse(string response, string ssl, string destinationHost)
        {
            var splitResponse = response.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var responseHeadersHtml = splitResponse[0];
            var statusWithHeaders = responseHeadersHtml.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, RequestConstants.STATUS_CODE_PATTERN).Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var headerArray = statusWithHeaders.Skip(1);
            var destinationUri = new Uri($"{ssl}://{destinationHost}");
            var headerDict = new Dictionary<string, string>();
            var cookieContainer = new CookieContainer();

            foreach (var header in headerArray)
            {
                var headerPair = header.Split(new[] { ": " }, 2, StringSplitOptions.None);

                if (headerPair[0].Contains(RequestConstants.SET_COOKIE_HEADER))
                {
                    cookieContainer.SetCookies(destinationUri, headerPair[1]);
                    continue;
                }

                headerDict.Add(headerPair[0], headerPair[1]);
            }

            return ProxyResponse.Create(status, headerDict, cookieContainer.GetCookies(destinationUri) as IEnumerable<Cookie>, splitResponse[1]);
        }
    }
}

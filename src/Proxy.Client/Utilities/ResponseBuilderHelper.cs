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
    /// <summary>
    /// Helper class to build the given raw response.
    /// </summary>
    internal static class ResponseBuilderHelper
    {
        /// <summary>
        /// Parses the raw response into a proxy response object.
        /// </summary>
        /// <param name="response">Raw response returned by the destionation server.</param>
        /// <param name="destinationUri">Destination URI object.</param>
        /// <returns>Proxy Response</returns>
        public static ProxyResponse BuildProxyResponse(string response, Uri destinationUri)
        {
            var splitResponse = response.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            if (splitResponse.Length == 1)
            {
                var status = ParseStatusCode(splitResponse[0]);

                return ProxyResponse.Create(status, Enumerable.Empty<ProxyHeader>(), Enumerable.Empty<Cookie>(), splitResponse[0]);
            }
            else
            {
                var responseHeadersHtml = splitResponse[0];
                var statusWithHeaders = responseHeadersHtml.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var statusHtml = statusWithHeaders[0];
                var status = ParseStatusCode(statusHtml);

                var headerList = new List<ProxyHeader>();
                var cookieContainer = new CookieContainer();

                var headerArray = statusWithHeaders.Skip(1);

                foreach (var header in headerArray)
                {
                    var headerPair = header.Split(new[] { ": " }, 2, StringSplitOptions.None);

                    if (headerPair[0].ToLower().Contains(RequestConstants.SET_COOKIE_HEADER))
                    {
                        cookieContainer.SetCookies(destinationUri, headerPair[1]);
                        continue;
                    }

                    headerList.Add(ProxyHeader.Create(headerPair[0], headerPair[1]));
                }

                return ProxyResponse.Create(status, headerList, cookieContainer.GetCookies(destinationUri), splitResponse[1]);
            }
        }

        private static HttpStatusCode ParseStatusCode(string content)
        {
            var statusNumber = Convert.ToInt32(Regex.Match(content, RequestConstants.STATUS_CODE_PATTERN).Value, CultureInfo.InvariantCulture);
            return (HttpStatusCode)statusNumber;
        }
    }
}

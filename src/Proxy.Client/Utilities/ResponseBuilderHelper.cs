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
        /// <param name="ssl">Http or Https.</param>
        /// <param name="destinationHost">Host name or IP address of the destination server.</param>
        /// <returns>Proxy Response</returns>
        public static ProxyResponse BuildProxyResponse(string response, string ssl, string destinationHost)
        {
            var splitResponse = response.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var responseHeadersHtml = splitResponse[0];
            var statusWithHeaders = responseHeadersHtml.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, RequestConstants.STATUS_CODE_PATTERN).Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var destinationUri = new Uri($"{ssl}://{destinationHost}");
            var headerDict = new Dictionary<string, string>();
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

                headerDict.Add(headerPair[0], headerPair[1]);
            }

            return ProxyResponse.Create(status, headerDict, cookieContainer.GetCookies(destinationUri) as IEnumerable<Cookie>, splitResponse[1]);
        }
    }
}

using HtmlAgilityPack;
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
        private static readonly HtmlDocument _document;

        static ResponseBuilder()
        {
            _document = new HtmlDocument();
        }

        public static ProxyResponse BuildProxyResponse(string response)
        {
            _document.LoadHtml(response);

            var responseHeadersHtml = _document.DocumentNode.FirstChild.OuterHtml;
            var contentHtml = _document.DocumentNode.ChildNodes[1].OuterHtml;

            var subResponseHeadersHtml = responseHeadersHtml.Substring(0, responseHeadersHtml.Length - 4);
            var statusWithHeaders = subResponseHeadersHtml.Split(new[] {"\r\n"}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, @"\d\d\d").Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var headerArray = statusWithHeaders.Skip(1).ToArray();
            var headers = headerArray.Select(header => header.Split(':')).ToDictionary(key => key[0], value => value[1].Trim());

            return ProxyResponse.Create(status, headers, contentHtml);
        }
    }
}

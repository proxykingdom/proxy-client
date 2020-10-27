using HtmlAgilityPack;
using Proxy.Client.Contracts;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Proxy.Client.Utilities
{
    public static class ResponseBuilder
    {
        private static readonly HtmlDocument _document;
        private static readonly StringBuilder _stringBuilder;

        static ResponseBuilder()
        {
            _document = new HtmlDocument();
            _stringBuilder = new StringBuilder();
        }

        public static ProxyResponse BuildProxyResponse(string response)
        {
            _document.LoadHtml(response);

            var responseHeadersHtml = _document.DocumentNode.FirstChild.OuterHtml;

            for (int i = 1; i < _document.DocumentNode.ChildNodes.Count; i++)
            {
                _stringBuilder.Append(_document.DocumentNode.ChildNodes[i].InnerHtml);
            }

            var content = _stringBuilder.ToString();

            var subResponseHeadersHtml = responseHeadersHtml.Substring(0, responseHeadersHtml.Length - 4);
            var statusWithHeaders = subResponseHeadersHtml.Split(new[] {"\r\n"}, StringSplitOptions.None);

            var statusHtml = statusWithHeaders[0];
            var statusNumber = Convert.ToInt32(Regex.Match(statusHtml, @"\d\d\d").Value, CultureInfo.InvariantCulture);
            var status = (HttpStatusCode)statusNumber;

            var headerArray = statusWithHeaders.Skip(1).ToArray();
            var headers = headerArray.Select(header => header.Split(':')).ToDictionary(key => key[0], value => value[1].Trim());

            return ProxyResponse.Create(status, headers, content);
        }
    }
}

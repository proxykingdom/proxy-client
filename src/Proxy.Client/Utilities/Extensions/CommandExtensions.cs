using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Proxy.Client.Utilities.Extensions
{
    public static class CommandExtensions
    {
        public static string ConcatenateHeaders(this IDictionary<string, string> dict)
        {
            var headerString = new StringBuilder();

            foreach (var item in dict)
            {
                headerString.Append($"{item.Key}: {item.Value}{Environment.NewLine}");
            }

            return headerString.ToString();
        }

        public static string ConcatenateCookies(this IEnumerable<Cookie> cookies)
        {
            var cookieString = new StringBuilder();

            cookieString.Append("Cookie: ");

            foreach (var cookie in cookies)
            {
                cookieString.Append($"{cookie};");
            }

            return cookieString.ToString();
        }
    }
}

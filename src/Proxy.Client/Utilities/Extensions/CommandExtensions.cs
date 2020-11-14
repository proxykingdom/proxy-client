using Proxy.Client.Contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Proxy.Client.Utilities.Extensions
{
    /// <summary>
    /// Command Extension class.
    /// </summary>
    internal static class CommandExtensions
    {
        /// <summary>
        /// Concatenates a header dictionary into a formatted HTTP request string.
        /// </summary>
        /// <param name="headers">Header list.</param>
        /// <returns>Concatenated header string</returns>
        public static string ConcatenateHeaders(this IEnumerable<ProxyHeader> headers)
        {
            var headerString = new StringBuilder();

            foreach (var header in headers)
            {
                headerString.Append($"{header.Name}: {header.Value}{Environment.NewLine}");
            }

            return headerString.ToString();
        }

        /// <summary>
        /// Concatenates a list of cookies into a formatted HTTP request string.
        /// </summary>
        /// <param name="cookies">List of cookies.</param>
        /// <returns>Concatenated cookie string</returns>
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

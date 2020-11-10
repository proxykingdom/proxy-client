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
        /// <param name="dict">Header dictionary.</param>
        /// <returns>Concatenated header string</returns>
        public static string ConcatenateHeaders(this IDictionary<string, string> dict)
        {
            var headerString = new StringBuilder();

            foreach (var item in dict)
            {
                headerString.Append($"{item.Key}: {item.Value}{Environment.NewLine}");
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

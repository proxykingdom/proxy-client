using System;
using System.Collections.Generic;
using System.Text;

namespace Proxy.Client.Utilities.Extensions
{
    public static class DictExtensions
    {
        public static string ConcatenateHeadersKvp(this IDictionary<string, string> dict)
        {
            var headerString = new StringBuilder();

            foreach (var item in dict)
            {
                headerString.Append($"{item.Key}: {item.Value}{Environment.NewLine}");
            }

            return headerString.ToString();
        }
    }
}

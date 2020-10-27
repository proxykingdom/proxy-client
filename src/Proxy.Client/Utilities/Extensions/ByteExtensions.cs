using System;
using System.Text;

namespace Proxy.Client.Utilities.Extensions
{
    internal static class ByteExtensions
    {
        internal static string HexEncode(this byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var buffer = new StringBuilder(data.Length * 2);

            for (int i = 0; i < data.Length; i++)
            {
                buffer.Append(data[i].ToString("x").PadLeft(2, '0'));
            }

            return buffer.ToString();
        }
    }
}

using Proxy.Client.Contracts.Constants;
using Proxy.Client.Exceptions;
using System;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class SocketTaskExtensions
    {
        private const int StringBufferSize = 500;

        private static readonly StringBuilder _placeHolder;

        static SocketTaskExtensions()
        {
            _placeHolder = new StringBuilder();
        }

        public static string ReceiveAll(this Socket s, SocketFlags flags)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            s.Receive(buffer, flags);

            var bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
            _placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                s.Receive(buffer, flags);
                bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
                _placeHolder.Append(bufferString);
            }

            var maybeContentLength = Regex.Match(_placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            if (String.IsNullOrEmpty(maybeContentLength))
                throw new ProxyException("Destination Server has no Content-Length header.");

            var contentLength = Convert.ToInt32(maybeContentLength, CultureInfo.InvariantCulture);

            totalBytesRead += splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = s.Receive(buffer, flags);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static async Task<string> ReceiveAllAsync(this Socket s, SocketFlags flags)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            await s.ReceiveAsync(buffer, flags);

            var bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
            _placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                await s.ReceiveAsync(buffer, flags);
                bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
                _placeHolder.Append(bufferString);
            }

            var maybeContentLength = Regex.Match(_placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            if (String.IsNullOrEmpty(maybeContentLength))
                throw new ProxyException("Destination Server has no Content-Length header.");

            var contentLength = Convert.ToInt32(maybeContentLength, CultureInfo.InvariantCulture);

            totalBytesRead += splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await s.ReceiveAsync(buffer, flags);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static string ReadAll(this SslStream ss)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            ss.Read(buffer, 0, buffer.Length);

            var bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
            _placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                ss.Read(buffer, 0, buffer.Length);
                bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
                _placeHolder.Append(bufferString);
            }

            var maybeContentLength = Regex.Match(_placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            if (String.IsNullOrEmpty(maybeContentLength))
                throw new ProxyException("Destination Server has no Content-Length header.");

            var contentLength = Convert.ToInt32(maybeContentLength, CultureInfo.InvariantCulture);

            totalBytesRead += splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static async Task<string> ReadAllAsync(this SslStream ss)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            await ss.ReadAsync(buffer, 0, buffer.Length);

            var bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
            _placeHolder.Append(bufferString);

            while(!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                await ss.ReadAsync(buffer, 0, buffer.Length);
                bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
                _placeHolder.Append(bufferString);
            }

            var maybeContentLength = Regex.Match(_placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            if (String.IsNullOrEmpty(maybeContentLength))
                throw new ProxyException("Destination Server has no Content-Length header.");

            var contentLength = Convert.ToInt32(maybeContentLength, CultureInfo.InvariantCulture);

            totalBytesRead += splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static Task<int> SendAsync(this Socket s, byte[] buffer, SocketFlags flags)
        {
            return Task.Factory.FromAsync(
                s.BeginSend(buffer, 0, buffer.Length, flags, null, null),
                s.EndSend
            );
        }

        public static Task<int> ReceiveAsync(this Socket s, byte[] buffer, SocketFlags flags)
        {
            return Task.Factory.FromAsync(
                s.BeginReceive(buffer, 0, buffer.Length, flags, null, null),
                s.EndReceive
            );
        }
    }
}

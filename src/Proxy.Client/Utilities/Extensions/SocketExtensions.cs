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
        private const string ContentLengthPattern = "(?<=Content-Length: ).\\d+";

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

            var (contentLength, readSize) = ExtractContentLengthWithReadByteSize(buffer);

            totalBytesRead += readSize;

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

            var (contentLength, readSize) = ExtractContentLengthWithReadByteSize(buffer);

            totalBytesRead += readSize;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await s.ReceiveAsync(buffer, flags);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static string ReadString(this SslStream ss)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            ss.Read(buffer, 0, buffer.Length);

            var (contentLength, readSize) = ExtractContentLengthWithReadByteSize(buffer);

            totalBytesRead += readSize;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                _placeHolder.Append(Encoding.UTF8.GetString(buffer, 0, innerBytesRead));
            }

            return _placeHolder.ToString();
        }

        public static async Task<string> ReadStringAsync(this SslStream ss)
        {
            var totalBytesRead = 0;
            var buffer = new byte[StringBufferSize];
            await ss.ReadAsync(buffer, 0, buffer.Length);

            var (contentLength, readSize) = ExtractContentLengthWithReadByteSize(buffer);

            totalBytesRead += readSize;

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

        private static (int contentLength, int readSize) ExtractContentLengthWithReadByteSize(byte[] buffer)
        {
            var bufferString = Encoding.UTF8.GetString(buffer).Trim('\0');
            var contentLength = Convert.ToInt32(Regex.Match(bufferString, ContentLengthPattern).Value, CultureInfo.InvariantCulture);
            var splitBuffer = bufferString.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

            _placeHolder.Append(bufferString);

            return (contentLength, splitBuffer[1].Length);
        }
    }
}

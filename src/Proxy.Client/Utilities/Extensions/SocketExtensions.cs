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
        private const int BufferSize = 500;
        private const int PeekBufferSize = 10;

        public static (string response, float firstByteTime) ReceiveAll(this Socket s)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = TimingHelper.Measure(() => 
            {
                bytesRead = s.Receive(buffer); 
            });

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = s.Receive(buffer);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) s.DecodeContentLength(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) s.DecodeChunked(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server");

            return (placeHolder.ToString(), firstByteTime);
        }

        public static async Task<(string response, float firstByteTime)> ReceiveAllAsync(this Socket s)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = await TimingHelper.MeasureAsync(async () => 
            {
                bytesRead = await s.ReceiveAsync(buffer, buffer.Length);
            });

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = await s.ReceiveAsync(buffer, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) await s.DecodeContentLengthAsync(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) await s.DecodeChunkedAsync(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server");

            return (placeHolder.ToString(), firstByteTime);
        }

        public static (string response, float firstByteTime) ReceiveAll(this SslStream ss)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = TimingHelper.Measure(() => 
            {
                bytesRead = ss.Read(buffer, 0, buffer.Length); 
            });

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) ss.DecodeContentLength(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) ss.DecodeChunked(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server");

            return (placeHolder.ToString(), firstByteTime);
        }

        public static async Task<(string response, float firstByteTime)> ReceiveAllAsync(this SslStream ss)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = await TimingHelper.MeasureAsync(async () =>
            {
                bytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
            });

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while(!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                bytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) await ss.DecodeContentLengthAsync(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) await ss.DecodeChunkedAsync(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server");

            return (placeHolder.ToString(), firstByteTime);
        }

        #region Content Decoding Methods

        #region Content Length Methods
        private static string DecodeContentLength(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var (contentLength, totalBytesRead) = ExtractContentLength(placeHolder, bufferString);

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = s.Receive(buffer, SocketFlags.None);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }

            return placeHolder.ToString();
        }

        private static async Task<string> DecodeContentLengthAsync(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var (contentLength, totalBytesRead) = ExtractContentLength(placeHolder, bufferString);

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await s.ReceiveAsync(buffer, buffer.Length);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }

            return placeHolder.ToString();
        }

        private static void DecodeContentLength(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var (contentLength, totalBytesRead) = ExtractContentLength(placeHolder, bufferString);

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }
        }

        private static async Task DecodeContentLengthAsync(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var (contentLength, totalBytesRead) = ExtractContentLength(placeHolder, bufferString);

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }
        }

        private static (int contentLength, int bytesRead) ExtractContentLength(StringBuilder placeHolder, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.RemoveEmptyEntries);

            var contentLengthString = Regex.Match(placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var contentLength = Convert.ToInt32(contentLengthString, CultureInfo.InvariantCulture);
            var totalBytesRead = splitBuffer.Length == 1 ? 0 : splitBuffer[1].Length;

            return (contentLength, totalBytesRead);
        }
        #endregion

        #region Chunked Methods
        private static void DecodeChunked(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (splitBuffer.Length == 1)
            {
                var peekBytesRead = s.Receive(buffer, PeekBufferSize, SocketFlags.None);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

            placeHolder.Append(bufferString);
            var (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);

            while (chunkSize != 0)
            {
                while (totalBytesRead < chunkSize)
                {
                    var remainingReadSize = chunkSize - totalBytesRead;
                    var readSize = remainingReadSize > BufferSize ? BufferSize : remainingReadSize;

                    var innerBytesRead = s.Receive(buffer, readSize, SocketFlags.None);
                    totalBytesRead += innerBytesRead;
                    placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
                }

                var peekBytesRead = s.Receive(buffer, PeekBufferSize, SocketFlags.None);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);

                (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);
            }
        }

        private static async Task DecodeChunkedAsync(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (splitBuffer.Length == 1)
            {
                var peekBytesRead = await s.ReceiveAsync(buffer, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

            placeHolder.Append(bufferString);
            var (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);

            while (chunkSize != 0)
            {
                while (totalBytesRead < chunkSize)
                {
                    var remainingReadSize = chunkSize - totalBytesRead;
                    var readSize = remainingReadSize > BufferSize ? BufferSize : remainingReadSize;

                    var innerBytesRead = await s.ReceiveAsync(buffer, readSize);
                    totalBytesRead += innerBytesRead;
                    placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
                }

                var peekBytesRead = await s.ReceiveAsync(buffer, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);

                (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);
            }
        }

        private static void DecodeChunked(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (splitBuffer.Length == 1)
            {
                var peekBytesRead = ss.Read(buffer, 0, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

            placeHolder.Append(bufferString);
            var (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);

            while (chunkSize != 0)
            {
                while (totalBytesRead < chunkSize)
                {
                    var remainingReadSize = chunkSize - totalBytesRead;
                    var readSize = remainingReadSize > BufferSize ? BufferSize : remainingReadSize;

                    var innerBytesRead = ss.Read(buffer, 0, readSize);
                    totalBytesRead += innerBytesRead;
                    placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
                }

                var peekBytesRead = ss.Read(buffer, 0, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);

                (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);
            }
        }

        private static async Task DecodeChunkedAsync(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (splitBuffer.Length == 1)
            {
                var peekBytesRead = await ss.ReadAsync(buffer, 0, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

            placeHolder.Append(bufferString);
            var (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);

            while (chunkSize != 0)
            {
                while (totalBytesRead < chunkSize)
                {
                    var remainingReadSize = chunkSize - totalBytesRead;
                    var readSize = remainingReadSize > BufferSize ? BufferSize : remainingReadSize;

                    var innerBytesRead = await ss.ReadAsync(buffer, 0, readSize);
                    totalBytesRead += innerBytesRead;
                    placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
                }

                var peekBytesRead = await ss.ReadAsync(buffer, 0, PeekBufferSize);
                bufferString = Encoding.ASCII.GetString(buffer, 0, peekBytesRead);

                (chunkSize, totalBytesRead) = ExtractChunkSize(bufferString);
            }
        }

        private static (int chunkSize, int bytesRead) ExtractChunkSize(string newContentLine)
        {
            var foundNewLine = newContentLine.IndexOf(Environment.NewLine);

            if (foundNewLine == -1)
                return (0, 0);

            var chunkSizeString = newContentLine.Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = newContentLine.Length - (foundNewLine + 4);

            return (chunkSize, totalBytesRead);
        }
        #endregion

        #endregion

        #region APM to Task Methods
        public static Task<int> SendAsync(this Socket s, byte[] buffer)
        {
            return Task.Factory.FromAsync(
                s.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                s.EndSend
            );
        }

        public static Task<int> ReceiveAsync(this Socket s, byte[] buffer, int count)
        {
            return Task.Factory.FromAsync(
                s.BeginReceive(buffer, 0, count, SocketFlags.None, null, null),
                s.EndReceive
            );
        }
        #endregion
    }
}

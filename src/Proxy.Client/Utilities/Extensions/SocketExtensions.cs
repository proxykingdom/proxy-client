using Proxy.Client.Contracts.Constants;
using Proxy.Client.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    /// <summary>
    /// Extension class that performs socket operations.
    /// </summary>
    internal static class SocketTaskExtensions
    {
        private const int BufferSize = 500;
        private const int PeekBufferSize = 10;

        /// <summary>
        /// Receives the response from the destination server.
        /// </summary>
        /// <param name="socket">Underlying socket.</param>
        /// <returns>The raw response and the time to first byte</returns>
        public static (string response, float firstByteTime) ReceiveAll(this Socket socket)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = TimingHelper.Measure(() => 
            {
                bytesRead = socket.Receive(buffer); 
            });

            if (bytesRead == 0)
                throw new ProxyException("Destination Server has no data to send.");

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                bytesRead = socket.Receive(buffer);

                if (bytesRead == 0)
                    return (placeHolder.ToString(), firstByteTime);

                bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) socket.DecodeContentLength(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) socket.DecodeChunked(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server.");

            return (placeHolder.ToString(), firstByteTime);
        }

        /// <summary>
        /// Asynchronously receives the response from the destination server.
        /// </summary>
        /// <param name="socket">Underlying socket.</param>
        /// <returns>The raw response and the time to first byte</returns>
        public static async Task<(string response, float firstByteTime)> ReceiveAllAsync(this Socket socket)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = await TimingHelper.MeasureAsync(async () => 
            {
                bytesRead = await socket.ReceiveAsync(buffer, buffer.Length);
            });

            if (bytesRead == 0)
                throw new ProxyException("Destination Server has no data to send.");

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                bytesRead = await socket.ReceiveAsync(buffer, buffer.Length);
                
                if (bytesRead == 0) 
                    return (placeHolder.ToString(), firstByteTime);

                bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) await socket.DecodeContentLengthAsync(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) await socket.DecodeChunkedAsync(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server.");

            return (placeHolder.ToString(), firstByteTime);
        }

        /// <summary>
        /// Receives the response from the destination server.
        /// </summary>
        /// <param name="sslStream">Underlying SSL encrypted stream.</param>
        /// <returns>The raw response and the time to first byte</returns>
        public static (string response, float firstByteTime) ReceiveAll(this SslStream sslStream)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = TimingHelper.Measure(() => 
            {
                bytesRead = sslStream.Read(buffer, 0, buffer.Length); 
            });

            if (bytesRead == 0)
                throw new ProxyException("Destination Server has no data to send.");

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                bytesRead = sslStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0) 
                    return (placeHolder.ToString(), firstByteTime);

                bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) sslStream.DecodeContentLength(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) sslStream.DecodeChunked(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server.");

            return (placeHolder.ToString(), firstByteTime);
        }

        /// <summary>
        /// Asynchronously receives the response from the destination server.
        /// </summary>
        /// <param name="sslStream">Underlying SSL encrypted stream.</param>
        /// <returns>The raw response and the time to first byte</returns>
        public static async Task<(string response, float firstByteTime)> ReceiveAllAsync(this SslStream sslStream)
        {
            var buffer = new byte[BufferSize];
            var placeHolder = new StringBuilder();
            var bytesRead = 0;

            var firstByteTime = await TimingHelper.MeasureAsync(async () =>
            {
                bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            });

            if (bytesRead == 0)
                throw new ProxyException("Destination Server has no data to send.");

            var bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            placeHolder.Append(bufferString);

            while(!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0) 
                    return (placeHolder.ToString(), firstByteTime);

                bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                placeHolder.Append(bufferString);
            }

            var readString = placeHolder.ToString();

            if (readString.Contains(RequestConstants.CONTENT_LENGTH_HEADER)) await sslStream.DecodeContentLengthAsync(placeHolder, buffer, bufferString);
            else if (readString.Contains(RequestConstants.TRANSFER_ENCODING_CHUNKED_HEADER)) await sslStream.DecodeChunkedAsync(placeHolder, buffer, bufferString);
            else throw new ProxyException("Unknown Content Encoding provided by Destination Server.");

            return (placeHolder.ToString(), firstByteTime);
        }

        #region APM to Task Methods
        /// <summary>
        /// Asynchronously sends the request to the destination server.
        /// </summary>
        /// <param name="socket">Underlying socket.</param>
        /// <param name="buffer">Buffer used to send the request.</param>
        /// <returns>Number of bytes sent</returns>
        public static Task<int> SendAsync(this Socket socket, byte[] buffer)
        {
            return Task.Factory.FromAsync(
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                socket.EndSend
            );
        }

        /// <summary>
        /// Asynchronously received the response from the destination server.
        /// </summary>
        /// <param name="socket">Underlying socket.</param>
        /// <param name="buffer">Buffer used to receive the response.</param>
        /// <param name="count">Buffer count.</param>
        /// <returns>Number of bytes received</returns>
        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int count)
        {
            return Task.Factory.FromAsync(
                socket.BeginReceive(buffer, 0, count, SocketFlags.None, null, null),
                socket.EndReceive
            );
        }
        #endregion

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
                placeHolder.Append(bufferString);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

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
                placeHolder.Append(bufferString);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

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
                placeHolder.Append(bufferString);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

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
                placeHolder.Append(bufferString);
            }
            else
            {
                bufferString = splitBuffer[1];
            }

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

            int endChunkStringIndex = 0;
            var chunkSizeString = newContentLine.Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = newContentLine.Length - (chunkSizeString.Length + 4);

            while (totalBytesRead > chunkSize)
            {
                endChunkStringIndex = chunkSize + (chunkSizeString.Length + 4);
                foundNewLine = newContentLine.IndexOf(Environment.NewLine, endChunkStringIndex);
                chunkSizeString = newContentLine.Substring(endChunkStringIndex, foundNewLine - endChunkStringIndex);
                chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            }

            var totalChunkBytesRead = totalBytesRead - endChunkStringIndex;

            return (chunkSize, totalChunkBytesRead);
        }
        #endregion
        #endregion
    }
}

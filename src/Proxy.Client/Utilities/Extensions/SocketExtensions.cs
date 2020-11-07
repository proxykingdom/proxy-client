﻿using Proxy.Client.Contracts.Constants;
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

            var firstByteTime = TimingHelper.Measure(() => 
            {
                s.Receive(buffer); 
            });

            var bufferString = Encoding.ASCII.GetString(buffer).Trim('\0');
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = s.Receive(buffer);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead).Trim('\0');
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

            var firstByteTime = await TimingHelper.MeasureAsync(async () => 
            { 
                await s.ReceiveAsync(buffer, s.Available); 
            });

            var bufferString = Encoding.ASCII.GetString(buffer).Trim('\0');
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = await s.ReceiveAsync(buffer, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead).Trim('\0');
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

            var firstByteTime = ss.Read(buffer, 0, buffer.Length);

            var bufferString = Encoding.ASCII.GetString(buffer).Trim('\0');
            placeHolder.Append(bufferString);

            while (!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead).Trim('\0');
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

            var firstByteTime = await TimingHelper.MeasureAsync(async () =>
            {
                await ss.ReadAsync(buffer, 0, buffer.Length);
            });

            var bufferString = Encoding.ASCII.GetString(buffer).Trim('\0'); //use substring instead of trim maybe
            placeHolder.Append(bufferString);

            while(!bufferString.Contains(RequestConstants.CONTENT_SEPERATOR))
            {
                var innerBytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
                bufferString = Encoding.ASCII.GetString(buffer, 0, innerBytesRead).Trim('\0');
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
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);
            var contentLengthString = Regex.Match(placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var contentLength = Convert.ToInt32(contentLengthString, CultureInfo.InvariantCulture);

            var totalBytesRead = splitBuffer[1].Length;

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
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);
            var contentLengthString = Regex.Match(placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var contentLength = Convert.ToInt32(contentLengthString, CultureInfo.InvariantCulture);

            var totalBytesRead = splitBuffer[1].Length;

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
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);
            var contentLengthString = Regex.Match(placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var contentLength = Convert.ToInt32(contentLengthString, CultureInfo.InvariantCulture);

            var totalBytesRead = splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = ss.Read(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }
        }

        private static async Task DecodeContentLengthAsync(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);
            var contentLengthString = Regex.Match(placeHolder.ToString(), RequestConstants.CONTENT_LENGTH_PATTERN).Value;
            var contentLength = Convert.ToInt32(contentLengthString, CultureInfo.InvariantCulture);

            var totalBytesRead = splitBuffer[1].Length;

            while (totalBytesRead < contentLength)
            {
                var innerBytesRead = await ss.ReadAsync(buffer, 0, buffer.Length);
                totalBytesRead += innerBytesRead;
                placeHolder.Append(Encoding.ASCII.GetString(buffer, 0, innerBytesRead));
            }
        }
        #endregion

        #region Chunked Methods
        private static void DecodeChunked(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var foundNewLine = splitBuffer[1].IndexOf("\r\n");
            var chunkSizeString = splitBuffer[1].Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = splitBuffer[1].Length - (foundNewLine + 4);

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

                foundNewLine = bufferString.IndexOf("\r\n");
                chunkSizeString = bufferString.Substring(0, foundNewLine);
                chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
                totalBytesRead = bufferString.Length - (foundNewLine + 4);
            }
        }

        private static async Task DecodeChunkedAsync(this Socket s, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var foundNewLine = splitBuffer[1].IndexOf("\r\n");
            var chunkSizeString = splitBuffer[1].Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = splitBuffer[1].Length - (foundNewLine + 4);

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

                foundNewLine = bufferString.IndexOf("\r\n");
                chunkSizeString = bufferString.Substring(0, foundNewLine);
                chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
                totalBytesRead = bufferString.Length - (foundNewLine + 4);
            }
        }

        private static void DecodeChunked(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var foundNewLine = splitBuffer[1].IndexOf("\r\n");
            var chunkSizeString = splitBuffer[1].Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = splitBuffer[1].Length - (foundNewLine + 4);

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

                foundNewLine = bufferString.IndexOf("\r\n");
                chunkSizeString = bufferString.Substring(0, foundNewLine);
                chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
                totalBytesRead = bufferString.Length - (foundNewLine + 4);
            }
        }

        private static async Task DecodeChunkedAsync(this SslStream ss, StringBuilder placeHolder, byte[] buffer, string bufferString)
        {
            var splitBuffer = bufferString.Split(new[] { RequestConstants.CONTENT_SEPERATOR }, 2, StringSplitOptions.None);

            var foundNewLine = splitBuffer[1].IndexOf("\r\n");
            var chunkSizeString = splitBuffer[1].Substring(0, foundNewLine);
            var chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
            var totalBytesRead = splitBuffer[1].Length - (foundNewLine + 4);

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

                foundNewLine = bufferString.IndexOf("\r\n");
                chunkSizeString = bufferString.Substring(0, foundNewLine);
                chunkSize = int.Parse(chunkSizeString, NumberStyles.HexNumber);
                totalBytesRead = bufferString.Length - (foundNewLine + 4);
            }
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

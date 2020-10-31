using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class SocketTaskExtensions
    {
        private const int StringBufferSize = 2048;

        public static string ReceiveAll(this Socket s, SocketFlags flags)
        {
            var buffer = new byte[StringBufferSize];
            var placeHolder = new StringBuilder();

            do
            {
                s.Receive(buffer, flags);
                placeHolder.Append(Encoding.UTF8.GetString(buffer));
            } while (s.Available != 0);

            return placeHolder.ToString().Trim('\0');
        }

        public static async Task<string> ReceiveAllAsync(this Socket s, SocketFlags flags)
        {
            var buffer = new byte[StringBufferSize];
            var placeHolder = new StringBuilder();

            do
            {
                await s.ReceiveAsync(buffer, flags);
                placeHolder.Append(Encoding.UTF8.GetString(buffer));
            } while (s.Available != 0);

            return placeHolder.ToString().Trim('\0');
        }

        public static string ReadString(this SslStream ss, Socket s)
        {
            var buffer = new byte[StringBufferSize];
            var placeHolder = new StringBuilder();

            do
            {
                ss.Read(buffer, 0, buffer.Length);
                placeHolder.Append(Encoding.UTF8.GetString(buffer));
            } while (s.Available != 0);

            return placeHolder.ToString().Trim('\0');
        }

        public static async Task<string> ReadStringAsync(this SslStream ss, Socket s)
        {
            var buffer = new byte[StringBufferSize];
            var placeHolder = new StringBuilder();

            do
            {
                await ss.ReadAsync(buffer, 0, buffer.Length);
                placeHolder.Append(Encoding.UTF8.GetString(buffer));
            } while (s.Available != 0);

            return placeHolder.ToString().Trim('\0');
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

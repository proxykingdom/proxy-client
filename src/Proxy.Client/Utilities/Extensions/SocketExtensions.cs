using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class SocketTaskExtensions
    {
        public static string ReceiveAll(this Socket s, SocketFlags flags)
        {
            var buffer = new byte[2048];
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
            var buffer = new byte[2048];
            var placeHolder = new StringBuilder();

            do
            {
                await s.ReceiveAsync(buffer, SocketFlags.None);
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

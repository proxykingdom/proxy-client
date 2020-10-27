using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class SocketTaskExtensions
    {
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

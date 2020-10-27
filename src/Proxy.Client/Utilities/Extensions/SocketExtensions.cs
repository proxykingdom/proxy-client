using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.Client.Utilities.Extensions
{
    public static class SocketTaskExtensions
    {
        public static byte[] ReceiveAll(this Socket s, byte[] buffer, SocketFlags flags)
        {
            var bufferSize = buffer.Length;
            var bytesReceivedCount = 0;

            do
            {
                //create a new array with the same contents with added new bytecount array
                bytesReceivedCount = s.Receive(buffer, flags);
                
                var newSize = 

            } while (s.Available != 0);
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

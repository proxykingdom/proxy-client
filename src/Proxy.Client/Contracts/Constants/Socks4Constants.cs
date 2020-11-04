namespace Proxy.Client.Contracts.Constants
{
    internal class Socks4Constants
    {
        internal const byte SOCKS4_VERSION_NUMBER = 4;
        internal const byte SOCKS4_CMD_CONNECT = 0x01;
        internal const byte SOCKS4_CMD_REPLY_REQUEST_GRANTED = 90;
        internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_OR_FAILED = 91;
        internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD = 92;
        internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD = 93;
    }
}

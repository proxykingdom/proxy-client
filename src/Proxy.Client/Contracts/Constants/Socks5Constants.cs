namespace Proxy.Client.Contracts
{
    internal class Socks5Constants
    {
        internal const byte SOCKS5_VERSION_NUMBER = 5;
        internal const byte SOCKS5_AUTH_NUMBER_OF_AUTH_METHODS_SUPPORTED = 2;
        internal const byte SOCKS5_AUTH_METHOD_NO_AUTHENTICATION_REQUIRED = 0x00;
        internal const byte SOCKS5_AUTH_METHOD_USERNAME_PASSWORD = 0x02;
        internal const byte SOCKS5_AUTH_METHOD_REPLY_NO_ACCEPTABLE_METHODS = 0xff;
        internal const byte SOCKS5_CMD_CONNECT = 0x01;
        internal const byte SOCKS5_CMD_REPLY_SUCCEEDED = 0x00;
        internal const byte SOCKS5_CMD_REPLY_GENERAL_SOCKS_SERVER_FAILURE = 0x01;
        internal const byte SOCKS5_CMD_REPLY_CONNECTION_NOT_ALLOWED_BY_RULESET = 0x02;
        internal const byte SOCKS5_CMD_REPLY_NETWORK_UNREACHABLE = 0x03;
        internal const byte SOCKS5_CMD_REPLY_HOST_UNREACHABLE = 0x04;
        internal const byte SOCKS5_CMD_REPLY_CONNECTION_REFUSED = 0x05;
        internal const byte SOCKS5_CMD_REPLY_TTL_EXPIRED = 0x06;
        internal const byte SOCKS5_CMD_REPLY_COMMAND_NOT_SUPPORTED = 0x07;
        internal const byte SOCKS5_CMD_REPLY_ADDRESS_TYPE_NOT_SUPPORTED = 0x08;
        internal const byte SOCKS5_ADDRTYPE_DOMAIN_NAME = 0x03;
        internal const byte SOCKS5_ADDRTYPE_IPV4 = 0x01;
        internal const byte SOCKS5_ADDRTYPE_IPV6 = 0x04;
        internal const byte SOCKS5_RESERVED = 0x00;
    }
}

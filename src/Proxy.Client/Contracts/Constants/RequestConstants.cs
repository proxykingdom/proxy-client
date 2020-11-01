namespace Proxy.Client.Contracts.Constants
{
    internal class RequestConstants
    {
        internal const string CONTENT_SEPERATOR = "\r\n\r\n";
        internal const string CONTENT_LENGTH_PATTERN = "(?<=Content-Length: ).\\d+";
        internal const string NO_SSL = "http";
        internal const string SSL = "https";
    }
}

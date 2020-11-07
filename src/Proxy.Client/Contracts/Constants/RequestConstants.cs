namespace Proxy.Client.Contracts.Constants
{
    internal class RequestConstants
    {
        internal const string CONTENT_SEPERATOR = "\r\n\r\n";
        internal const string CONTENT_LENGTH_HEADER = "Content-Length";
        internal const string TRANSFER_ENCODING_CHUNKED_HEADER = "chunked";
        internal const string CONTENT_LENGTH_PATTERN = "(?<=Content-Length: )[0-9]*";
        internal const string STATUS_CODE_PATTERN = "\\d\\d\\d";
        internal const string NO_SSL = "http";
        internal const string SSL = "https";
    }
}

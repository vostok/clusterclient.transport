using System;
using System.Collections.Specialized;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal class ReceivedRequest
    {
        public string Method { get; set; }
        public Uri Url { get; set; }
        public byte[] Body { get; set; }
        public long BodySize { get; set; }
        public NameValueCollection Headers { get; set; }
        public NameValueCollection Query { get; set; }
        public UserIdentity UserIdentity { get; set; }
    }
}
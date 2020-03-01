using System;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions
{
    internal class UserStreamException : Exception
    {
        public UserStreamException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

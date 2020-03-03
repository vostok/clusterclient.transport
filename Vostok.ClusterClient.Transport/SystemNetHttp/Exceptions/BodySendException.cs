using System;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions
{
    internal class BodySendException : Exception
    {
        public BodySendException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

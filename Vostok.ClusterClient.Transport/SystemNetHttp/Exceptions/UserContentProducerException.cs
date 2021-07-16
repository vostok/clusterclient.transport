using System;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions
{
    internal class UserContentProducerException : Exception
    {
        public UserContentProducerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
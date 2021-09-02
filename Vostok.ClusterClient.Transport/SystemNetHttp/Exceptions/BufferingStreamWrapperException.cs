using System;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions
{
    internal class BufferingStreamWrapperException : Exception
    {
        public BufferingStreamWrapperException(Exception innerException)
            : base(null, innerException)
        {
        }
    }
}
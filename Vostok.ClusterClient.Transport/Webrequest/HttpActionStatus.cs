namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal enum HttpActionStatus
    {
        Success,
        ConnectionFailure,
        SendFailure,
        ReceiveFailure,
        Timeout,
        RequestCanceled,
        ProtocolError,
        UnknownFailure,
        InsufficientStorage,
        UserStreamFailure
    }
}

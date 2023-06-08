using System;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using Vostok.Logging.Abstractions;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Transport.Sockets
{
    /// <summary>
    /// This class was created to set the TcpKeepalive options directly on the Socket in the HttpMessageHandler.
    /// But due to significant changes in System.Net.Http.HttpConnection class it can't work starting from .Net7.
    /// There is no _socket field. But starting from .Net5 HttpMessageHandler has a special api for setting TcpKeepAlive options.
    /// </summary>
    internal static class SocketAccessor
    {
        private static readonly Func<Stream, Socket> Empty = _ => null;
        private static readonly object Sync = new object();

        private static volatile Func<Stream, Socket> accessor;

        public static Socket GetSocket(Stream stream, ILog log)
        {
            if (stream == null)
                return null;

            EnsureInitialized(log);

            try
            {
                return accessor(stream);
            }
            catch (Exception error)
            {
                if (accessor != Empty)
                    log.Error(error, "Failed to obtain Socket instance from request stream of type {StreamType}.", stream.GetType().Name);

                accessor = Empty;
                return null;
            }
        }

        private static void EnsureInitialized(ILog log)
        {
            if (accessor == null)
            {
                lock (Sync)
                {
                    if (accessor == null)
                        accessor = Build(log);
                }
            }
        }

        private static Func<Stream, Socket> Build(ILog log)
        {
            try
            {
                var parameterExpr = Expression.Parameter(typeof(Stream));
                var httpContentStreamType = typeof(HttpClient).Assembly.GetType("System.Net.Http.HttpContentStream");
                var httpContentStreamExpr = Expression.Convert(parameterExpr, httpContentStreamType);
                var connectionField = httpContentStreamType.GetField("_connection", BindingFlags.Instance | BindingFlags.NonPublic);
                var connectionFieldExpr = Expression.Field(httpContentStreamExpr, connectionField);
                var socketField = connectionField.FieldType.GetField("_socket", BindingFlags.Instance | BindingFlags.NonPublic);
                var socketFieldExpr = Expression.Field(connectionFieldExpr, socketField);
                var nullExpr = Expression.Constant(null, connectionField.FieldType);

                var condition = Expression.Condition(
                    Expression.Equal(connectionFieldExpr, nullExpr),
                    Expression.Constant(null, socketField.FieldType),
                    socketFieldExpr
                );
                return Expression.Lambda<Func<Stream, Socket>>(condition, parameterExpr).Compile();
            }
            catch (Exception error)
            {
                log.ForContext(typeof(SocketAccessor)).Error(error, "Failed to build Socket accessor delegate.");
                return Empty;
            }
        }
    }
}

using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Transport.Helpers
{
    internal static class NetCore50Utils
    {
        private const string Namespace = "Vostok.Clusterclient.Transport.Core50";
        private const string Library = "Vostok.ClusterClient.Transport.Core50.dll";

        private static readonly Action<HttpMessageHandler, bool, TimeSpan, TimeSpan, bool> tuning;

        static NetCore50Utils()
        {
            var assembly = ResourceAssemblyLoader.Load(Library);

            var handlerTunerType = assembly.GetType($"{Namespace}.SocketsHttpHandlerTuner");

            var handlerTunerMethod = handlerTunerType.GetMethod("Tune", BindingFlags.Public | BindingFlags.Static);

            var handlerParameter = Expression.Parameter(typeof(HttpMessageHandler));
            var tcpKeepAliveEnablesParameter = Expression.Parameter(typeof(bool));
            var tcpKeepAliveIntervalParameter = Expression.Parameter(typeof(TimeSpan));
            var tcpKeepAliveTimeParameter = Expression.Parameter(typeof(TimeSpan));
            var enableMultipleHttp2ConnectionsParameter = Expression.Parameter(typeof(bool));

            tuning = Expression.Lambda<Action<HttpMessageHandler, bool, TimeSpan, TimeSpan, bool>>(
                    Expression.Call(handlerTunerMethod, handlerParameter, tcpKeepAliveEnablesParameter, tcpKeepAliveIntervalParameter, tcpKeepAliveTimeParameter, enableMultipleHttp2ConnectionsParameter),
                    handlerParameter,
                    tcpKeepAliveEnablesParameter,
                    tcpKeepAliveIntervalParameter,
                    tcpKeepAliveTimeParameter,
                    enableMultipleHttp2ConnectionsParameter)
                .Compile();
        }

        public static void TuneHandler(HttpMessageHandler handler, bool tcpKeepAliveEnables, TimeSpan tcpKeepAliveInterval, TimeSpan tcpKeepAliveTime, bool enableMultipleHttp2Connections)
        {
            tuning(handler, tcpKeepAliveEnables, tcpKeepAliveInterval, tcpKeepAliveTime, enableMultipleHttp2Connections);
        }
    }
}

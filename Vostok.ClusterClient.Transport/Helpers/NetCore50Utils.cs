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

        private static readonly Action<HttpMessageHandler> tuning;

        static NetCore50Utils()
        {
            var assembly = ResourceAssemblyLoader.Load(Library);

            var handlerTunerType = assembly.GetType($"{Namespace}.SocketsHttpHandlerTuner");

            var handlerTunerMethod = handlerTunerType.GetMethod("Tune", BindingFlags.Public | BindingFlags.Static);

            var handlerParameter = Expression.Parameter(typeof(HttpMessageHandler));

            tuning = Expression.Lambda<Action<HttpMessageHandler>>(Expression.Call(handlerTunerMethod, handlerParameter), handlerParameter)
                .Compile();
        }

        public static void TuneHandler(HttpMessageHandler handler)
            => tuning(handler);
    }
}

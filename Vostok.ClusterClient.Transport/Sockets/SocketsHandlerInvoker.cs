using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Transport.Helpers;
using Invoker = System.Func<System.Net.Http.HttpMessageHandler, System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken, System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>>;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Transport.Sockets
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SocketsHandlerInvoker : HttpMessageHandler
    {
        private static readonly Invoker invoker;

        static SocketsHandlerInvoker()
        {
            try
            {
                invoker = BuildInvoker();

                CanInvokeDirectly = true;
            }
            catch (Exception error)
            {
                Console.Out.WriteLine(error);

                invoker = (handler, message, token) => new HttpClient(handler).SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
            }
        }

        private SocketsHandlerInvoker()
        {
        }

        public static bool CanInvokeDirectly { get; }

        public static Task<HttpResponseMessage> Invoke(HttpMessageHandler handler, HttpRequestMessage message, CancellationToken token)
            => invoker(handler, message, token);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken token)
            => throw new NotSupportedException();

        private static Invoker BuildInvoker()
        {
            var handlerParameter = Expression.Parameter(typeof(HttpMessageHandler));
            var socketHandler = Expression.Convert(handlerParameter, NetCore21Utils.SocketHandlerType);
            var messageParameter = Expression.Parameter(typeof(HttpRequestMessage));
            var tokenParameter = Expression.Parameter(typeof(CancellationToken));

            var sendMethod = NetCore21Utils.SocketHandlerType.GetMethod(nameof(SendAsync), BindingFlags.Instance | BindingFlags.NonPublic);
            var sendMethodCall = Expression.Call(socketHandler, sendMethod, messageParameter, tokenParameter);

            return Expression.Lambda<Invoker>(sendMethodCall, handlerParameter, messageParameter, tokenParameter).Compile();
        }
    }
}

using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using HandlerFactoryFunc = System.Func<System.Net.IWebProxy, bool, System.TimeSpan, System.TimeSpan, System.TimeSpan, int, System.Security.Cryptography.X509Certificates.X509Certificate2[], System.Net.Security.RemoteCertificateValidationCallback, System.Net.Http.HttpMessageHandler>;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Transport.Helpers
{
    internal static class NetCore21Utils
    {
        private const string Namespace = "Vostok.Clusterclient.Transport.Core21";
        private const string Library = "Vostok.ClusterClient.Transport.Core21.dll";
        private static readonly HandlerFactoryFunc socketHandlerFactory;

        static NetCore21Utils()
        {
            var assembly = ResourceAssemblyLoader.Load(Library);

            var handlerFactoryType = assembly.GetType($"{Namespace}.SocketsHttpHandlerFactory");

            var handlerFactoryMethod = handlerFactoryType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

            var proxyParameter = Expression.Parameter(typeof(IWebProxy));
            var redirectParameter = Expression.Parameter(typeof(bool));
            var timeoutParameter = Expression.Parameter(typeof(TimeSpan));
            var idleTimeoutParameter = Expression.Parameter(typeof(TimeSpan));
            var lifetimeParameter = Expression.Parameter(typeof(TimeSpan));
            var maxConnectionsParameter = Expression.Parameter(typeof(int));
            var certificatesParameter = Expression.Parameter(typeof(X509Certificate2[]));
            var remoteCertificateValidationCallback = Expression.Parameter(typeof(RemoteCertificateValidationCallback));

            socketHandlerFactory = Expression.Lambda<HandlerFactoryFunc>(
                    Expression.Call(
                        handlerFactoryMethod,
                        proxyParameter,
                        redirectParameter,
                        timeoutParameter,
                        idleTimeoutParameter,
                        lifetimeParameter,
                        maxConnectionsParameter,
                        certificatesParameter,
                        remoteCertificateValidationCallback),
                    proxyParameter,
                    redirectParameter,
                    timeoutParameter,
                    idleTimeoutParameter,
                    lifetimeParameter,
                    maxConnectionsParameter,
                    certificatesParameter,
                    remoteCertificateValidationCallback)
                .Compile();

            SocketHandlerType = (Type)handlerFactoryType.GetProperty("SocketHandlerType")?.GetValue(null);
        }

        public static Type SocketHandlerType { get; }

        public static HttpMessageHandler CreateSocketsHandler(
            IWebProxy proxy,
            bool allowAutoRedirect,
            TimeSpan connectionTimeout,
            TimeSpan connectionIdleTimeout,
            TimeSpan connectionLifetime,
            int maxConnectionsPerEndpoint,
            X509Certificate2[] clientCertificates,
            RemoteCertificateValidationCallback remoteCertificateValidationCallback)
            => socketHandlerFactory(
                proxy,
                allowAutoRedirect,
                connectionTimeout,
                connectionIdleTimeout,
                connectionLifetime,
                maxConnectionsPerEndpoint,
                clientCertificates,
                remoteCertificateValidationCallback);
    }
}
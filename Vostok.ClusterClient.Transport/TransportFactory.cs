using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Commons.Environment;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    internal static class TransportFactory
    {
        private static readonly object Sync = new object();
        private static volatile Func<UniversalTransportSettings, ILog, ITransport> createTransport;

        public static ITransport Create(UniversalTransportSettings settings, ILog log)
        {
            if (createTransport == null)
            {
                lock (Sync)
                {
                    if (createTransport == null)
                        createTransport = BuildFactory(log);
                }
            }

            return createTransport(settings, log);
        }

        private static Func<UniversalTransportSettings, ILog, ITransport> BuildFactory(ILog log)
        {
            var assembly = GetTransportAssembly(log);
            var type = assembly.GetType("Vostok.Clusterclient.Transport.Adapter.TransportFactory");
            var method = type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);

            var settingsParameter = Expression.Parameter(typeof(object));
            var logParameter = Expression.Parameter(typeof(ILog));

            return Expression.Lambda<Func<object, ILog, ITransport>>(
                    Expression.Call(method, settingsParameter, logParameter),
                    settingsParameter,
                    logParameter)
                .Compile();
        }

        private static Assembly GetTransportAssembly(ILog log)
        {
            if (RuntimeDetector.IsDotNetCore21AndNewer)
                return LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Sockets.Merged.dll");

            if (RuntimeDetector.IsDotNetCore20)
                return LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Native.Merged.dll");

            if (RuntimeDetector.IsDotNetFramework)
                return LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Webrequest.Merged.dll");

            log.Warn("Unknown .NET runtime. Will fall back to HttpWebRequest-based transport.");

            return LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Webrequest.Merged.dll");
        }

        private static Assembly LoadAssemblyFromResource(string libName)
        {
            const string nameSpace = "Vostok.Clusterclient.Transport";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{nameSpace}.{libName}";

            using (var input = assembly.GetManifestResourceStream(resourceName))
            {
                if (input == null)
                    throw new InvalidOperationException($"Resource with name '{resourceName}' was not found.");

                var buffer = new MemoryStream((int)input.Length);

                input.CopyTo(buffer);

                return Assembly.Load(buffer.ToArray());
            }
        }
    }
}

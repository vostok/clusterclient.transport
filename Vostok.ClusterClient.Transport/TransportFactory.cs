using System;
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
            try
            {
                var assembly = GetTransportAssembly(log);

                var type = assembly.GetType("Vostok.Clusterclient.Transport.Adapter.TransportFactory");
                var method = type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);

                var settingsParam = Expression.Parameter(typeof(object));
                var logParam = Expression.Parameter(typeof(ILog));

                return Expression.Lambda<Func<object, ILog, ITransport>>(
                        Expression.Call(method, settingsParam, logParam),
                        settingsParam,
                        logParam)
                    .Compile();
            }
            catch (Exception e)
            {
                log.Error(e, $"An exception has occured during {nameof(UniversalTransport)} initialization.");
                throw;
            }
        }

        private static Assembly GetTransportAssembly(ILog log)
        {
            Assembly assembly;
            if (RuntimeDetector.IsDotNetCore21AndNewer)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Sockets.Merged.dll");
            else if (RuntimeDetector.IsDotNetCore20)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Native.Merged.dll");
            else if (RuntimeDetector.IsDotNetFramework)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Webrequest.Merged.dll");
            else
            {
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Webrequest.Merged.dll");
                log.ForContext<UniversalTransport>().Debug("Unknown .NET runtime. Use WebRequest-based transport as fallback.");
            }

            return assembly;
        }

        private static Assembly LoadAssemblyFromResource(string libName)
        {
            const string nameSpace = "Vostok.Clusterclient.Transport";

            var assembly = Assembly.GetExecutingAssembly();
            var resName = $"{nameSpace}.{libName}";

            using (var input = assembly.GetManifestResourceStream(resName))
            {
                var size = input.Length;
                var bytes = new byte[size];
                input.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }
    }
}
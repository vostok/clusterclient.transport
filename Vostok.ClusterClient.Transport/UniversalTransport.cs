using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    public class UniversalTransport : ITransport
    {
        private readonly ITransport implementation;

        public UniversalTransport(UniversalTransportSettings settings, ILog log)
        {
            Assembly assembly;
            if (RuntimeDetector.IsDotNetFramework)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.WebRequest.Merged.dll");
            else if (RuntimeDetector.IsDotNetCore21AndNewer)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.Sockets.Merged.dll");
            else
                throw new NotSupportedException("Runtime is not supported");
            var type = assembly.GetType("Vostok.Clusterclient.Transport.Adapter.TransportFactory");
            var method = type.GetMethod("Create", BindingFlags.Static|BindingFlags.Public);
            implementation = (ITransport) method.Invoke(null, new object[]{settings ?? new UniversalTransportSettings(), log});
        }

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
            => implementation.SendAsync(request, connectionTimeout, timeout, cancellationToken);

        /// <inheritdoc />
        public TransportCapabilities Capabilities => implementation.Capabilities;
        
        private Assembly LoadAssemblyFromResource(string libName)
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
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Transport
{
    public class UniversalTransport : ITransport
    {
        private readonly ITransport implementation;

        public UniversalTransport(UniversalTransportSettings settings, ILog log)
        {
            Assembly assembly;
            if (RuntimeDetector.IsDotNetFramework)
                assembly = LoadAssemblyFromResource("Vostok.ClusterClient.Transport.Adapter.WebRequest.Merged.dll");
            else
                throw new NotSupportedException("Runtime is not supported");
            var type = assembly.GetType("Vostok.ClusterClient.Transport.Adapter.WebRequest.TransportFactory");
            var method = type.GetMethod("Create", BindingFlags.Static|BindingFlags.Public);
            implementation = (ITransport) method.Invoke(null, new Object[]{settings ?? new UniversalTransportSettings(), log});
        }

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan timeout, CancellationToken cancellationToken)
            => implementation.SendAsync(request, timeout, cancellationToken);

        /// <inheritdoc />
        public TransportCapabilities Capabilities => implementation.Capabilities;
        
        private Assembly LoadAssemblyFromResource(string libName)
        {
            const string nameSpace = "Vostok.ClusterClient.Transport";

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
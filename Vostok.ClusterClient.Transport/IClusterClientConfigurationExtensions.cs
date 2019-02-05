using JetBrains.Annotations;
using Vostok.Clusterclient.Core;

namespace Vostok.Clusterclient.Transport
{
    [PublicAPI]
    public static class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Initialiazes configuration transport with a <see cref="UniversalTransport"/> with given settings.
        /// </summary>
        public static void SetupUniversalTransport(this IClusterClientConfiguration self, UniversalTransportSettings settings)
        {
            self.Transport = new UniversalTransport(settings, self.Log);
        }

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="UniversalTransport"/> with default settings.
        /// </summary>
        public static void SetupUniversalTransport(this IClusterClientConfiguration self)
        {
            self.Transport = new UniversalTransport(self.Log);
        }
    }
}
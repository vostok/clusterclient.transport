using System;
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
            => self.Transport = new UniversalTransport(settings, self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="UniversalTransport"/> with default settings.
        /// </summary>
        public static void SetupUniversalTransport(this IClusterClientConfiguration self)
            => self.Transport = new UniversalTransport(self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="SocketsTransport"/> with given settings.
        /// </summary>
        public static void SetupSocketTransport(this IClusterClientConfiguration self, SocketsTransportSettings settings)
            => self.Transport = new SocketsTransport(settings, self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="SocketsTransport"/> with default settings.
        /// </summary>
        public static void SetupSocketTransport(this IClusterClientConfiguration self)
            => self.Transport = new SocketsTransport(self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="WebRequestTransport"/> with given settings.
        /// </summary>
        public static void SetupWebRequestTransport(this IClusterClientConfiguration self, WebRequestTransportSettings settings)
            => self.Transport = new WebRequestTransport(settings, self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="WebRequestTransport"/> with default settings.
        /// </summary>
        public static void SetupWebRequestTransport(this IClusterClientConfiguration self)
            => self.Transport = new WebRequestTransport(self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="NativeTransport"/> with given settings.
        /// </summary>
        [Obsolete("Don't use this ITransport implementation on .NET Core 2.1 or later. Use SocketsTransport instead.")]
        public static void SetupNativeTransport(this IClusterClientConfiguration self, NativeTransportSettings settings)
            => self.Transport = new NativeTransport(settings, self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="NativeTransport"/> with default settings.
        /// </summary>
        [Obsolete("Don't use this ITransport implementation on .NET Core 2.1 or later. Use SocketsTransport instead.")]
        public static void SetupNativeTransport(this IClusterClientConfiguration self)
            => self.Transport = new NativeTransport(self.Log);
    }
}
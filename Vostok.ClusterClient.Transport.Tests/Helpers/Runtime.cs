using System;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    [Flags]
    internal enum Runtime
    {
        Mono = 1,
        Framework = 1 << 1,
        Core20 = 1 << 2,
        Core21 = 1 << 3,
        Core31 = 1 << 4
    }
}

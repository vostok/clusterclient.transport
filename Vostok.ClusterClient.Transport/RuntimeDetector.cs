using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vostok.ClusterClient.Transport
{
    public static class RuntimeDetector
    {
        private static readonly string Dir = RuntimeEnvironment.GetRuntimeDirectory();

        public static bool IsMono { get; } = Type.GetType("Mono.Runtime") != null;
        public static bool IsDotNetCore { get; } = Dir.Contains("NETCore");
        public static bool IsDotNetCore20 { get; } = IsDotNetCore && Dir.Contains($"{Path.DirectorySeparatorChar}2.0.");
        public static bool IsDotNetCore21AndNewer { get; } = IsDotNetCore && Dir.Contains($"{Path.DirectorySeparatorChar}2.") && !IsDotNetCore20;
        public static bool IsDotNetFramework { get; } = Dir.Contains($"Microsoft.NET{Path.DirectorySeparatorChar}Framework");
    }
}
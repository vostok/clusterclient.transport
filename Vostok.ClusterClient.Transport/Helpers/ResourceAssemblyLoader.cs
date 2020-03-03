using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vostok.Clusterclient.Transport.Helpers
{
    internal static class ResourceAssemblyLoader
    {
        private const string Namespace = "Vostok.Clusterclient.Transport";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Assembly Load(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{Namespace}.{name}";

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

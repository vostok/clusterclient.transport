using System;
using System.Reflection;

namespace Vostok.Clusterclient.Transport.Helpers
{
    internal static class NetCore20Utils
    {
        private const string Namespace = "Vostok.Clusterclient.Transport.Core20";
        private const string Library = "Vostok.ClusterClient.Transport.Core20.dll";

        private static readonly Type methodGeneratorType;

        static NetCore20Utils()
        {
            var assembly = ResourceAssemblyLoader.Load(Library);
            
            methodGeneratorType = assembly.GetType($"{Namespace}.MethodGenerator");
        }

        public static Action<object> CreateAssignment(FieldInfo field, int value)
            => methodGeneratorType
                .GetMethod(nameof(CreateAssignment), BindingFlags.Static | BindingFlags.Public)?
                .Invoke(null, new object[] { field, value }) as Action<object>;

        public static Action<object> CreateNullAssignment(FieldInfo field)
            => methodGeneratorType
                .GetMethod(nameof(CreateNullAssignment), BindingFlags.Static | BindingFlags.Public)?
                .Invoke(null, new object[] { field }) as Action<object>;
    }
}

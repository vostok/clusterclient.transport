using System.Reflection;

namespace Vostok.ClusterClient.Transport
{
    internal static class Translator
    {
        public static TTarget Translate<TTarget>(object o) where TTarget : new()
        {
            var sourceType = o.GetType();
            var targetType = typeof(TTarget);
            
            var properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            var instance = new TTarget();
            
            foreach (var property in properties)
            {
                var sourceProperty = sourceType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public);
                if (sourceProperty == null)
                    continue;
                property.SetValue(instance, sourceProperty.GetValue(o));
            }

            return instance;
        }
    }
}
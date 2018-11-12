using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Vostok.Clusterclient.Transport
{
    internal static class Translator
    {
        public static TTarget Translate<TTarget>(object o)
            where TTarget : new()
        {
            return TranslatorInternal<TTarget>.GetTranslator(o.GetType())(o);
        }

        private static class TranslatorInternal<TTarget>
            where TTarget : new()
        {
            private static readonly ConcurrentDictionary<Type, Lazy<Func<object, TTarget>>> Cache
                = new ConcurrentDictionary<Type, Lazy<Func<object, TTarget>>>();

            public static Func<object, TTarget> GetTranslator(Type type)
            {
                return Cache.GetOrAdd(
                    type,
                    t => new Lazy<Func<object, TTarget>>(() => CreateTranslator(t))).Value;
            }

            private static Func<object, TTarget> CreateTranslator(Type sourceType)
            {
                var targetType = typeof(TTarget);
                
                var properties = targetType.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                var objectParameter = Expression.Parameter(typeof(object));
                var parameter = Expression.Convert(objectParameter, sourceType);
                
                var bindings = new List<MemberBinding>();

                foreach (var property in properties)
                {
                    var sourceProperty = sourceType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public);
                    if (sourceProperty == null)
                        continue;
                    var bindExpression = Expression.Bind(property, Expression.MakeMemberAccess(parameter, sourceProperty));
                    bindings.Add(bindExpression);
                }

                var memberInit = Expression.MemberInit(Expression.New(targetType), bindings);
                
                return Expression.Lambda<Func<object, TTarget>>(memberInit, objectParameter).Compile();
            }

        }
    }
}
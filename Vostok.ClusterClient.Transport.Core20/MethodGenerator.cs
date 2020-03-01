using System;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport.Core20
{
    [UsedImplicitly]
    public static class MethodGenerator
    {
        [UsedImplicitly]
        public static Action<object> CreateAssignment(FieldInfo field, int value)
        {
            var dyn = new DynamicMethod($"Assign_{field}_{value}", null, new[] { typeof(object) }, typeof(MethodGenerator));

            var il = dyn.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, value);
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            return (Action<object>) dyn.CreateDelegate(typeof(Action<object>));
        }

        [UsedImplicitly]
        public static Action<object> CreateNullAssignment(FieldInfo field)
        {
            var dyn = new DynamicMethod($"Assign_null_to_{field}", null, new[] { typeof(object) }, typeof(MethodGenerator));

            var il = dyn.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            return (Action<object>) dyn.CreateDelegate(typeof(Action<object>));
        }
    }
}

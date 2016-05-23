namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class ImplementSpecialConstructorStep : ModuleStep
    {
        private void ImplementConstructor(TypeWrapper typeWrapper)
        {
            TypeDefinition definition;
            if (typeWrapper.Type.IsDefinition)
            {
                definition = (TypeDefinition) typeWrapper.Type;
            }
            else
            {
                if (!typeWrapper.Type.IsGenericInstance)
                {
                    throw new NotImplementedException(string.Format("Can't implement special constructor for type {0}.", typeWrapper.Type.FullName));
                }
                definition = ((GenericInstanceType) typeWrapper.Type).ElementType.Resolve();
            }
            MethodDefinition definition2 = definition.Methods.FirstOrDefault<MethodDefinition>(<>c.<>9__2_0 ?? (<>c.<>9__2_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ImplementConstructor>b__2_0)));
            if (definition2 != null)
            {
                typeWrapper.SpecialConstructor = definition2;
            }
            else
            {
                definition.Methods.Add(typeWrapper.SpecialConstructor);
                ILProcessor iLProcessor = typeWrapper.SpecialConstructor.Body.GetILProcessor();
                iLProcessor.Emit(OpCodes.Ldarg_0);
                MethodReference defaultConstructor = typeWrapper.BaseType.DefaultConstructor;
                if (typeWrapper.BaseType.SpecialConstructor != null)
                {
                    iLProcessor.Emit(OpCodes.Ldarg_1);
                    if (typeWrapper.BaseType.Type.IsGenericInstance)
                    {
                        GenericInstanceType type = (GenericInstanceType) typeWrapper.BaseType.Type;
                        defaultConstructor = typeWrapper.BaseType.SpecialConstructor.MakeGenericMethod(type.GenericArguments.ToArray());
                    }
                    else
                    {
                        defaultConstructor = typeWrapper.BaseType.SpecialConstructor;
                    }
                }
                iLProcessor.Emit(OpCodes.Call, base.OperationContext.Import(typeWrapper.SpecialConstructor.Module, defaultConstructor));
                iLProcessor.Emit(OpCodes.Ret);
            }
        }

        private bool Process(TypeWrapper typeWrapper)
        {
            if (!typeWrapper.Type.IsValueType)
            {
                if (typeWrapper.SpecialConstructor == null)
                {
                    return false;
                }
                if (typeWrapper.BaseType.SpecialConstructor == null)
                {
                    if (!typeWrapper.BaseType.Assembly.System || (typeWrapper.BaseType.DefaultConstructor == null))
                    {
                        return false;
                    }
                }
                else if (!this.Process(typeWrapper.BaseType))
                {
                    return false;
                }
                this.ImplementConstructor(typeWrapper);
            }
            return true;
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                foreach (TypeWrapper wrapper2 in assemblyWrapper.Types)
                {
                    if (!this.Process(wrapper2))
                    {
                        wrapper2.SpecialConstructor = null;
                        wrapper2.ImplementCreateInstanceMethod = false;
                    }
                    else if (wrapper2.Type.IsGenericInstance)
                    {
                        wrapper2.ImplementCreateInstanceMethod = false;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ImplementSpecialConstructorStep.<>c <>9 = new ImplementSpecialConstructorStep.<>c();
            public static Func<MethodDefinition, bool> <>9__2_0;

            internal bool <ImplementConstructor>b__2_0(MethodDefinition m)
            {
                if (!m.IsConstructor || m.IsStatic)
                {
                    return false;
                }
                if (m.Parameters.Count != 1)
                {
                    return false;
                }
                return (m.Parameters[0].ParameterType.FullName == typeof(UIntPtr).FullName);
            }
        }
    }
}


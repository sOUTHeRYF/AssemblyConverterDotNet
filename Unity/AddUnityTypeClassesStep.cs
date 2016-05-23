namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AddUnityTypeClassesStep : ModuleStep
    {
        private MethodReference getTypeFromHandleMethod;
        private TypeReference objectType;
        private TypeReference typeType;
        private TypeReference unityTypeType;
        private TypeDefinition unityTypeTypeDefinition;

        private MethodDefinition AddConstructor(TypeWrapper typeWrapper, TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition(".ctor", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, base.ModuleContext.GetCorLibType("System.Void"));
            type.Methods.Add(item);
            return item;
        }

        private MethodDefinition AddCreateInstanceMethod(TypeWrapper typeWrapper, TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("CreateInstance", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, this.objectType);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = body.GetILProcessor();
            if (typeWrapper.Type.IsValueType)
            {
                body.InitLocals = true;
                VariableDefinition definition2 = new VariableDefinition("instance", typeWrapper.Type);
                body.Variables.Add(definition2);
                iLProcessor.EmitLdloca(definition2);
                iLProcessor.Emit(OpCodes.Initobj, typeWrapper.Type);
                iLProcessor.EmitLdloc(definition2);
                iLProcessor.Emit(OpCodes.Box, typeWrapper.Type);
            }
            else
            {
                if (typeWrapper.SpecialConstructor == null)
                {
                    throw new Exception(string.Format("CreateInstance method can't be added for type \"{0}\".", typeWrapper.Type.FullName));
                }
                iLProcessor.EmitLdc_I4(0);
                iLProcessor.Emit(OpCodes.Conv_U);
                iLProcessor.Emit(OpCodes.Newobj, typeWrapper.SpecialConstructor);
            }
            iLProcessor.Emit(OpCodes.Ret);
            type.Methods.Add(item);
            return item;
        }

        private TypeDefinition AddUnityTypeClass(TypeWrapper typeWrapper)
        {
            string name = string.Format("$UnityType{0}", typeWrapper.Id);
            TypeDefinition item = new TypeDefinition("UnityEngine.Internal.Types", name, TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed, this.unityTypeType);
            base.Module.Types.Add(item);
            this.AddConstructor(typeWrapper, item);
            if (typeWrapper.ImplementCreateInstanceMethod)
            {
                this.AddCreateInstanceMethod(typeWrapper, item);
            }
            return item;
        }

        protected override IStepContext Execute()
        {
            this.unityTypeTypeDefinition = ((AddUnityTypeClassStep.StepContext) base.PreviousStepContext).UnityType;
            base.Execute();
            this.unityTypeTypeDefinition = null;
            return null;
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                this.unityTypeType = base.ModuleContext.Import(this.unityTypeTypeDefinition);
                this.objectType = base.ModuleContext.GetCorLibType("System.Object");
                this.typeType = base.ModuleContext.GetCorLibType("System.Type");
                this.getTypeFromHandleMethod = base.ModuleContext.GetCorLibMethod(this.typeType, "GetTypeFromHandle");
                foreach (TypeWrapper wrapper2 in assemblyWrapper.Types)
                {
                    wrapper2.UnityType = this.AddUnityTypeClass(wrapper2);
                }
                if (base.IsUnityEngine)
                {
                    using (IEnumerator<AssemblyWrapper> enumerator = base.MetadataContainer.Assemblies.Where<AssemblyWrapper>((<>c.<>9__6_0 ?? (<>c.<>9__6_0 = new Func<AssemblyWrapper, bool>(<>c.<>9.<ProcessModule>b__6_0)))).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            foreach (TypeWrapper wrapper3 in enumerator.Current.Types)
                            {
                                wrapper3.UnityType = this.AddUnityTypeClass(wrapper3);
                            }
                        }
                    }
                }
                this.unityTypeType = null;
                this.objectType = null;
                this.typeType = null;
                this.getTypeFromHandleMethod = null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddUnityTypeClassesStep.<>c <>9 = new AddUnityTypeClassesStep.<>c();
            public static Func<AssemblyWrapper, bool> <>9__6_0;

            internal bool <ProcessModule>b__6_0(AssemblyWrapper a)
            {
                return a.System;
            }
        }
    }
}


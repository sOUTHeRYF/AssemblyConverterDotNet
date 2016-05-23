namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AddGetUnityTypeMethodStep : ModuleStep
    {
        private MethodReference allocMethod;
        private MethodReference toIntPtrMethod;

        private MethodDefinition AddGetUnityTypeMethod(TypeDefinition type, AssemblyWrapper assemblyWrapper)
        {
            MethodDefinition item = new MethodDefinition("GetUnityType", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, base.ModuleContext.GetCorLibType("System.IntPtr"));
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("id", ParameterAttributes.None, base.ModuleContext.GetCorLibType("System.Int32"));
            item.Parameters.Add(definition2);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = body.GetILProcessor();
            Collection<Instruction> instructions = body.Instructions;
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdc_I4(assemblyWrapper.FirstTypeId);
            iLProcessor.Emit(OpCodes.Sub);
            Instruction[] targets = new Instruction[assemblyWrapper.Types.Length];
            iLProcessor.Emit(OpCodes.Switch, targets);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.Emit(OpCodes.Ret);
            for (int i = 0; i < assemblyWrapper.Types.Length; i++)
            {
                iLProcessor.Emit(OpCodes.Newobj, assemblyWrapper.Types[i].UnityType.Methods.Single<MethodDefinition>(<>c.<>9__3_0 ?? (<>c.<>9__3_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddGetUnityTypeMethod>b__3_0))));
                targets[i] = instructions.Last<Instruction>();
                iLProcessor.Emit(OpCodes.Call, this.allocMethod);
                iLProcessor.Emit(OpCodes.Call, this.toIntPtrMethod);
                iLProcessor.Emit(OpCodes.Ret);
            }
            return item;
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                this.allocMethod = base.ModuleContext.GetCorLibMethod("System.Runtime.InteropServices.GCHandle", <>c.<>9__2_0 ?? (<>c.<>9__2_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ProcessModule>b__2_0)));
                this.toIntPtrMethod = base.ModuleContext.GetCorLibMethod("System.Runtime.InteropServices.GCHandle", "ToIntPtr");
                TypeDefinition type = base.AddStaticClass("UnityEngine.Internal", "$Metadata");
                assemblyWrapper.GetUnityTypeMethod = this.AddGetUnityTypeMethod(type, assemblyWrapper);
                if (base.IsUnityEngine)
                {
                    foreach (AssemblyWrapper wrapper2 in base.MetadataContainer.Assemblies.Where<AssemblyWrapper>(<>c.<>9__2_1 ?? (<>c.<>9__2_1 = new Func<AssemblyWrapper, bool>(<>c.<>9.<ProcessModule>b__2_1))))
                    {
                        TypeDefinition definition2 = base.AddStaticClass("UnityEngine.Internal", string.Format("$Metadata{0}", wrapper2.SystemAssemblyId));
                        wrapper2.GetUnityTypeMethod = this.AddGetUnityTypeMethod(definition2, wrapper2);
                    }
                }
                this.allocMethod = null;
                this.toIntPtrMethod = null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddGetUnityTypeMethodStep.<>c <>9 = new AddGetUnityTypeMethodStep.<>c();
            public static Func<MethodDefinition, bool> <>9__2_0;
            public static Func<AssemblyWrapper, bool> <>9__2_1;
            public static Func<MethodDefinition, bool> <>9__3_0;

            internal bool <AddGetUnityTypeMethod>b__3_0(MethodDefinition m)
            {
                return (m.Name == ".ctor");
            }

            internal bool <ProcessModule>b__2_0(MethodDefinition m)
            {
                return ((m.Name == "Alloc") && (m.Parameters.Count == 1));
            }

            internal bool <ProcessModule>b__2_1(AssemblyWrapper a)
            {
                return a.System;
            }
        }
    }
}


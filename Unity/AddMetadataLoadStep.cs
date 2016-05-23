namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AddMetadataLoadStep : ModuleStep
    {
        private FieldDefinition AddDataField(TypeDefinition type)
        {
            FieldDefinition item = new FieldDefinition("data", FieldAttributes.CompilerControlled | FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static, base.ModuleContext.GetCorLibType("System.IntPtr"));
            type.Fields.Add(item);
            return item;
        }

        private void AddLoadMethod(TypeDefinition type, FieldDefinition dataField)
        {
            MethodDefinition item = new MethodDefinition("Load", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, base.ModuleContext.IntPtrType);
            type.Methods.Add(item);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = body.GetILProcessor();
            TypeReference corLibType = base.ModuleContext.GetCorLibType("System.Byte");
            TypeReference variableType = base.ModuleContext.GetCorLibType("System.Int32");
            TypeReference reference3 = base.ModuleContext.GetCorLibType("System.IntPtr");
            TypeReference reference4 = base.ModuleContext.GetCorLibType("System.Runtime.InteropServices.Marshal");
            TypeReference reference5 = base.ModuleContext.GetCorLibType("System.IO.Stream");
            TypeReference reference6 = base.ModuleContext.GetCorLibType("System.Type");
            VariableDefinition definition2 = new VariableDefinition("stream", reference5);
            VariableDefinition definition3 = new VariableDefinition("length", variableType);
            VariableDefinition definition4 = new VariableDefinition("buffer", new ArrayType(corLibType));
            VariableDefinition definition5 = new VariableDefinition("result", reference3);
            body.Variables.Add(definition2);
            body.Variables.Add(definition3);
            body.Variables.Add(definition4);
            body.Variables.Add(definition5);
            body.InitLocals = true;
            iLProcessor.Emit(OpCodes.Ldtoken, type);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(reference6, "GetTypeFromHandle"));
            if (base.OperationContext.IsWSA)
            {
                iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
                iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.TypeInfo", "get_Assembly"));
            }
            else
            {
                iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(reference6, "get_Assembly"));
            }
            iLProcessor.Emit(OpCodes.Ldstr, "UnityMetadata");
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.Assembly", <>c.<>9__2_0 ?? (<>c.<>9__2_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddLoadMethod>b__2_0))));
            iLProcessor.EmitStloc(definition2);
            iLProcessor.EmitLdloc(definition2);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(reference5, "get_Length"));
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.EmitStloc(definition3);
            iLProcessor.EmitLdloc(definition3);
            iLProcessor.Emit(OpCodes.Newarr, corLibType);
            iLProcessor.EmitStloc(definition4);
            iLProcessor.EmitLdloc(definition2);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdloc(definition3);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(reference5, "Read"));
            iLProcessor.Emit(OpCodes.Pop);
            iLProcessor.EmitLdloc(definition2);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(reference5, <>c.<>9__2_1 ?? (<>c.<>9__2_1 = new Func<MethodDefinition, bool>(<>c.<>9.<AddLoadMethod>b__2_1))));
            iLProcessor.EmitLdloc(definition3);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(reference4, "AllocCoTaskMem"));
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdloc(definition3);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(reference4, <>c.<>9__2_2 ?? (<>c.<>9__2_2 = new Func<MethodDefinition, bool>(<>c.<>9.<AddLoadMethod>b__2_2))));
            foreach (AssemblyWrapper wrapper in base.MetadataContainer.Assemblies)
            {
                if (wrapper.GetUnityTypeMethod != null)
                {
                    iLProcessor.EmitLdloc(definition5);
                    iLProcessor.EmitLdc_I4(wrapper.Offset);
                    iLProcessor.Emit(OpCodes.Add);
                    iLProcessor.Emit(OpCodes.Ldftn, base.ModuleContext.Import(wrapper.GetUnityTypeMethod));
                    iLProcessor.Emit(OpCodes.Stind_I);
                }
            }
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Stsfld, dataField);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Ret);
        }

        protected override void ProcessModule()
        {
            if (base.Module.Assembly.Name.Name == "UnityEngine")
            {
                TypeDefinition type = base.Module.GetType("UnityEngine.Internal", "$Metadata");
                FieldDefinition dataField = this.AddDataField(type);
                this.AddLoadMethod(type, dataField);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddMetadataLoadStep.<>c <>9 = new AddMetadataLoadStep.<>c();
            public static Func<MethodDefinition, bool> <>9__2_0;
            public static Func<MethodDefinition, bool> <>9__2_1;
            public static Func<MethodDefinition, bool> <>9__2_2;

            internal bool <AddLoadMethod>b__2_0(MethodDefinition m)
            {
                return ((m.Name == "GetManifestResourceStream") && (m.Parameters.Count == 1));
            }

            internal bool <AddLoadMethod>b__2_1(MethodDefinition m)
            {
                return ((m.Name == "Dispose") && m.IsPublic);
            }

            internal bool <AddLoadMethod>b__2_2(MethodDefinition m)
            {
                if (m.Name != "Copy")
                {
                    return false;
                }
                if (m.Parameters.Count != 4)
                {
                    return false;
                }
                TypeReference parameterType = m.Parameters[0].ParameterType;
                return (parameterType.IsArray && (((ArrayType) parameterType).ElementType.MetadataType == MetadataType.Byte));
            }
        }
    }
}


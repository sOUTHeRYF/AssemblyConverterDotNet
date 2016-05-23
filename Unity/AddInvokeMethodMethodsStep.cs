namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AddInvokeMethodMethodsStep : ModuleStep
    {
        private MethodReference gcHandleToObjectMethod;
        private MethodReference gcHandleToPinnedArrayObjectMethod;
        private TypeReference int64Type;
        private MethodReference objectToGCHandleMethod;
        private MethodReference ptrToStringUniMethod;

        protected override IStepContext Execute()
        {
            ModuleContext unityEngineModuleContext = base.OperationContext.UnityEngineModuleContext;
            TypeDefinition type = unityEngineModuleContext.Module.GetType("UnityEngine.Internal", "$MethodUtility");
            int num = 0;
            foreach (AssemblyWrapper wrapper in base.MetadataContainer.Assemblies)
            {
                if (wrapper.System)
                {
                    foreach (TypeWrapper wrapper2 in wrapper.Types)
                    {
                        if (wrapper2.Methods != null)
                        {
                            foreach (MethodWrapper wrapper3 in wrapper2.Methods)
                            {
                                type.Methods.Add(Process(unityEngineModuleContext, wrapper3, num++));
                            }
                        }
                    }
                }
            }
            return base.Execute();
        }

        private static bool IsPodStruct(TypeDefinition type)
        {
            return type.Fields.All<FieldDefinition>((<>c.<>9__8_0 ?? (<>c.<>9__8_0 = new Func<FieldDefinition, bool>(<>c.<>9.<IsPodStruct>b__8_0))));
        }

        private static MethodDefinition Process(ModuleContext moduleContext, MethodWrapper methodWrapper, int index)
        {
            TypeReference corLibType = moduleContext.GetCorLibType("System.Int64");
            MethodReference method = moduleContext.Import(moduleContext.OperationContext.GCHandledObjectsGCHandleToObjectMethod);
            MethodReference reference3 = moduleContext.Import(moduleContext.OperationContext.GCHandledObjectsGCHandleToPinnedArrayObjectMethod);
            MethodReference reference4 = moduleContext.Import(moduleContext.OperationContext.GCHandledObjectsObjectToGCHandleMethod);
            MethodReference corLibMethod = moduleContext.GetCorLibMethod("System.Runtime.InteropServices.Marshal", <>c.<>9__7_0 ?? (<>c.<>9__7_0 = new Func<MethodDefinition, bool>(<>c.<>9.<Process>b__7_0)));
            MethodDefinition targetMethod = methodWrapper.TargetMethod;
            MethodDefinition definition2 = new MethodDefinition(string.Format("$Invoke{0}", index), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, corLibType);
            methodWrapper.InvokeMethod = definition2;
            ParameterDefinition item = new ParameterDefinition("instance", ParameterAttributes.None, corLibType);
            ParameterDefinition definition4 = new ParameterDefinition("args", ParameterAttributes.None, new PointerType(corLibType));
            definition2.Parameters.Add(item);
            definition2.Parameters.Add(definition4);
            ILProcessor iLProcessor = definition2.Body.GetILProcessor();
            if (targetMethod.HasThis)
            {
                iLProcessor.EmitLdarg(item);
                iLProcessor.Emit(OpCodes.Call, method);
                if (targetMethod.DeclaringType.MetadataType != MetadataType.Object)
                {
                    iLProcessor.Emit(OpCodes.Castclass, moduleContext.Import(methodWrapper.DeclaringType.Type));
                }
            }
            for (int i = 0; i < targetMethod.Parameters.Count; i++)
            {
                TypeDefinition definition6;
                ParameterDefinition definition5 = targetMethod.Parameters[i];
                iLProcessor.EmitLdarg(definition4);
                if (i != 0)
                {
                    iLProcessor.EmitLdc_I4(i * 8);
                    iLProcessor.Emit(OpCodes.Add);
                }
                switch (definition5.ParameterType.MetadataType)
                {
                    case MetadataType.Boolean:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I1);
                        continue;
                    }
                    case MetadataType.Byte:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I1);
                        continue;
                    }
                    case MetadataType.Int32:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I4);
                        continue;
                    }
                    case MetadataType.Int64:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I8);
                        continue;
                    }
                    case MetadataType.Single:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_R4);
                        continue;
                    }
                    case MetadataType.Double:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_R8);
                        continue;
                    }
                    case MetadataType.String:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I);
                        iLProcessor.Emit(OpCodes.Call, corLibMethod);
                        continue;
                    }
                    case MetadataType.ValueType:
                    {
                        definition6 = definition5.ParameterType.Resolve();
                        if (!definition6.IsEnum)
                        {
                            break;
                        }
                        iLProcessor.Emit(OpCodes.Ldind_I4);
                        continue;
                    }
                    case MetadataType.Class:
                    case MetadataType.GenericInstance:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I8);
                        iLProcessor.Emit(OpCodes.Call, method);
                        iLProcessor.Emit(definition5.ParameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, moduleContext.Import(definition5.ParameterType));
                        continue;
                    }
                    case MetadataType.Array:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I8);
                        iLProcessor.Emit(OpCodes.Call, reference3);
                        iLProcessor.Emit(OpCodes.Castclass, moduleContext.Import(definition5.ParameterType));
                        continue;
                    }
                    case MetadataType.IntPtr:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I);
                        continue;
                    }
                    case MetadataType.Object:
                    {
                        iLProcessor.Emit(OpCodes.Ldind_I8);
                        iLProcessor.Emit(OpCodes.Call, method);
                        continue;
                    }
                    default:
                        throw new NotSupportedException(string.Format("Parameter type {0} is not supported.", definition5.ParameterType.FullName));
                }
                if (!IsPodStruct(definition6))
                {
                    iLProcessor.Emit(OpCodes.Ldind_I8);
                    iLProcessor.Emit(OpCodes.Call, method);
                    iLProcessor.Emit(OpCodes.Unbox_Any, moduleContext.Import(definition5.ParameterType));
                }
                else
                {
                    iLProcessor.Emit(OpCodes.Ldind_I);
                    iLProcessor.Emit(OpCodes.Ldobj, moduleContext.Import(definition5.ParameterType));
                }
            }
            iLProcessor.Emit(targetMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, moduleContext.Import(methodWrapper.InvokeTargetMethod));
            switch (targetMethod.ReturnType.MetadataType)
            {
                case MetadataType.Void:
                    iLProcessor.EmitLdc_I4(-1);
                    iLProcessor.Emit(OpCodes.Conv_I8);
                    break;

                case MetadataType.Boolean:
                case MetadataType.Byte:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.Single:
                case MetadataType.ValueType:
                    iLProcessor.Emit(OpCodes.Box, moduleContext.Import(targetMethod.ReturnType));
                    iLProcessor.Emit(OpCodes.Call, reference4);
                    break;

                case MetadataType.String:
                case MetadataType.Class:
                case MetadataType.Array:
                case MetadataType.GenericInstance:
                case MetadataType.Object:
                    if (targetMethod.ReturnType.IsValueType)
                    {
                        iLProcessor.Emit(OpCodes.Box, moduleContext.Import(targetMethod.ReturnType));
                    }
                    iLProcessor.Emit(OpCodes.Call, reference4);
                    break;

                default:
                    throw new NotSupportedException(string.Format("Return type {0} is not supported.", targetMethod.ReturnType.FullName));
            }
            iLProcessor.Emit(OpCodes.Ret);
            return definition2;
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                this.int64Type = base.ModuleContext.GetCorLibType("System.Int64");
                this.objectToGCHandleMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsObjectToGCHandleMethod);
                this.gcHandleToObjectMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsGCHandleToObjectMethod);
                this.gcHandleToPinnedArrayObjectMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsGCHandleToPinnedArrayObjectMethod);
                this.ptrToStringUniMethod = base.ModuleContext.GetCorLibMethod("System.Runtime.InteropServices.Marshal", <>c.<>9__6_0 ?? (<>c.<>9__6_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ProcessModule>b__6_0)));
                Dictionary<TypeDefinition, int> dictionary = new Dictionary<TypeDefinition, int>();
                foreach (TypeWrapper wrapper2 in assemblyWrapper.Types)
                {
                    TypeDefinition definition = wrapper2.Type.Resolve();
                    if (wrapper2.Methods != null)
                    {
                        foreach (MethodWrapper wrapper3 in wrapper2.Methods)
                        {
                            int num3;
                            TypeDefinition declaringType = wrapper3.TargetMethod.DeclaringType;
                            if (!dictionary.TryGetValue(declaringType, out num3))
                            {
                                num3 = 0;
                            }
                            MethodDefinition item = Process(base.ModuleContext, wrapper3, num3);
                            if (definition.IsInterface)
                            {
                                wrapper2.UnityType.Methods.Add(item);
                            }
                            else if (definition.HasGenericParameters)
                            {
                                wrapper2.UnityType.Methods.Add(item);
                                MethodDefinition targetMethod = wrapper3.TargetMethod;
                                if (targetMethod.IsPrivate || targetMethod.IsFamilyAndAssembly)
                                {
                                    targetMethod.IsAssembly = true;
                                }
                                else if (targetMethod.IsFamily)
                                {
                                    targetMethod.IsFamilyOrAssembly = true;
                                }
                            }
                            else
                            {
                                declaringType.Methods.Add(item);
                            }
                            dictionary[declaringType] = num3 + 1;
                        }
                    }
                }
                this.int64Type = null;
                this.objectToGCHandleMethod = null;
                this.gcHandleToObjectMethod = null;
                this.ptrToStringUniMethod = null;
                this.gcHandleToPinnedArrayObjectMethod = null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddInvokeMethodMethodsStep.<>c <>9 = new AddInvokeMethodMethodsStep.<>c();
            public static Func<MethodDefinition, bool> <>9__6_0;
            public static Func<MethodDefinition, bool> <>9__7_0;
            public static Func<FieldDefinition, bool> <>9__8_0;

            internal bool <IsPodStruct>b__8_0(FieldDefinition f)
            {
                return f.FieldType.IsValueType;
            }

            internal bool <Process>b__7_0(MethodDefinition m)
            {
                return ((m.Name == "PtrToStringUni") && (m.Parameters.Count == 1));
            }

            internal bool <ProcessModule>b__6_0(MethodDefinition m)
            {
                return ((m.Name == "PtrToStringUni") && (m.Parameters.Count == 1));
            }
        }
    }
}


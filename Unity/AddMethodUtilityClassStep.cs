namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AddMethodUtilityClassStep : ModuleStep
    {
        private TypeReference booleanType;
        private TypeReference byteType;
        private TypeReference charType;
        private TypeReference doubleType;
        private MethodReference gcHandleExplicitCastMethod;
        private MethodReference gcHandleFromIntPtrMethod;
        private MethodReference gcHandleGetTargetMethod;
        private MethodReference gcHandleToObjectMethod;
        private TypeReference gcHandleType;
        private MethodReference getSystemTypeMethod;
        private TypeReference int16Type;
        private TypeReference int32Type;
        private TypeReference int64Type;
        private TypeReference intPtrType;
        private MethodReference objectToGCHandleMethod;
        private TypeReference objectType;
        private TypeReference sbyteType;
        private TypeReference singleType;
        private TypeReference stringType;
        private MethodReference typeGetTypeFromHandleMethod;
        private TypeReference typeInfoType;
        private TypeReference typeType;
        private TypeReference uint16Type;
        private TypeReference uint32Type;
        private TypeReference uint64Type;
        private TypeDefinition unityTypeType;
        private TypeReference voidType;

        private void AddCreateInstanceMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("CreateInstance", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("type", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            VariableDefinition definition3 = new VariableDefinition("temp", this.gcHandleType);
            MethodBody body = item.Body;
            body.Variables.Add(definition3);
            body.InitLocals = true;
            MethodDefinition method = this.unityTypeType.Methods.Single<MethodDefinition>(<>c.<>9__29_0 ?? (<>c.<>9__29_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddCreateInstanceMethod>b__29_0)));
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleFromIntPtrMethod);
            iLProcessor.EmitStloc(definition3);
            iLProcessor.EmitLdloca(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleGetTargetMethod);
            iLProcessor.Emit(OpCodes.Castclass, this.unityTypeType);
            iLProcessor.Emit(OpCodes.Callvirt, method);
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddGetFieldStructMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("GetFieldStruct", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.singleType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition definition4 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.EmitLdarg(definition4);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(this.singleType) {
                Parameters = { new ParameterDefinition("instance", ParameterAttributes.None, this.objectType), new ParameterDefinition("index", ParameterAttributes.None, this.int32Type) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddGetFieldTypeMethod(TypeDefinition type, string name, TypeReference valueType)
        {
            MethodDefinition item = new MethodDefinition(name, MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, valueType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitLdarg(definition3);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(valueType) {
                Parameters = { new ParameterDefinition("instance", ParameterAttributes.None, this.objectType) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddGetUnityTypeMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("GetUnityType", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.intPtrType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("id", ParameterAttributes.None, this.int32Type);
            ParameterDefinition definition3 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarg(definition3);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(this.intPtrType) {
                Parameters = { new ParameterDefinition("id", ParameterAttributes.None, this.int32Type) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeDefaultConstructorMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("InvokeDefaultConstructor", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            item.Parameters.Add(definition2);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition3 = new VariableDefinition("result", this.int64Type);
            VariableDefinition definition4 = new VariableDefinition("target", this.objectType);
            VariableDefinition definition5 = new VariableDefinition("constructor", base.ModuleContext.GetCorLibType("System.Reflection.ConstructorInfo"));
            body.Variables.Add(definition3);
            body.Variables.Add(definition4);
            body.Variables.Add(definition5);
            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                CatchType = base.ModuleContext.GetCorLibType("System.Exception")
            };
            body.ExceptionHandlers.Add(handler);
            Instruction target = Utility.CreateLdloc(definition3);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition4);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitLdc_I4(0x34);
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Ldsfld, base.ModuleContext.GetCorLibField(this.typeType, "EmptyTypes"));
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeType, <>c.<>9__30_0 ?? (<>c.<>9__30_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddInvokeDefaultConstructorMethod>b__30_0))));
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            Instruction instruction2 = Utility.CreateLdc_I4(-1);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction2);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", <>c.<>9__30_1 ?? (<>c.<>9__30_1 = new Func<MethodDefinition, bool>(<>c.<>9.<AddInvokeDefaultConstructorMethod>b__30_1))));
            iLProcessor.Emit(OpCodes.Pop);
            iLProcessor.Append(instruction2);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.EmitStloc(definition3);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            handler.TryStart = body.Instructions.First<Instruction>();
            handler.TryEnd = body.Instructions.Last<Instruction>();
            handler.HandlerStart = body.Instructions.Last<Instruction>();
            handler.HandlerEnd = target;
            iLProcessor.EmitStloc(definition3);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodBooleanMethod(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodBoolean", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            body.Variables.Add(definition5);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Isinst, this.booleanType);
            Instruction target = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.booleanType);
            iLProcessor.Emit(OpCodes.Stind_I1);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(target);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodClassMethod(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodClass", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            bool isWSA = base.OperationContext.IsWSA;
            TypeReference variableType = isWSA ? this.typeInfoType : this.typeType;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("type", variableType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            if (isWSA)
            {
                iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            }
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(variableType, "get_IsValueType"));
            Instruction target = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brtrue_S, target);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdc_I4(0x18);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.EmitLdc_I4(4);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I4);
            iLProcessor.Emit(OpCodes.Call, this.getSystemTypeMethod);
            if (isWSA)
            {
                iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            }
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(variableType, "IsAssignableFrom"));
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(target);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodDoubleMethod(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodDouble", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("type", this.typeType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Ldtoken, this.doubleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction target = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.doubleType);
            iLProcessor.Emit(OpCodes.Stind_R8);
            Instruction instruction2 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ldtoken, this.int32Type);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction3 = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction3);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.int32Type);
            iLProcessor.Emit(OpCodes.Conv_R8);
            iLProcessor.Emit(OpCodes.Stind_R8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction3);
            iLProcessor.Emit(OpCodes.Ldtoken, this.singleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction4 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction4);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.singleType);
            iLProcessor.Emit(OpCodes.Conv_R8);
            iLProcessor.Emit(OpCodes.Stind_R8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction4);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodInt32Method(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodInt32", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("type", this.typeType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Ldtoken, this.int32Type);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction target = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.int32Type);
            iLProcessor.Emit(OpCodes.Stind_I4);
            Instruction instruction2 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ldtoken, this.singleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction3 = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction3);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.singleType);
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.Emit(OpCodes.Stind_I4);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction3);
            iLProcessor.Emit(OpCodes.Ldtoken, this.doubleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction4 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction4);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.doubleType);
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.Emit(OpCodes.Stind_I4);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction4);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodInt64Method(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodInt64", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("type", this.typeType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Ldtoken, this.int64Type);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction target = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.int64Type);
            iLProcessor.Emit(OpCodes.Stind_I8);
            Instruction instruction2 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ldtoken, this.int32Type);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction3 = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction3);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.int32Type);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.Emit(OpCodes.Stind_I8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction3);
            iLProcessor.Emit(OpCodes.Ldtoken, this.singleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction4 = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction4);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.singleType);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.Emit(OpCodes.Stind_I8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction4);
            iLProcessor.Emit(OpCodes.Ldtoken, this.doubleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction5 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction5);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.doubleType);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.Emit(OpCodes.Stind_I8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private MethodDefinition AddInvokeMethodMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethod", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("args", ParameterAttributes.None, new PointerType(this.int64Type));
            ParameterDefinition definition4 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("result", this.int64Type);
            body.Variables.Add(definition5);
            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                CatchType = base.ModuleContext.GetCorLibType("System.Exception")
            };
            body.ExceptionHandlers.Add(handler);
            Instruction target = Utility.CreateLdloc(definition5);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.EmitLdarg(definition4);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(this.int64Type) {
                Parameters = { new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type), new ParameterDefinition("args", ParameterAttributes.None, new PointerType(this.int64Type)) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            MethodDefinition method = base.Module.GetType("UnityEngine.MonoBehaviour").Methods.Single<MethodDefinition>(<>c.<>9__32_0 ?? (<>c.<>9__32_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddInvokeMethodMethod>b__32_0)));
            iLProcessor.Emit(OpCodes.Call, method);
            handler.TryStart = body.Instructions.First<Instruction>();
            handler.TryEnd = body.Instructions.Last<Instruction>();
            handler.HandlerStart = body.Instructions.Last<Instruction>();
            handler.HandlerEnd = target;
            iLProcessor.EmitLdc_I4(-1);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition AddInvokeMethodReflectionMethod(TypeDefinition type, MethodDefinition utf8ToStringMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodReflection", MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("result", this.int64Type);
            body.Variables.Add(definition5);
            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                CatchType = base.ModuleContext.GetCorLibType("System.Exception")
            };
            body.ExceptionHandlers.Add(handler);
            Instruction target = Utility.CreateLdloc(definition5);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdc_I4(12);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.EmitLdc_I4(4);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I4);
            iLProcessor.Emit(OpCodes.Call, this.getSystemTypeMethod);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdc_I4(4);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, utf8ToStringMethod);
            iLProcessor.EmitLdc_I4(0x134);
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Newarr, this.objectType);
            iLProcessor.Emit(OpCodes.Dup);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Stelem_Ref);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeType, <>c.<>9__34_0 ?? (<>c.<>9__34_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddInvokeMethodReflectionMethod>b__34_0))));
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            MethodDefinition method = base.Module.GetType("UnityEngine.MonoBehaviour").Methods.Single<MethodDefinition>(<>c.<>9__34_1 ?? (<>c.<>9__34_1 = new Func<MethodDefinition, bool>(<>c.<>9.<AddInvokeMethodReflectionMethod>b__34_1)));
            iLProcessor.Emit(OpCodes.Call, method);
            handler.TryStart = body.Instructions.First<Instruction>();
            handler.TryEnd = body.Instructions.Last<Instruction>();
            handler.HandlerStart = body.Instructions.Last<Instruction>();
            handler.HandlerEnd = target;
            iLProcessor.EmitLdc_I4(-1);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private void AddInvokeMethodSingleMethod(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodSingle", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("type", this.typeType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Ldtoken, this.singleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction target = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.singleType);
            iLProcessor.Emit(OpCodes.Stind_R4);
            Instruction instruction2 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ldtoken, this.int32Type);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction3 = Utility.CreateLdloc(definition6);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction3);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.int32Type);
            iLProcessor.Emit(OpCodes.Conv_R4);
            iLProcessor.Emit(OpCodes.Stind_R4);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction3);
            iLProcessor.Emit(OpCodes.Ldtoken, this.doubleType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction instruction4 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction4);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Unbox_Any, this.doubleType);
            iLProcessor.Emit(OpCodes.Conv_R4);
            iLProcessor.Emit(OpCodes.Stind_R4);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            iLProcessor.Append(instruction4);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddInvokeMethodStringMethod(TypeDefinition type, MethodDefinition invokeMethodMethod, MethodDefinition invokeMethodReflectionMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodString", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("value", this.objectType);
            VariableDefinition definition6 = new VariableDefinition("pinned", new PinnedType(this.stringType));
            VariableDefinition definition7 = new VariableDefinition("ptr", new PointerType(this.charType));
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            body.Variables.Add(definition7);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.Emit(OpCodes.Ldtoken, this.stringType);
            iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
            iLProcessor.Emit(OpCodes.Ceq);
            Instruction target = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Castclass, this.stringType);
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.Emit(OpCodes.Dup);
            Instruction instruction2 = Utility.CreateStloc(definition7);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction2);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Runtime.CompilerServices.RuntimeHelpers", "get_OffsetToStringData"));
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.Emit(OpCodes.Conv_I);
            iLProcessor.EmitLdloc(definition7);
            iLProcessor.Emit(OpCodes.Stind_I);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarga(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, invokeMethodMethod);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(target);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, invokeMethodReflectionMethod);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddSetFieldStructMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("SetFieldStruct", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.voidType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("value", ParameterAttributes.None, this.singleType);
            ParameterDefinition definition4 = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition definition5 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            item.Parameters.Add(definition5);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdarg(definition5);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(this.voidType) {
                Parameters = { new ParameterDefinition("instance", ParameterAttributes.None, this.objectType), new ParameterDefinition("value", ParameterAttributes.None, this.singleType), new ParameterDefinition("index", ParameterAttributes.None, this.int32Type) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddSetFieldTypeMethod(TypeDefinition type, string name, TypeReference valueType)
        {
            MethodDefinition item = new MethodDefinition(name, MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.voidType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("value", ParameterAttributes.None, valueType);
            ParameterDefinition definition4 = new ParameterDefinition("method", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.EmitLdarg(definition4);
            Mono.Cecil.CallSite site = new Mono.Cecil.CallSite(this.voidType) {
                Parameters = { new ParameterDefinition("instance", ParameterAttributes.None, this.objectType), new ParameterDefinition("value", ParameterAttributes.None, valueType) }
            };
            iLProcessor.Emit(OpCodes.Calli, site);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private MethodDefinition AddUtf8ToStringMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("Utf8ToString", MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, this.stringType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("ptr", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition3 = new VariableDefinition("temp", new PointerType(this.byteType));
            VariableDefinition definition4 = new VariableDefinition("length", this.int32Type);
            VariableDefinition definition5 = new VariableDefinition("bytes", new ArrayType(this.byteType));
            body.Variables.Add(definition3);
            body.Variables.Add(definition4);
            body.Variables.Add(definition5);
            MethodReference corLibMethod = base.ModuleContext.GetCorLibMethod(this.intPtrType, <>c.<>9__33_0 ?? (<>c.<>9__33_0 = new Func<MethodDefinition, bool>(<>c.<>9.<AddUtf8ToStringMethod>b__33_0)));
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, corLibMethod);
            iLProcessor.EmitStloc(definition3);
            Instruction target = Utility.CreateLdloc(definition3);
            iLProcessor.Emit(OpCodes.Br_S, target);
            Instruction instruction = Utility.CreateLdloc(definition3);
            iLProcessor.Append(instruction);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.EmitStloc(definition3);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ldind_U1);
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction);
            iLProcessor.EmitLdloc(definition3);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Sub);
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.EmitStloc(definition4);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Newarr, this.byteType);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Runtime.InteropServices.Marshal", <>c.<>9__33_1 ?? (<>c.<>9__33_1 = new Func<MethodDefinition, bool>(<>c.<>9.<AddUtf8ToStringMethod>b__33_1))));
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Text.Encoding", "get_UTF8"));
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Text.Encoding", <>c.<>9__33_2 ?? (<>c.<>9__33_2 = new Func<MethodDefinition, bool>(<>c.<>9.<AddUtf8ToStringMethod>b__33_2))));
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        protected override void ProcessModule()
        {
            if (base.IsUnityEngine)
            {
                MethodDefinition definition4;
                this.booleanType = base.ModuleContext.GetCorLibType("System.Boolean");
                this.byteType = base.ModuleContext.GetCorLibType("System.Byte");
                this.sbyteType = base.ModuleContext.GetCorLibType("System.SByte");
                this.charType = base.ModuleContext.GetCorLibType("System.Char");
                this.int16Type = base.ModuleContext.GetCorLibType("System.Int16");
                this.uint16Type = base.ModuleContext.GetCorLibType("System.UInt16");
                this.int32Type = base.ModuleContext.GetCorLibType("System.Int32");
                this.uint32Type = base.ModuleContext.GetCorLibType("System.UInt32");
                this.int64Type = base.ModuleContext.GetCorLibType("System.Int64");
                this.uint64Type = base.ModuleContext.GetCorLibType("System.UInt64");
                this.intPtrType = base.ModuleContext.GetCorLibType("System.IntPtr");
                this.singleType = base.ModuleContext.GetCorLibType("System.Single");
                this.doubleType = base.ModuleContext.GetCorLibType("System.Double");
                this.stringType = base.ModuleContext.GetCorLibType("System.String");
                this.objectType = base.ModuleContext.GetCorLibType("System.Object");
                this.voidType = base.ModuleContext.GetCorLibType("System.Void");
                this.typeType = base.ModuleContext.GetCorLibType("System.Type");
                this.typeGetTypeFromHandleMethod = base.ModuleContext.GetCorLibMethod(this.typeType, "GetTypeFromHandle");
                this.gcHandleToObjectMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsGCHandleToObjectMethod);
                this.objectToGCHandleMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsObjectToGCHandleMethod);
                this.gcHandleType = base.ModuleContext.GetCorLibType("System.Runtime.InteropServices.GCHandle");
                this.gcHandleExplicitCastMethod = base.ModuleContext.GetCorLibMethod(this.gcHandleType, <>c.<>9__27_0 ?? (<>c.<>9__27_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ProcessModule>b__27_0)));
                this.gcHandleFromIntPtrMethod = base.ModuleContext.GetCorLibMethod(this.gcHandleType, "FromIntPtr");
                this.gcHandleGetTargetMethod = base.ModuleContext.GetCorLibMethod(this.gcHandleType, "get_Target");
                this.unityTypeType = base.Module.GetType("UnityEngine.Internal", "$UnityType");
                this.getSystemTypeMethod = base.ModuleContext.Import(base.OperationContext.TypeInformationGetTypeFromTypeIdMethod);
                if (base.OperationContext.IsWSA)
                {
                    this.typeInfoType = base.ModuleContext.GetCorLibType("System.Reflection.TypeInfo");
                }
                TypeDefinition type = base.AddStaticClass("UnityEngine.Internal", "$MethodUtility");
                this.AddGetUnityTypeMethod(type);
                this.AddCreateInstanceMethod(type);
                if (base.OperationContext.IsWSA)
                {
                    this.WSA_AddInvokeDefaultConstructorMethod(type);
                }
                else
                {
                    this.AddInvokeDefaultConstructorMethod(type);
                }
                MethodDefinition invokeMethodMethod = this.AddInvokeMethodMethod(type);
                MethodDefinition definition3 = this.AddUtf8ToStringMethod(type);
                if (base.OperationContext.IsWSA)
                {
                    definition4 = this.WSA_AddInvokeMethodReflectionMethod(type, definition3);
                }
                else
                {
                    definition4 = this.AddInvokeMethodReflectionMethod(type, definition3);
                }
                this.AddInvokeMethodBooleanMethod(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodInt32Method(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodInt64Method(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodSingleMethod(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodDoubleMethod(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodStringMethod(type, invokeMethodMethod, definition4);
                this.AddInvokeMethodClassMethod(type, invokeMethodMethod, definition4);
                this.AddGetFieldTypeMethod(type, "GetFieldBoolean", this.booleanType);
                this.AddSetFieldTypeMethod(type, "SetFieldBoolean", this.booleanType);
                this.AddGetFieldTypeMethod(type, "GetFieldSingle", this.singleType);
                this.AddSetFieldTypeMethod(type, "SetFieldSingle", this.singleType);
                this.AddGetFieldStructMethod(type);
                this.AddSetFieldStructMethod(type);
                this.AddGetFieldTypeMethod(type, "GetFieldInt64", this.int64Type);
                this.AddSetFieldTypeMethod(type, "SetFieldInt64", this.int64Type);
                this.booleanType = null;
                this.byteType = null;
                this.sbyteType = null;
                this.charType = null;
                this.int16Type = null;
                this.uint16Type = null;
                this.int32Type = null;
                this.uint32Type = null;
                this.int64Type = null;
                this.uint64Type = null;
                this.intPtrType = null;
                this.singleType = null;
                this.stringType = null;
                this.objectType = null;
                this.voidType = null;
                this.typeType = null;
                this.typeGetTypeFromHandleMethod = null;
                this.gcHandleToObjectMethod = null;
                this.objectToGCHandleMethod = null;
                this.gcHandleType = null;
                this.gcHandleFromIntPtrMethod = null;
                this.gcHandleExplicitCastMethod = null;
                this.gcHandleGetTargetMethod = null;
                this.unityTypeType = null;
                this.typeInfoType = null;
            }
        }

        private MethodDefinition WSA_AddCanConvertMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("CanConvert", MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, this.booleanType);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("fromType", ParameterAttributes.None, this.typeType);
            ParameterDefinition definition3 = new ParameterDefinition("toType", ParameterAttributes.None, this.typeType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            MethodBody body = item.Body;
            Collection<VariableDefinition> variables = body.Variables;
            Collection<Instruction> instructions = body.Instructions;
            body.InitLocals = true;
            VariableDefinition definition4 = new VariableDefinition("fromTypeInfo", this.typeInfoType);
            VariableDefinition definition5 = new VariableDefinition("toTypeInfo", this.typeInfoType);
            variables.Add(definition4);
            variables.Add(definition5);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.EmitLdarg(definition3);
            Instruction target = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Bne_Un_S, target);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            iLProcessor.EmitStloc(definition4);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            iLProcessor.EmitStloc(definition5);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeInfoType, "get_IsPrimitive"));
            Instruction instruction2 = Utility.CreateLdloc(definition5);
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction2);
            iLProcessor.EmitLdloc(definition5);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeInfoType, "get_IsPrimitive"));
            Instruction instruction3 = Utility.CreateLdarg(definition2);
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction3);
            iLProcessor.Append(instruction2);
            iLProcessor.EmitLdloc(definition4);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeInfoType, "IsAssignableFrom"));
            iLProcessor.Emit(OpCodes.Ret);
            var typeArray1 = new [10];
            TypeReference[] referenceArray1 = new TypeReference[] { this.charType, this.int16Type, this.uint16Type, this.int32Type, this.uint32Type, this.int64Type, this.uint64Type, this.singleType, this.doubleType };
            typeArray1[0] = new { FromType = this.byteType, ToTypes = referenceArray1 };
            TypeReference[] referenceArray2 = new TypeReference[] { this.int16Type, this.int32Type, this.int64Type, this.singleType, this.doubleType };
            typeArray1[1] = new { FromType = this.sbyteType, ToTypes = referenceArray2 };
            TypeReference[] referenceArray3 = new TypeReference[] { this.uint16Type, this.int32Type, this.uint32Type, this.int64Type, this.uint64Type, this.singleType, this.doubleType };
            typeArray1[2] = new { FromType = this.charType, ToTypes = referenceArray3 };
            TypeReference[] referenceArray4 = new TypeReference[] { this.int32Type, this.int64Type, this.singleType, this.doubleType };
            typeArray1[3] = new { FromType = this.int16Type, ToTypes = referenceArray4 };
            TypeReference[] referenceArray5 = new TypeReference[] { this.int32Type, this.uint32Type, this.int64Type, this.uint64Type, this.singleType, this.doubleType };
            typeArray1[4] = new { FromType = this.uint16Type, ToTypes = referenceArray5 };
            TypeReference[] referenceArray6 = new TypeReference[] { this.int64Type, this.singleType, this.doubleType };
            typeArray1[5] = new { FromType = this.int32Type, ToTypes = referenceArray6 };
            TypeReference[] referenceArray7 = new TypeReference[] { this.int64Type, this.uint64Type, this.singleType, this.doubleType };
            typeArray1[6] = new { FromType = this.uint32Type, ToTypes = referenceArray7 };
            TypeReference[] referenceArray8 = new TypeReference[] { this.singleType, this.doubleType };
            typeArray1[7] = new { FromType = this.int64Type, ToTypes = referenceArray8 };
            TypeReference[] referenceArray9 = new TypeReference[] { this.singleType, this.doubleType };
            typeArray1[8] = new { FromType = this.uint64Type, ToTypes = referenceArray9 };
            TypeReference[] referenceArray10 = new TypeReference[] { this.doubleType };
            typeArray1[9] = new { FromType = this.singleType, ToTypes = referenceArray10 };
            Instruction instruction = null;
            foreach (var type2 in typeArray1)
            {
                if (instruction3 != null)
                {
                    iLProcessor.Append(instruction3);
                    instruction3 = null;
                }
                else
                {
                    iLProcessor.EmitLdarg(definition2);
                    instruction.Operand = instructions.Last<Instruction>();
                }
                iLProcessor.Emit(OpCodes.Ldtoken, type2.FromType);
                iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
                instruction = Instruction.Create(OpCodes.Bne_Un_S, instructions.Last<Instruction>());
                iLProcessor.Append(instruction);
                Instruction instruction5 = Utility.CreateLdc_I4(1);
                for (int i = 0; i < type2.ToTypes.Length; i++)
                {
                    iLProcessor.EmitLdarg(definition3);
                    iLProcessor.Emit(OpCodes.Ldtoken, type2.ToTypes[i]);
                    iLProcessor.Emit(OpCodes.Call, this.typeGetTypeFromHandleMethod);
                    if ((i + 1) != type2.ToTypes.Length)
                    {
                        iLProcessor.Emit(OpCodes.Beq_S, instruction5);
                    }
                    else
                    {
                        iLProcessor.Emit(OpCodes.Ceq);
                        iLProcessor.Emit(OpCodes.Ret);
                    }
                }
                if (type2.ToTypes.Length > 1)
                {
                    iLProcessor.Append(instruction5);
                    iLProcessor.Emit(OpCodes.Ret);
                }
            }
            iLProcessor.EmitLdc_I4(0);
            instruction.Operand = instructions.Last<Instruction>();
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition WSA_AddGetMethodMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("GetMethod", MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, base.ModuleContext.GetCorLibType("System.Reflection.MethodInfo"));
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("type", ParameterAttributes.None, this.typeType);
            ParameterDefinition definition3 = new ParameterDefinition("methodName", ParameterAttributes.None, this.stringType);
            ParameterDefinition definition4 = new ParameterDefinition("parameterType", ParameterAttributes.None, this.typeType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            ModuleDefinition corLib = base.OperationContext.CorLib;
            TypeDefinition definition5 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__36_0 ?? (<>c.<>9__36_0 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_0)))).Resolve();
            TypeDefinition definition6 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__36_1 ?? (<>c.<>9__36_1 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_1)))).Resolve();
            TypeDefinition definition7 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__36_2 ?? (<>c.<>9__36_2 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_2)))).Resolve();
            TypeDefinition definition8 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__36_3 ?? (<>c.<>9__36_3 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_3)))).Resolve();
            TypeDefinition definition9 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__36_4 ?? (<>c.<>9__36_4 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_4)))).Resolve();
            TypeReference[] arguments = new TypeReference[] { definition8 };
            MethodReference method = definition6.Methods.Single<MethodDefinition>((<>c.<>9__36_5 ?? (<>c.<>9__36_5 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_5)))).MakeGenericMethod(arguments);
            VariableDefinition definition10 = new VariableDefinition("typeInfo", base.ModuleContext.Import(definition5));
            TypeReference[] referenceArray2 = new TypeReference[] { definition8 };
            VariableDefinition definition11 = new VariableDefinition("enumerator", base.ModuleContext.Import(definition7.MakeGenericType(referenceArray2)));
            VariableDefinition definition12 = new VariableDefinition("method", base.ModuleContext.Import(definition8));
            VariableDefinition definition13 = new VariableDefinition("parameters", new ArrayType(base.ModuleContext.Import(definition9)));
            VariableDefinition definition14 = new VariableDefinition("baseType", this.typeType);
            body.Variables.Add(definition10);
            body.Variables.Add(definition11);
            body.Variables.Add(definition12);
            body.Variables.Add(definition13);
            body.Variables.Add(definition14);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            iLProcessor.EmitStloc(definition10);
            iLProcessor.EmitLdloc(definition10);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(definition5, "get_DeclaredMethods"));
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.Import(method));
            iLProcessor.EmitStloc(definition11);
            Instruction target = Utility.CreateLdloc(definition11);
            iLProcessor.Emit(OpCodes.Br_S, target);
            Instruction instruction = Utility.CreateLdloc(definition11);
            iLProcessor.Append(instruction);
            TypeReference[] referenceArray3 = new TypeReference[] { definition8 };
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.Import(definition7.Methods.Single<MethodDefinition>((<>c.<>9__36_6 ?? (<>c.<>9__36_6 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_6)))).MakeGenericMethod(referenceArray3)));
            iLProcessor.EmitStloc(definition12);
            iLProcessor.EmitLdloc(definition12);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MemberInfo", "get_Name"));
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.stringType, "op_Equality"));
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdloc(definition12);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "get_IsStatic"));
            iLProcessor.Emit(OpCodes.Brtrue_S, target);
            iLProcessor.EmitLdloc(definition12);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "GetParameters"));
            iLProcessor.EmitStloc(definition13);
            iLProcessor.EmitLdloc(definition13);
            iLProcessor.Emit(OpCodes.Ldlen);
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Bne_Un_S, target);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdloc(definition13);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.Emit(OpCodes.Ldelem_Ref);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.ParameterInfo", "get_ParameterType"));
            iLProcessor.Emit(OpCodes.Call, this.WSA_AddCanConvertMethod(type));
            iLProcessor.Emit(OpCodes.Brfalse_S, target);
            iLProcessor.EmitLdloc(definition12);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Collections.IEnumerator", "MoveNext"));
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction);
            iLProcessor.EmitLdloc(definition10);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(definition5, "get_BaseType"));
            iLProcessor.EmitStloc(definition14);
            iLProcessor.EmitLdloc(definition14);
            Instruction instruction3 = Instruction.Create(OpCodes.Ldstr, "{0} method not found.");
            iLProcessor.Emit(OpCodes.Brfalse_S, instruction3);
            iLProcessor.EmitLdloc(definition14);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.Emit(OpCodes.Call, item);
            iLProcessor.Emit(OpCodes.Ret);
            iLProcessor.Append(instruction3);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Newarr, this.objectType);
            iLProcessor.Emit(OpCodes.Dup);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Stelem_Ref);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.stringType, <>c.<>9__36_7 ?? (<>c.<>9__36_7 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_7))));
            iLProcessor.Emit(OpCodes.Newobj, base.ModuleContext.GetCorLibMethod("System.Exception", <>c.<>9__36_8 ?? (<>c.<>9__36_8 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddGetMethodMethod>b__36_8))));
            iLProcessor.Emit(OpCodes.Throw);
            return item;
        }

        private void WSA_AddInvokeDefaultConstructorMethod(TypeDefinition type)
        {
            MethodDefinition item = new MethodDefinition("InvokeDefaultConstructor", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            item.Parameters.Add(definition2);
            MethodBody body = item.Body;
            body.InitLocals = true;
            ModuleDefinition corLib = base.OperationContext.CorLib;
            TypeDefinition definition3 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__31_0 ?? (<>c.<>9__31_0 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddInvokeDefaultConstructorMethod>b__31_0)))).Resolve();
            TypeDefinition definition4 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__31_1 ?? (<>c.<>9__31_1 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddInvokeDefaultConstructorMethod>b__31_1)))).Resolve();
            TypeDefinition definition5 = corLib.ExportedTypes.Single<ExportedType>((<>c.<>9__31_2 ?? (<>c.<>9__31_2 = new Func<ExportedType, bool>(<>c.<>9.<WSA_AddInvokeDefaultConstructorMethod>b__31_2)))).Resolve();
            TypeReference[] arguments = new TypeReference[] { definition5 };
            MethodReference method = definition3.Methods.Single<MethodDefinition>((<>c.<>9__31_3 ?? (<>c.<>9__31_3 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddInvokeDefaultConstructorMethod>b__31_3)))).MakeGenericMethod(arguments);
            VariableDefinition definition6 = new VariableDefinition("result", this.int64Type);
            VariableDefinition definition7 = new VariableDefinition("target", this.objectType);
            TypeReference[] referenceArray2 = new TypeReference[] { definition5 };
            VariableDefinition definition8 = new VariableDefinition("enumerator", base.ModuleContext.Import(definition4.MakeGenericType(referenceArray2)));
            VariableDefinition definition9 = new VariableDefinition("constructor", base.ModuleContext.GetCorLibType("System.Reflection.ConstructorInfo"));
            body.Variables.Add(definition6);
            body.Variables.Add(definition7);
            body.Variables.Add(definition8);
            body.Variables.Add(definition9);
            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                CatchType = base.ModuleContext.GetCorLibType("System.Exception")
            };
            body.ExceptionHandlers.Add(handler);
            ILProcessor iLProcessor = body.GetILProcessor();
            Instruction target = Utility.CreateLdloc(definition6);
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition7);
            iLProcessor.EmitLdloc(definition7);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod("System.Reflection.IntrospectionExtensions", "GetTypeInfo"));
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.typeInfoType, "get_DeclaredConstructors"));
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.Import(method));
            iLProcessor.EmitStloc(definition8);
            Instruction instruction2 = Utility.CreateLdloc(definition8);
            iLProcessor.Emit(OpCodes.Br_S, instruction2);
            Instruction instruction = Utility.CreateLdloc(definition8);
            iLProcessor.Append(instruction);
            TypeReference[] referenceArray3 = new TypeReference[] { definition5 };
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.Import(definition4.Methods.Single<MethodDefinition>((<>c.<>9__31_4 ?? (<>c.<>9__31_4 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddInvokeDefaultConstructorMethod>b__31_4)))).MakeGenericMethod(referenceArray3)));
            iLProcessor.EmitStloc(definition9);
            iLProcessor.EmitLdloc(definition9);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "get_IsStatic"));
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction2);
            iLProcessor.EmitLdloc(definition9);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "GetParameters"));
            iLProcessor.Emit(OpCodes.Ldlen);
            iLProcessor.Emit(OpCodes.Conv_I4);
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction2);
            iLProcessor.EmitLdloc(definition9);
            iLProcessor.EmitLdloc(definition7);
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "Invoke"));
            iLProcessor.Emit(OpCodes.Pop);
            Instruction instruction4 = Utility.CreateLdc_I4(-1);
            iLProcessor.Emit(OpCodes.Br_S, instruction4);
            iLProcessor.Append(instruction2);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Collections.IEnumerator", "MoveNext"));
            iLProcessor.Emit(OpCodes.Brtrue_S, instruction);
            iLProcessor.Append(instruction4);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.EmitStloc(definition6);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            handler.TryStart = body.Instructions.First<Instruction>();
            handler.TryEnd = body.Instructions.Last<Instruction>();
            handler.HandlerStart = body.Instructions.Last<Instruction>();
            handler.HandlerEnd = target;
            iLProcessor.EmitStloc(definition6);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private MethodDefinition WSA_AddInvokeMethodReflectionMethod(TypeDefinition type, MethodDefinition utf8ToStringMethod)
        {
            MethodDefinition item = new MethodDefinition("InvokeMethodReflection", MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, this.int64Type);
            type.Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.int64Type);
            ParameterDefinition definition3 = new ParameterDefinition("arg", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition4 = new ParameterDefinition("unityMethod", ParameterAttributes.None, this.intPtrType);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            body.InitLocals = true;
            VariableDefinition definition5 = new VariableDefinition("result", this.int64Type);
            VariableDefinition definition6 = new VariableDefinition("target", this.objectType);
            body.Variables.Add(definition5);
            body.Variables.Add(definition6);
            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                CatchType = base.ModuleContext.GetCorLibType("System.Exception")
            };
            body.ExceptionHandlers.Add(handler);
            Instruction target = Utility.CreateLdloc(definition5);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.EmitStloc(definition6);
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.EmitLdarg(definition4);
            iLProcessor.EmitLdc_I4(base.OperationContext.Is64 ? 8 : 4);
            iLProcessor.Emit(OpCodes.Add);
            iLProcessor.Emit(OpCodes.Ldind_I);
            iLProcessor.Emit(OpCodes.Call, utf8ToStringMethod);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod(this.objectType, "GetType"));
            iLProcessor.Emit(OpCodes.Call, this.WSA_AddGetMethodMethod(type));
            iLProcessor.EmitLdloc(definition6);
            iLProcessor.EmitLdc_I4(1);
            iLProcessor.Emit(OpCodes.Newarr, this.objectType);
            iLProcessor.Emit(OpCodes.Dup);
            iLProcessor.EmitLdc_I4(0);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Stelem_Ref);
            iLProcessor.Emit(OpCodes.Callvirt, base.ModuleContext.GetCorLibMethod("System.Reflection.MethodBase", "Invoke"));
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            MethodDefinition method = base.Module.GetType("UnityEngine.MonoBehaviour").Methods.Single<MethodDefinition>(<>c.<>9__35_0 ?? (<>c.<>9__35_0 = new Func<MethodDefinition, bool>(<>c.<>9.<WSA_AddInvokeMethodReflectionMethod>b__35_0)));
            iLProcessor.Emit(OpCodes.Call, method);
            handler.TryStart = body.Instructions.First<Instruction>();
            handler.TryEnd = body.Instructions.Last<Instruction>();
            handler.HandlerStart = body.Instructions.Last<Instruction>();
            handler.HandlerEnd = target;
            iLProcessor.EmitLdc_I4(-1);
            iLProcessor.Emit(OpCodes.Conv_I8);
            iLProcessor.EmitStloc(definition5);
            iLProcessor.Emit(OpCodes.Leave_S, target);
            iLProcessor.Append(target);
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddMethodUtilityClassStep.<>c <>9 = new AddMethodUtilityClassStep.<>c();
            public static Func<MethodDefinition, bool> <>9__27_0;
            public static Func<MethodDefinition, bool> <>9__29_0;
            public static Func<MethodDefinition, bool> <>9__30_0;
            public static Func<MethodDefinition, bool> <>9__30_1;
            public static Func<ExportedType, bool> <>9__31_0;
            public static Func<ExportedType, bool> <>9__31_1;
            public static Func<ExportedType, bool> <>9__31_2;
            public static Func<MethodDefinition, bool> <>9__31_3;
            public static Func<MethodDefinition, bool> <>9__31_4;
            public static Func<MethodDefinition, bool> <>9__32_0;
            public static Func<MethodDefinition, bool> <>9__33_0;
            public static Func<MethodDefinition, bool> <>9__33_1;
            public static Func<MethodDefinition, bool> <>9__33_2;
            public static Func<MethodDefinition, bool> <>9__34_0;
            public static Func<MethodDefinition, bool> <>9__34_1;
            public static Func<MethodDefinition, bool> <>9__35_0;
            public static Func<ExportedType, bool> <>9__36_0;
            public static Func<ExportedType, bool> <>9__36_1;
            public static Func<ExportedType, bool> <>9__36_2;
            public static Func<ExportedType, bool> <>9__36_3;
            public static Func<ExportedType, bool> <>9__36_4;
            public static Func<MethodDefinition, bool> <>9__36_5;
            public static Func<MethodDefinition, bool> <>9__36_6;
            public static Func<MethodDefinition, bool> <>9__36_7;
            public static Func<MethodDefinition, bool> <>9__36_8;

            internal bool <AddCreateInstanceMethod>b__29_0(MethodDefinition m)
            {
                return (m.Name == "CreateInstance");
            }

            internal bool <AddInvokeDefaultConstructorMethod>b__30_0(MethodDefinition m)
            {
                return ((m.Name == "GetConstructor") && (m.Parameters.Count == 4));
            }

            internal bool <AddInvokeDefaultConstructorMethod>b__30_1(MethodDefinition m)
            {
                return ((m.Name == "Invoke") && (m.Parameters.Count == 2));
            }

            internal bool <AddInvokeMethodMethod>b__32_0(MethodDefinition m)
            {
                return (m.Name == "HandleFastCallException");
            }

            internal bool <AddInvokeMethodReflectionMethod>b__34_0(MethodDefinition m)
            {
                return ((m.Name == "InvokeMember") && (m.Parameters.Count == 5));
            }

            internal bool <AddInvokeMethodReflectionMethod>b__34_1(MethodDefinition m)
            {
                return (m.Name == "HandleFastCallException");
            }

            internal bool <AddUtf8ToStringMethod>b__33_0(MethodDefinition m)
            {
                return ((((m.Name == "op_Explicit") && (m.Parameters.Count == 1)) && (m.Parameters[0].ParameterType.MetadataType == MetadataType.IntPtr)) && m.ReturnType.IsPointer);
            }

            internal bool <AddUtf8ToStringMethod>b__33_1(MethodDefinition m)
            {
                if (m.Name != "Copy")
                {
                    return false;
                }
                Collection<ParameterDefinition> parameters = m.Parameters;
                if (parameters.Count != 4)
                {
                    return false;
                }
                return ((((parameters[0].ParameterType.MetadataType == MetadataType.IntPtr) && (parameters[1].ParameterType.FullName == "System.Byte[]")) && (parameters[2].ParameterType.MetadataType == MetadataType.Int32)) && (parameters[3].ParameterType.MetadataType == MetadataType.Int32));
            }

            internal bool <AddUtf8ToStringMethod>b__33_2(MethodDefinition m)
            {
                if (m.Name != "GetString")
                {
                    return false;
                }
                Collection<ParameterDefinition> parameters = m.Parameters;
                if (parameters.Count != 3)
                {
                    return false;
                }
                return (((parameters[0].ParameterType.FullName == "System.Byte[]") && (parameters[1].ParameterType.MetadataType == MetadataType.Int32)) && (parameters[2].ParameterType.MetadataType == MetadataType.Int32));
            }

            internal bool <ProcessModule>b__27_0(MethodDefinition m)
            {
                return ((m.Name == "op_Explicit") && (m.Parameters[0].ParameterType.MetadataType == MetadataType.IntPtr));
            }

            internal bool <WSA_AddGetMethodMethod>b__36_0(ExportedType t)
            {
                return (t.FullName == "System.Reflection.TypeInfo");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_1(ExportedType t)
            {
                return (t.FullName == "System.Collections.Generic.IEnumerable`1");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_2(ExportedType t)
            {
                return (t.FullName == "System.Collections.Generic.IEnumerator`1");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_3(ExportedType t)
            {
                return (t.FullName == "System.Reflection.MethodInfo");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_4(ExportedType t)
            {
                return (t.FullName == "System.Reflection.ParameterInfo");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_5(MethodDefinition m)
            {
                return (m.Name == "GetEnumerator");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_6(MethodDefinition m)
            {
                return (m.Name == "get_Current");
            }

            internal bool <WSA_AddGetMethodMethod>b__36_7(MethodDefinition m)
            {
                return ((((m.Name == "Format") && (m.Parameters.Count == 2)) && m.Parameters[1].ParameterType.IsArray) && (((ArrayType) m.Parameters[1].ParameterType).ElementType.MetadataType == MetadataType.Object));
            }

            internal bool <WSA_AddGetMethodMethod>b__36_8(MethodDefinition m)
            {
                return (m.IsConstructor && (m.Parameters.Count == 1));
            }

            internal bool <WSA_AddInvokeDefaultConstructorMethod>b__31_0(ExportedType t)
            {
                return (t.FullName == "System.Collections.Generic.IEnumerable`1");
            }

            internal bool <WSA_AddInvokeDefaultConstructorMethod>b__31_1(ExportedType t)
            {
                return (t.FullName == "System.Collections.Generic.IEnumerator`1");
            }

            internal bool <WSA_AddInvokeDefaultConstructorMethod>b__31_2(ExportedType t)
            {
                return (t.FullName == "System.Reflection.ConstructorInfo");
            }

            internal bool <WSA_AddInvokeDefaultConstructorMethod>b__31_3(MethodDefinition m)
            {
                return (m.Name == "GetEnumerator");
            }

            internal bool <WSA_AddInvokeDefaultConstructorMethod>b__31_4(MethodDefinition m)
            {
                return (m.Name == "get_Current");
            }

            internal bool <WSA_AddInvokeMethodReflectionMethod>b__35_0(MethodDefinition m)
            {
                return (m.Name == "HandleFastCallException");
            }
        }
    }
}


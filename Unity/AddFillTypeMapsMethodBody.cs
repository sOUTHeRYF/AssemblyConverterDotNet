namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AddFillTypeMapsMethodBody : Step
    {
        private TypeDefinition bootstrapHelpers;
        private MethodReference dictionaryAddMethod;
        private GenericInstanceType dictionaryInstanceType;
        private TypeDefinition dictionaryType;
        private MethodDefinition fillMapsMethod;
        private TypeReference int32Type;
        private MethodReference listAddMethod;
        private GenericInstanceType listInstanceType;
        private TypeDefinition listType;
        private TypeReference systemTypeType;
        private MethodReference typeOfMethod;
        private TypeReference voidType;

        private void ConstructDictionary(ILProcessor ilProcessor, int typeCount)
        {
            ilProcessor.Body.Variables.Add(new VariableDefinition(this.dictionaryInstanceType));
            MethodDefinition method = this.dictionaryType.Methods.Single<MethodDefinition>(InnerClass.FuncC ?? (InnerClass.FuncC = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.IsConstructDictionarySpecial)));
            TypeReference[] arguments = new TypeReference[] { this.systemTypeType, this.int32Type };
            MethodReference reference = this.Import(method).MakeGenericMethod(arguments);
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.EmitLdc_I4(typeCount);
            ilProcessor.Emit(OpCodes.Newobj, reference);
            ilProcessor.Emit(OpCodes.Dup);
            ilProcessor.Emit(OpCodes.Stloc_0);
            ilProcessor.Emit(OpCodes.Stind_Ref);
        }

        private void ConstructList(ILProcessor ilProcessor, int typeCount)
        {
            ilProcessor.Body.Variables.Add(new VariableDefinition(this.listInstanceType));
            MethodDefinition method = this.listType.Methods.Single<MethodDefinition>(InnerClass.FuncD ?? (InnerClass.FuncD = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.IsConstructListSpecial)));
            TypeReference[] arguments = new TypeReference[] { this.systemTypeType };
            MethodReference reference = this.Import(method).MakeGenericMethod(arguments);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.EmitLdc_I4(typeCount);
            ilProcessor.Emit(OpCodes.Newobj, reference);
            ilProcessor.Emit(OpCodes.Dup);
            ilProcessor.Emit(OpCodes.Stloc_1);
            ilProcessor.Emit(OpCodes.Stind_Ref);
        }

        protected override IStepContext Execute()
        {
            this.Initialize();
            IOrderedEnumerable<TypeWrapper> source = base.MetadataContainer.Assemblies.SelectMany<AssemblyWrapper, TypeWrapper>((InnerClass.FuncA ?? (InnerClass.FuncA = new Func<AssemblyWrapper, IEnumerable<TypeWrapper>>(InnerClass.InnerInstance.GetExecuteParamA)))).OrderBy<TypeWrapper, int>(InnerClass.FuncB ?? (InnerClass.FuncB = new Func<TypeWrapper, int>(InnerClass.InnerInstance.GetExecuteParamB)));
            int typeCount = source.Last<TypeWrapper>().Id + 1;
            this.fillMapsMethod.Body.Instructions.Clear();
            MethodBody body = this.fillMapsMethod.Body;
            body.InitLocals = true;
            ILProcessor iLProcessor = body.GetILProcessor();
            this.ConstructDictionary(iLProcessor, typeCount);
            this.ConstructList(iLProcessor, typeCount);
            iLProcessor.Emit(OpCodes.Ldloc_1);
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Callvirt, this.listAddMethod);
            int num2 = 0;
            foreach (IEnumerable<TypeWrapper> enumerable in GroupItemsInChunks<TypeWrapper>(source, 0x1388))
            {
                MethodDefinition method = this.GenerateFillTypeMapsMethod(enumerable, "FillTypeMaps" + num2, this.bootstrapHelpers);
                num2++;
                iLProcessor.Emit(OpCodes.Ldloc_0);
                iLProcessor.Emit(OpCodes.Ldloc_1);
                iLProcessor.Emit(OpCodes.Call, method);
            }
            iLProcessor.Emit(OpCodes.Ret);
            return null;
        }

        private void FillMapsForType(ILProcessor ilProcessor, TypeWrapper type)
        {
            ilProcessor.Emit(OpCodes.Ldtoken, this.Import(type.Type));
            ilProcessor.Emit(OpCodes.Call, this.typeOfMethod);
            ilProcessor.Emit(OpCodes.Stloc_0);
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldloc_0);
            ilProcessor.EmitLdc_I4(type.Id);
            ilProcessor.Emit(OpCodes.Callvirt, this.dictionaryAddMethod);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldloc_0);
            ilProcessor.Emit(OpCodes.Callvirt, this.listAddMethod);
        }

        private MethodDefinition GenerateFillTypeMapsMethod(IEnumerable<TypeWrapper> types, string methodName, TypeDefinition declaringType)
        {
            MethodDefinition item = new MethodDefinition(methodName, MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, this.voidType);
            declaringType.Methods.Add(item);
            item.Parameters.Add(new ParameterDefinition(this.dictionaryInstanceType));
            item.Parameters.Add(new ParameterDefinition(this.listInstanceType));
            MethodBody body = item.Body;
            body.Variables.Add(new VariableDefinition(this.systemTypeType));
            ILProcessor iLProcessor = body.GetILProcessor();
            foreach (TypeWrapper wrapper in types)
            {
                this.FillMapsForType(iLProcessor, wrapper);
            }
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

      //  [IteratorStateMachine(typeof(GroupItemsInChunksInnerClass<T>))]
        private static IEnumerable<IEnumerable<T>> GroupItemsInChunks<T>(IEnumerable<T> paramAllTypes, int paramChunkSize)
        {
            return new GroupItemsInChunksInnerClass<T>(-2) { AllTypes = paramAllTypes, ChunkSize = paramChunkSize };
        }

        private MethodReference Import(MethodReference method)
        {
            if (method.DeclaringType.Scope != this.bootstrapHelpers.Scope)
            {
                return base.OperationContext.UnityEngineModuleContext.Import(method);
            }
            return method;
        }

        private TypeReference Import(TypeReference type)
        {
            if (type.Scope != this.bootstrapHelpers.Scope)
            {
                return base.OperationContext.UnityEngineModuleContext.Import(type);
            }
            return type;
        }

        private void Initialize()
        {
            this.bootstrapHelpers = base.OperationContext.UnityEngineModuleContext.Module.GetType("UnityEngineInternal.BootstrapHelpers");
            this.fillMapsMethod = this.bootstrapHelpers.Methods.Single<MethodDefinition>(InnerClass.FuncE ?? (InnerClass.FuncE = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.GetInitializeParamA)));
            this.dictionaryType = base.OperationContext.UnityEngineModuleContext.GetCorLibType("System.Collections.Generic.Dictionary`2").Resolve();
            this.listType = base.OperationContext.UnityEngineModuleContext.GetCorLibType("System.Collections.Generic.List`1").Resolve();
            this.systemTypeType = base.OperationContext.UnityEngineModuleContext.GetCorLibType("System.Type");
            this.int32Type = base.OperationContext.UnityEngineModuleContext.Int32Type;
            this.voidType = base.OperationContext.UnityEngineModuleContext.Module.TypeSystem.Void;
            this.dictionaryInstanceType = new GenericInstanceType(this.Import(this.dictionaryType));
            this.dictionaryInstanceType.GenericArguments.Add(this.systemTypeType);
            this.dictionaryInstanceType.GenericArguments.Add(this.int32Type);
            this.listInstanceType = new GenericInstanceType(this.Import(this.listType));
            this.listInstanceType.GenericArguments.Add(this.systemTypeType);
            MethodDefinition setMethod = this.dictionaryType.Properties.Single<PropertyDefinition>((InnerClass.FuncF ?? (InnerClass.FuncF = new Func<PropertyDefinition, bool>(InnerClass.InnerInstance.GetInitializeParamB)))).SetMethod;
            TypeReference[] arguments = new TypeReference[] { this.systemTypeType, this.int32Type };
            this.dictionaryAddMethod = this.Import(setMethod).MakeGenericMethod(arguments);
            MethodDefinition method = this.listType.Methods.Single<MethodDefinition>(InnerClass.FuncG ?? (InnerClass.FuncG = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.GetInitializeParamC)));
            TypeReference[] referenceArray2 = new TypeReference[] { this.systemTypeType };
            this.listAddMethod = this.Import(method).MakeGenericMethod(referenceArray2);
            this.typeOfMethod = this.Import(this.systemTypeType.Resolve().Methods.Single<MethodDefinition>(InnerClass.FuncH ?? (InnerClass.FuncH = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.GetInitializeParamD))));
        }

        [Serializable, CompilerGenerated]
        private sealed class InnerClass
        {
            public static readonly AddFillTypeMapsMethodBody.InnerClass InnerInstance = new AddFillTypeMapsMethodBody.InnerClass();
            public static Func<AssemblyWrapper, IEnumerable<TypeWrapper>> FuncA;
            public static Func<TypeWrapper, int> FuncB;
            public static Func<MethodDefinition, bool> FuncC;
            public static Func<MethodDefinition, bool> FuncD;
            public static Func<MethodDefinition, bool> FuncE;
            public static Func<PropertyDefinition, bool> FuncF;
            public static Func<MethodDefinition, bool> FuncG;
            public static Func<MethodDefinition, bool> FuncH;

            internal bool IsConstructDictionarySpecial(MethodDefinition x)
            {
                return (((x.Name == ".ctor") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.Int32"));
            }

            internal bool IsConstructListSpecial(MethodDefinition x)
            {
                return (((x.Name == ".ctor") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.Int32"));
            }

            internal IEnumerable<TypeWrapper> GetExecuteParamA(AssemblyWrapper x)
            {
                return x.Types;
            }

            internal int GetExecuteParamB(TypeWrapper x)
            {
                return x.Id;
            }

            internal bool GetInitializeParamA(MethodDefinition x)
            {
                return (x.Name == "FillTypeMaps");
            }

            internal bool GetInitializeParamB(PropertyDefinition x)
            {
                return (x.Name == "Item");
            }

            internal bool GetInitializeParamC(MethodDefinition x)
            {
                return (x.Name == "Add");
            }

            internal bool GetInitializeParamD(MethodDefinition x)
            {
                return (((x.Name == "GetTypeFromHandle") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.RuntimeTypeHandle"));
            }
        }

        [CompilerGenerated]
        private sealed class GroupItemsInChunksInnerClass<T> : IEnumerable<IEnumerable<T>>, IEnumerable, IEnumerator<IEnumerable<T>>, IDisposable, IEnumerator
        {
            private int state;
            private IEnumerable<T> current;
            public IEnumerable<T> AllTypes;
            public int ChunkSize;
            private int initialThreadId;
            private IEnumerable<T> allTypes;
            private int chunkSize;

            [DebuggerHidden]
            public GroupItemsInChunksInnerClass(int paramState)
            {
                this.state = paramState;
                this.initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.state;
                bool result = false;
                if (num == 0)
                {
                    this.state = -1;
                    while (this.allTypes.Any<T>())
                    {
                        int num2 = this.allTypes.Count<T>();
                        if (this.chunkSize > num2)
                        {
                            this.chunkSize = num2;
                        }
                        this.current = this.allTypes.Take<T>(this.chunkSize);
                        this.state = 1;
                        return true;
                    Label_0055:
                        this.state = -1;
                        this.allTypes = this.allTypes.Skip<T>(this.chunkSize);
                    }
                    return false;
                }
                if (num != 1)
                {
                    return false;
                }
                goto Label_0055;
            }

            [DebuggerHidden]
            IEnumerator<IEnumerable<T>> IEnumerable<IEnumerable<T>>.GetEnumerator()
            {
                AddFillTypeMapsMethodBody.GroupItemsInChunksInnerClass<T> d__;
                if ((this.state == -2) && (this.initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.state = 0;
                    d__ = (AddFillTypeMapsMethodBody.GroupItemsInChunksInnerClass<T>) this;
                }
                else
                {
                    d__ = new AddFillTypeMapsMethodBody.GroupItemsInChunksInnerClass<T>(0);
                }
                d__.allTypes = this.AllTypes;
                d__.chunkSize = this.ChunkSize;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            IEnumerable<T> IEnumerator<IEnumerable<T>>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.current;
                }
            }
        }
    }
}


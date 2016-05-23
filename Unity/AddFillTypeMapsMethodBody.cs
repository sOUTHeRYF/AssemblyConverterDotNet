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
            MethodDefinition method = this.dictionaryType.Methods.Single<MethodDefinition>(<>c.<>9__15_0 ?? (<>c.<>9__15_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ConstructDictionary>b__15_0)));
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
            MethodDefinition method = this.listType.Methods.Single<MethodDefinition>(<>c.<>9__16_0 ?? (<>c.<>9__16_0 = new Func<MethodDefinition, bool>(<>c.<>9.<ConstructList>b__16_0)));
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
            IOrderedEnumerable<TypeWrapper> source = base.MetadataContainer.Assemblies.SelectMany<AssemblyWrapper, TypeWrapper>((<>c.<>9__12_0 ?? (<>c.<>9__12_0 = new Func<AssemblyWrapper, IEnumerable<TypeWrapper>>(<>c.<>9.<Execute>b__12_0)))).OrderBy<TypeWrapper, int>(<>c.<>9__12_1 ?? (<>c.<>9__12_1 = new Func<TypeWrapper, int>(<>c.<>9.<Execute>b__12_1)));
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

        [IteratorStateMachine(typeof(<GroupItemsInChunks>d__20))]
        private static IEnumerable<IEnumerable<T>> GroupItemsInChunks<T>(IEnumerable<T> allTypes, int chunkSize)
        {
            return new <GroupItemsInChunks>d__20<T>(-2) { <>3__allTypes = allTypes, <>3__chunkSize = chunkSize };
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
            this.fillMapsMethod = this.bootstrapHelpers.Methods.Single<MethodDefinition>(<>c.<>9__17_0 ?? (<>c.<>9__17_0 = new Func<MethodDefinition, bool>(<>c.<>9.<Initialize>b__17_0)));
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
            MethodDefinition setMethod = this.dictionaryType.Properties.Single<PropertyDefinition>((<>c.<>9__17_1 ?? (<>c.<>9__17_1 = new Func<PropertyDefinition, bool>(<>c.<>9.<Initialize>b__17_1)))).SetMethod;
            TypeReference[] arguments = new TypeReference[] { this.systemTypeType, this.int32Type };
            this.dictionaryAddMethod = this.Import(setMethod).MakeGenericMethod(arguments);
            MethodDefinition method = this.listType.Methods.Single<MethodDefinition>(<>c.<>9__17_2 ?? (<>c.<>9__17_2 = new Func<MethodDefinition, bool>(<>c.<>9.<Initialize>b__17_2)));
            TypeReference[] referenceArray2 = new TypeReference[] { this.systemTypeType };
            this.listAddMethod = this.Import(method).MakeGenericMethod(referenceArray2);
            this.typeOfMethod = this.Import(this.systemTypeType.Resolve().Methods.Single<MethodDefinition>(<>c.<>9__17_3 ?? (<>c.<>9__17_3 = new Func<MethodDefinition, bool>(<>c.<>9.<Initialize>b__17_3))));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AddFillTypeMapsMethodBody.<>c <>9 = new AddFillTypeMapsMethodBody.<>c();
            public static Func<AssemblyWrapper, IEnumerable<TypeWrapper>> <>9__12_0;
            public static Func<TypeWrapper, int> <>9__12_1;
            public static Func<MethodDefinition, bool> <>9__15_0;
            public static Func<MethodDefinition, bool> <>9__16_0;
            public static Func<MethodDefinition, bool> <>9__17_0;
            public static Func<PropertyDefinition, bool> <>9__17_1;
            public static Func<MethodDefinition, bool> <>9__17_2;
            public static Func<MethodDefinition, bool> <>9__17_3;

            internal bool <ConstructDictionary>b__15_0(MethodDefinition x)
            {
                return (((x.Name == ".ctor") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.Int32"));
            }

            internal bool <ConstructList>b__16_0(MethodDefinition x)
            {
                return (((x.Name == ".ctor") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.Int32"));
            }

            internal IEnumerable<TypeWrapper> <Execute>b__12_0(AssemblyWrapper x)
            {
                return x.Types;
            }

            internal int <Execute>b__12_1(TypeWrapper x)
            {
                return x.Id;
            }

            internal bool <Initialize>b__17_0(MethodDefinition x)
            {
                return (x.Name == "FillTypeMaps");
            }

            internal bool <Initialize>b__17_1(PropertyDefinition x)
            {
                return (x.Name == "Item");
            }

            internal bool <Initialize>b__17_2(MethodDefinition x)
            {
                return (x.Name == "Add");
            }

            internal bool <Initialize>b__17_3(MethodDefinition x)
            {
                return (((x.Name == "GetTypeFromHandle") && (x.Parameters.Count == 1)) && (x.Parameters[0].ParameterType.FullName == "System.RuntimeTypeHandle"));
            }
        }

        [CompilerGenerated]
        private sealed class <GroupItemsInChunks>d__20<T> : IEnumerable<IEnumerable<T>>, IEnumerable, IEnumerator<IEnumerable<T>>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private IEnumerable<T> <>2__current;
            public IEnumerable<T> <>3__allTypes;
            public int <>3__chunkSize;
            private int <>l__initialThreadId;
            private IEnumerable<T> allTypes;
            private int chunkSize;

            [DebuggerHidden]
            public <GroupItemsInChunks>d__20(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    while (this.allTypes.Any<T>())
                    {
                        int num2 = this.allTypes.Count<T>();
                        if (this.chunkSize > num2)
                        {
                            this.chunkSize = num2;
                        }
                        this.<>2__current = this.allTypes.Take<T>(this.chunkSize);
                        this.<>1__state = 1;
                        return true;
                    Label_0055:
                        this.<>1__state = -1;
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
                AddFillTypeMapsMethodBody.<GroupItemsInChunks>d__20<T> d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (AddFillTypeMapsMethodBody.<GroupItemsInChunks>d__20<T>) this;
                }
                else
                {
                    d__ = new AddFillTypeMapsMethodBody.<GroupItemsInChunks>d__20<T>(0);
                }
                d__.allTypes = this.<>3__allTypes;
                d__.chunkSize = this.<>3__chunkSize;
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
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}


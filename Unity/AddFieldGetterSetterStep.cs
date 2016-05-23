namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    internal sealed class AddFieldGetterSetterStep : ModuleStep
    {
        private TypeReference booleanType;
        private static readonly string[] colorMembers = new string[] { "r", "g", "b", "a" };
        private MethodReference gcHandleToObjectMethod;
        private TypeReference int32Type;
        private TypeReference int64Type;
        private MethodReference objectToGCHandleMethod;
        private TypeReference objectType;
        private static readonly string[] quaternionMembers = new string[] { "x", "y", "z", "w" };
        private TypeReference singleType;
        private static readonly string[] vector2Members = new string[] { "x", "y" };
        private static readonly string[] vector3Members = new string[] { "x", "y", "z" };
        private static readonly string[] vector4Members = new string[] { "x", "y", "z", "w" };
        private TypeReference voidType;

        private Collection<FieldDefinition>.Enumerator wrapEnum;
        private MethodDefinition AddGetter(TypeWrapper typeWrapper, FieldWrapper fieldWrapper)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Get{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.GetType(fieldWrapper));
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            item.Parameters.Add(definition2);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            if (fieldWrapper.StructSequence != null)
            {
                this.InjectFieldGetCode(typeWrapper.Type.Module, fieldWrapper.Type, iLProcessor, fieldWrapper, 0);
            }
            else
            {
                this.EmitSimpleFieldLoadCode(typeWrapper.Type.Module, iLProcessor, fieldWrapper.Field);
            }
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition AddGetterObject(TypeWrapper typeWrapper, FieldWrapper fieldWrapper)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Get{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.int64Type);
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            item.Parameters.Add(definition2);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            iLProcessor.Emit(OpCodes.Ldfld, base.ModuleContext.Import(fieldWrapper.Field));
            iLProcessor.Emit(OpCodes.Call, this.objectToGCHandleMethod);
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition AddGetterStruct(TypeWrapper typeWrapper, FieldWrapper fieldWrapper, string[] members)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Get{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.singleType);
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition3 = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            if (fieldWrapper.StructSequence != null)
            {
                this.InjectFieldGetCodeStructMember(typeWrapper.Type.Module, iLProcessor, body, fieldWrapper, definition3, 0);
                return item;
            }
            this.EmitKnownStructFieldReturn(typeWrapper.Type.Module, iLProcessor, body, fieldWrapper.Field, definition3);
            return item;
        }

        private MethodBody AddMethodToStruct(StructType structType, string fieldName, TypeReference returnType, ParameterDefinition[] parameters)
        {
            Dictionary<string, MethodDefinition> privateSetters = null;
            string str;
            if (returnType == this.voidType)
            {
                if (structType.PrivateSetters == null)
                {
                    structType.PrivateSetters = new Dictionary<string, MethodDefinition>();
                }
                privateSetters = structType.PrivateSetters;
                str = "$Set";
            }
            else
            {
                if (structType.PrivateGetters == null)
                {
                    structType.PrivateGetters = new Dictionary<string, MethodDefinition>();
                }
                privateSetters = structType.PrivateGetters;
                str = "$Get";
            }
            MethodDefinition item = new MethodDefinition(str + privateSetters.Count, MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig, returnType);
            ((TypeDefinition) structType.Type.Type).Methods.Add(item);
            if (parameters != null)
            {
                foreach (ParameterDefinition definition2 in parameters)
                {
                    item.Parameters.Add(definition2);
                }
            }
            privateSetters.Add(fieldName, item);
            return item.Body;
        }

        private MethodDefinition AddSetter(TypeWrapper typeWrapper, FieldWrapper fieldWrapper)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Set{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.voidType);
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition3 = new ParameterDefinition("value", ParameterAttributes.None, this.GetType(fieldWrapper));
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            if (fieldWrapper.StructSequence != null)
            {
                this.InjectFieldSetCode(typeWrapper.Type.Module, iLProcessor, fieldWrapper, definition3, 0);
            }
            else
            {
                this.EmitSimpleFieldSetCode(typeWrapper.Type.Module, iLProcessor, fieldWrapper.Field, definition3);
            }
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition AddSetterObject(TypeWrapper typeWrapper, FieldWrapper fieldWrapper)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Set{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.voidType);
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition3 = new ParameterDefinition("value", ParameterAttributes.None, this.int64Type);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            iLProcessor.EmitLdarg(definition3);
            iLProcessor.Emit(OpCodes.Call, this.gcHandleToObjectMethod);
            iLProcessor.Emit(OpCodes.Castclass, base.ModuleContext.Import(fieldWrapper.Type.Type.Resolve()));
            iLProcessor.Emit(OpCodes.Stfld, base.ModuleContext.Import(fieldWrapper.Field));
            iLProcessor.Emit(OpCodes.Ret);
            return item;
        }

        private MethodDefinition AddSetterStruct(TypeWrapper typeWrapper, FieldWrapper fieldWrapper, string[] members)
        {
            MethodDefinition item = new MethodDefinition(string.Format("$Set{0}", fieldWrapper.Id), MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static, this.voidType);
            ((TypeDefinition) typeWrapper.Type).Methods.Add(item);
            ParameterDefinition definition2 = new ParameterDefinition("instance", ParameterAttributes.None, this.objectType);
            ParameterDefinition definition3 = new ParameterDefinition("value", ParameterAttributes.None, this.singleType);
            ParameterDefinition definition4 = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            item.Parameters.Add(definition2);
            item.Parameters.Add(definition3);
            item.Parameters.Add(definition4);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(definition2);
            iLProcessor.Emit(OpCodes.Castclass, typeWrapper.Type);
            if (fieldWrapper.StructSequence != null)
            {
                this.InjectFieldSetCodeStructMember(typeWrapper.Type.Module, iLProcessor, body, fieldWrapper, definition4, definition3, 0);
                return item;
            }
            this.EmitKnownStructFieldSetReturn(typeWrapper.Type.Module, iLProcessor, body, fieldWrapper.Field, definition4, definition3);
            return item;
        }

        private void EmitKnownStructFieldReturn(ModuleDefinition destModule, ILProcessor ilProcessor, MethodBody body, FieldDefinition field, ParameterDefinition indexParameter)
        {
            int num;
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            ilProcessor.EmitLdarg(indexParameter);
            string[] members = GetKnownStructMembers(field);
            Instruction[] targets = new Instruction[members.Length];
            ilProcessor.Emit(OpCodes.Switch, targets);
            ilProcessor.Emit(OpCodes.Ldstr, indexParameter.Name);
            ilProcessor.Emit(OpCodes.Newobj, destModule.ImportReference(base.ModuleContext.GetCorLibMethod("System.ArgumentOutOfRangeException", InnerClass.<>9__28_0 ?? (InnerClass.<>9__28_0 = new Func<MethodDefinition, bool>(InnerClass.<>9.<EmitKnownStructFieldReturn>b__28_0)))));
            ilProcessor.Emit(OpCodes.Throw);
            Collection<FieldDefinition> fields = field.FieldType.Resolve().Fields;
            for (int i = 0; i < members.Length; i = num)
            {
                ilProcessor.Emit(OpCodes.Ldfld, destModule.ImportReference(base.ModuleContext.Import(fields.Single<FieldDefinition>(f => f.Name == members[i]))));
                targets[i] = body.Instructions.Last<Instruction>();
                ilProcessor.Emit(OpCodes.Ret);
                num = i + 1;
            }
        }

        private void EmitKnownStructFieldSetReturn(ModuleDefinition destModule, ILProcessor ilProcessor, MethodBody body, FieldDefinition field, ParameterDefinition indexParameter, ParameterDefinition valueParameter)
        {
            int num;
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            ilProcessor.EmitLdarg(valueParameter);
            ilProcessor.EmitLdarg(indexParameter);
            string[] members = GetKnownStructMembers(field);
            Instruction[] targets = new Instruction[members.Length];
            ilProcessor.Emit(OpCodes.Switch, targets);
            ilProcessor.Emit(OpCodes.Ldstr, indexParameter.Name);
            ilProcessor.Emit(OpCodes.Newobj, destModule.ImportReference(base.ModuleContext.GetCorLibMethod("System.ArgumentOutOfRangeException", <>c.<>9__29_0 ?? (<>c.<>9__29_0 = new Func<MethodDefinition, bool>(<>c.<>9.<EmitKnownStructFieldSetReturn>b__29_0)))));
            ilProcessor.Emit(OpCodes.Throw);
            Collection<FieldDefinition> fields = field.FieldType.Resolve().Fields;
            for (int i = 0; i < members.Length; i = num)
            {
                ilProcessor.Emit(OpCodes.Stfld, destModule.ImportReference(base.ModuleContext.Import(fields.Single<FieldDefinition>(f => f.Name == members[i]))));
                targets[i] = body.Instructions.Last<Instruction>();
                ilProcessor.Emit(OpCodes.Ret);
                num = i + 1;
            }
        }

        private void EmitPrivateFieldGetMethodCall(ModuleDefinition destModule, ILProcessor ilProcessor, FieldDefinition structField, FieldWrapper fieldWrapper, ParameterDefinition getterParam)
        {
            string str = this.MakeName(structField, fieldWrapper);
            StructType type = base.MetadataContainer.AddStruct(structField.FieldType);
            MethodReference method = base.OperationContext.Import(destModule, type.PrivateGetters[str]);
            if (getterParam != null)
            {
                ilProcessor.EmitLdarg(getterParam);
            }
            ilProcessor.Emit(OpCodes.Call, method);
            if (getterParam != null)
            {
                ilProcessor.Emit(OpCodes.Ret);
            }
        }

        private void EmitPrivateFieldSetMethodCall(ModuleDefinition destModule, ILProcessor ilProcessor, FieldDefinition structField, FieldWrapper fieldWrapper, ParameterDefinition valueParameter, ParameterDefinition indexParameter)
        {
            string str = this.MakeName(structField, fieldWrapper);
            StructType type = base.MetadataContainer.AddStruct(structField.FieldType);
            MethodReference method = base.OperationContext.Import(destModule, type.PrivateSetters[str]);
            ilProcessor.EmitLdarg(valueParameter);
            if (indexParameter != null)
            {
                ilProcessor.EmitLdarg(indexParameter);
            }
            ilProcessor.Emit(OpCodes.Call, method);
            if (indexParameter != null)
            {
                ilProcessor.Emit(OpCodes.Ret);
            }
        }

        private void EmitSimpleFieldLoadCode(ModuleDefinition destModule, ILProcessor ilProcessor, FieldDefinition fieldWrapper)
        {
            ilProcessor.Emit(OpCodes.Ldfld, base.OperationContext.Import(destModule, fieldWrapper));
        }

        private void EmitSimpleFieldSetCode(ModuleDefinition destModule, ILProcessor ilProcessor, FieldDefinition fieldWrapper, ParameterDefinition valueParameter)
        {
            ilProcessor.EmitLdarg(valueParameter);
            ilProcessor.Emit(OpCodes.Stfld, base.OperationContext.Import(destModule, fieldWrapper));
        }

        private TypeReference GetFieldTypeReference(FieldWrapper nestedStructField)
        {
            MetadataType metadataType = nestedStructField.Field.FieldType.MetadataType;
            if (metadataType != MetadataType.Boolean)
            {
                if (metadataType != MetadataType.Single)
                {
                    throw new ArgumentException("Unsupported type: " + nestedStructField.Type.FullName);
                }
                return this.singleType;
            }
            return this.booleanType;
        }

        private static string[] GetKnownStructMembers(FieldDefinition field)
        {
            string fullName = field.FieldType.FullName;
            if (!(fullName == "UnityEngine.Color"))
            {
                if (fullName == "UnityEngine.Quaternion")
                {
                    return quaternionMembers;
                }
                if (fullName == "UnityEngine.Vector2")
                {
                    return vector2Members;
                }
                if (fullName == "UnityEngine.Vector3")
                {
                    return vector3Members;
                }
                if (fullName != "UnityEngine.Vector4")
                {
                    throw new ArgumentException("Unknown struct type: " + field.FieldType.FullName);
                }
                return vector4Members;
            }
            return colorMembers;
        }

        [IteratorStateMachine(typeof(<GetNestedFieldsRecurse>d__39))]
        private IEnumerable<FieldWrapper> GetNestedFieldsRecurse(FieldDefinition nestedStruct, List<FieldDefinition> seq)
        {
            if (nestedStruct.FieldType is TypeDefinition)
            {
                using (this.wrapEnum = ((TypeDefinition) nestedStruct.FieldType).Fields.GetEnumerator())
                {
                    while (this.wrapEnum.MoveNext())
                    {
                        this.<field>5__1 = this.wrapEnum.Current;
                        if (((!this.<field>5__1.IsStatic && !this.<field>5__1.IsInitOnly) && !this.<field>5__1.IsNotSerialized) && (this.<field>5__1.IsPublic || AssemblyWrapper.HasSerializeFieldAttribute(this.<field>5__1)))
                        {
                            switch (this.<field>5__1.FieldType.MetadataType)
                            {
                                case MetadataType.Boolean:
                                case MetadataType.Single:
                                    FieldWrapper wrapper;
                                    wrapper = new FieldWrapper(0, this.<field>5__1, this.MetadataContainer.AddType(this.<field>5__1.FieldType)) {
                                        Name = string.Format("{0}.{1}", nestedStruct.Name, wrapper.Name),
                                        StructSequence = seq.ToArray()
                                    };
                                    yield return wrapper;
                                    break;

                                case MetadataType.ValueType:
                                    if (AssemblyWrapper.specialStructs.Contains<string>(this.<field>5__1.FieldType.FullName))
                                    {
                                        FieldWrapper wrapper2;
                                        wrapper2 = new FieldWrapper(0, this.<field>5__1, this.MetadataContainer.AddType(this.<field>5__1.FieldType)) {
                                            Name = string.Format("{0}.{1}", nestedStruct.Name, wrapper2.Name),
                                            StructSequence = seq.ToArray()
                                        };
                                        yield return wrapper2;
                                    }
                                    else
                                    {
                                        if (seq == null)
                                        {
                                            seq = new List<FieldDefinition>();
                                        }
                                        seq.Add(this.<field>5__1);
                                        using (this.<>7__wrap2 = this.GetNestedFieldsRecurse(this.<field>5__1, seq).GetEnumerator())
                                        {
                                            while (this.<>7__wrap2.MoveNext())
                                            {
                                                FieldWrapper current = this.<>7__wrap2.Current;
                                                current.Name = string.Format("{0}.{1}", this.<field>5__1.Name, current.Name);
                                                yield return current;
                                            }
                                        }
                                        this.<>7__wrap2 = null;
                                        seq.RemoveAt(seq.Count - 1);
                                    }
                                    break;
                            }
                            this.<field>5__1 = null;
                        }
                    }
                }
                this.wrapEnum = new Collection<FieldDefinition>.Enumerator();
            }
        }

        private TypeReference GetType(FieldWrapper fieldWrapper)
        {
            MetadataType metadataType = fieldWrapper.Type.Type.MetadataType;
            if (metadataType != MetadataType.Boolean)
            {
                if (metadataType != MetadataType.Single)
                {
                    throw new NotSupportedException(string.Format("Field type \"{0}\" is not supported.", fieldWrapper.Type.Type.FullName));
                }
                return this.singleType;
            }
            return this.booleanType;
        }

        private void InjectFieldGetCode(ModuleDefinition destModule, TypeWrapper type, ILProcessor ilProcessor, FieldWrapper fieldWrapper, int structIdx)
        {
            if (((fieldWrapper.StructSequence == null) || (structIdx < 0)) || (structIdx >= fieldWrapper.StructSequence.Length))
            {
                throw new ArgumentException("Invalid struct sequesnce parameters");
            }
            FieldDefinition field = fieldWrapper.StructSequence[structIdx];
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            if (structIdx == (fieldWrapper.StructSequence.Length - 1))
            {
                if (fieldWrapper.Field.IsPublic)
                {
                    this.EmitSimpleFieldLoadCode(destModule, ilProcessor, fieldWrapper.Field);
                }
                else
                {
                    FieldDefinition structField = fieldWrapper.StructSequence[structIdx];
                    StructType structType = base.MetadataContainer.AddStruct(structField.FieldType);
                    this.InjectPrivateAccessorsForStruct(structType);
                    this.EmitPrivateFieldGetMethodCall(destModule, ilProcessor, structField, fieldWrapper, null);
                }
            }
            else
            {
                FieldDefinition definition3 = fieldWrapper.StructSequence[structIdx + 1];
                if (definition3.IsPublic)
                {
                    StructType type3 = base.MetadataContainer.AddStruct(definition3.FieldType);
                    this.InjectFieldGetCode(destModule, type3.Type, ilProcessor, fieldWrapper, structIdx + 1);
                }
                else
                {
                    StructType type4 = base.MetadataContainer.AddStruct(field.FieldType);
                    this.InjectPrivateAccessorsForStruct(type4);
                    this.EmitPrivateFieldGetMethodCall(destModule, ilProcessor, field, fieldWrapper, null);
                }
            }
        }

        private void InjectFieldGetCodeStructMember(ModuleDefinition destModule, ILProcessor ilProcessor, MethodBody body, FieldWrapper fieldWrapper, ParameterDefinition indexParameter, int structIdx)
        {
            if (((fieldWrapper.StructSequence == null) || (structIdx < 0)) || (structIdx >= fieldWrapper.StructSequence.Length))
            {
                throw new ArgumentException("Invalid struct sequesnce parameters");
            }
            FieldDefinition field = fieldWrapper.StructSequence[structIdx];
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            if (structIdx == (fieldWrapper.StructSequence.Length - 1))
            {
                if (fieldWrapper.Field.IsPublic)
                {
                    this.EmitKnownStructFieldReturn(destModule, ilProcessor, body, fieldWrapper.Field, indexParameter);
                }
                else
                {
                    FieldDefinition structField = fieldWrapper.StructSequence[structIdx];
                    StructType structType = base.MetadataContainer.AddStruct(structField.FieldType);
                    this.InjectPrivateAccessorsForStruct(structType);
                    this.EmitPrivateFieldGetMethodCall(destModule, ilProcessor, structField, fieldWrapper, indexParameter);
                }
            }
            else
            {
                FieldDefinition definition3 = fieldWrapper.StructSequence[structIdx + 1];
                if (definition3.IsPublic)
                {
                    base.MetadataContainer.AddStruct(definition3.FieldType);
                    this.InjectFieldGetCodeStructMember(destModule, ilProcessor, body, fieldWrapper, indexParameter, structIdx + 1);
                }
                else
                {
                    StructType type2 = base.MetadataContainer.AddStruct(field.FieldType);
                    this.InjectPrivateAccessorsForStruct(type2);
                    this.EmitPrivateFieldGetMethodCall(destModule, ilProcessor, field, fieldWrapper, indexParameter);
                }
            }
        }

        private void InjectFieldSetCode(ModuleDefinition destModule, ILProcessor ilProcessor, FieldWrapper fieldWrapper, ParameterDefinition valueParameter, int structIdx)
        {
            if (((fieldWrapper.StructSequence == null) || (structIdx < 0)) || (structIdx >= fieldWrapper.StructSequence.Length))
            {
                throw new ArgumentException("Invalid struct sequesnce parameters");
            }
            FieldDefinition field = fieldWrapper.StructSequence[structIdx];
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            if (structIdx == (fieldWrapper.StructSequence.Length - 1))
            {
                if (fieldWrapper.Field.IsPublic)
                {
                    this.EmitSimpleFieldSetCode(destModule, ilProcessor, fieldWrapper.Field, valueParameter);
                }
                else
                {
                    FieldDefinition structField = fieldWrapper.StructSequence[structIdx];
                    StructType structType = base.MetadataContainer.AddStruct(structField.FieldType);
                    this.InjectPrivateAccessorsForStruct(structType);
                    this.EmitPrivateFieldSetMethodCall(destModule, ilProcessor, structField, fieldWrapper, valueParameter, null);
                }
            }
            else
            {
                FieldDefinition definition3 = fieldWrapper.StructSequence[structIdx + 1];
                if (definition3.IsPublic)
                {
                    base.MetadataContainer.AddStruct(definition3.FieldType);
                    this.InjectFieldSetCode(destModule, ilProcessor, fieldWrapper, valueParameter, structIdx + 1);
                }
                else
                {
                    StructType type2 = base.MetadataContainer.AddStruct(field.FieldType);
                    this.InjectPrivateAccessorsForStruct(type2);
                    this.EmitPrivateFieldSetMethodCall(destModule, ilProcessor, field, fieldWrapper, valueParameter, null);
                }
            }
        }

        private void InjectFieldSetCodeStructMember(ModuleDefinition destModule, ILProcessor ilProcessor, MethodBody body, FieldWrapper fieldWrapper, ParameterDefinition indexParameter, ParameterDefinition valueParameter, int structIdx)
        {
            if (((fieldWrapper.StructSequence == null) || (structIdx < 0)) || (structIdx >= fieldWrapper.StructSequence.Length))
            {
                throw new ArgumentException("Invalid struct sequesnce parameters");
            }
            FieldDefinition field = fieldWrapper.StructSequence[structIdx];
            ilProcessor.Emit(OpCodes.Ldflda, base.OperationContext.Import(destModule, field));
            if (structIdx == (fieldWrapper.StructSequence.Length - 1))
            {
                if (fieldWrapper.Field.IsPublic)
                {
                    this.EmitKnownStructFieldSetReturn(destModule, ilProcessor, body, fieldWrapper.Field, indexParameter, valueParameter);
                }
                else
                {
                    FieldDefinition structField = fieldWrapper.StructSequence[structIdx];
                    StructType structType = base.MetadataContainer.AddStruct(structField.FieldType);
                    this.InjectPrivateAccessorsForStruct(structType);
                    this.EmitPrivateFieldSetMethodCall(destModule, ilProcessor, structField, fieldWrapper, valueParameter, indexParameter);
                }
            }
            else
            {
                FieldDefinition definition3 = fieldWrapper.StructSequence[structIdx + 1];
                if (definition3.IsPublic)
                {
                    base.MetadataContainer.AddStruct(definition3.FieldType);
                    this.InjectFieldSetCodeStructMember(destModule, ilProcessor, body, fieldWrapper, indexParameter, valueParameter, structIdx + 1);
                }
                else
                {
                    StructType type2 = base.MetadataContainer.AddStruct(field.FieldType);
                    this.InjectPrivateAccessorsForStruct(type2);
                    this.EmitPrivateFieldSetMethodCall(destModule, ilProcessor, field, fieldWrapper, valueParameter, indexParameter);
                }
            }
        }

        private void InjectGetterForField(StructType structType, FieldDefinition field, TypeReference fieldType)
        {
            ILProcessor iLProcessor = this.AddMethodToStruct(structType, field.Name, fieldType, null).GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            this.EmitSimpleFieldLoadCode(structType.Type.Type.Module, iLProcessor, field);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void InjectGetterForKnownStructMembers(StructType structType, FieldDefinition field)
        {
            ParameterDefinition indexParameter = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition[] parameters = new ParameterDefinition[] { indexParameter };
            MethodBody body = this.AddMethodToStruct(structType, field.Name, this.singleType, parameters);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            this.EmitKnownStructFieldReturn(structType.Type.Type.Module, iLProcessor, body, field, indexParameter);
        }

        private void InjectGetterForNestedStructField(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            if (AssemblyWrapper.specialStructs.Contains<string>(nestedStructField.Field.FieldType.FullName))
            {
                this.InjectGetterForNestedStructKnownStructMembers(structType, nestedStruct, nestedStructField);
            }
            else
            {
                this.InjectGetterForNestedStructSimpleField(structType, nestedStruct, nestedStructField);
            }
        }

        private void InjectGetterForNestedStructKnownStructMembers(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            ParameterDefinition indexParameter = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition[] parameters = new ParameterDefinition[] { indexParameter };
            MethodBody body = this.AddMethodToStruct(structType, nestedStructField.Name, this.singleType, parameters);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            if (nestedStructField.StructSequence != null)
            {
                this.InjectFieldGetCodeStructMember(structType.Type.Type.Module, iLProcessor, body, nestedStructField, indexParameter, 0);
            }
            else
            {
                this.EmitKnownStructFieldReturn(structType.Type.Type.Module, iLProcessor, body, nestedStructField.Field, indexParameter);
            }
        }

        private void InjectGetterForNestedStructSimpleField(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            TypeReference fieldTypeReference = this.GetFieldTypeReference(nestedStructField);
            ILProcessor iLProcessor = this.AddMethodToStruct(structType, nestedStructField.Name, fieldTypeReference, null).GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            if (nestedStructField.StructSequence != null)
            {
                this.InjectFieldGetCode(structType.Type.Type.Module, nestedStructField.Type, iLProcessor, nestedStructField, 0);
            }
            else
            {
                this.EmitSimpleFieldLoadCode(structType.Type.Type.Module, iLProcessor, nestedStructField.Field);
            }
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void InjectPrivateAccessorsForStruct(StructType structType)
        {
            if (structType.PrivateGetters == null)
            {
                foreach (FieldDefinition definition in ((TypeDefinition) structType.Type.Type).Fields)
                {
                    if ((!definition.IsStatic && !definition.IsInitOnly) && (!definition.IsNotSerialized && !definition.IsPublic))
                    {
                        MetadataType metadataType = definition.FieldType.MetadataType;
                        if (metadataType != MetadataType.Boolean)
                        {
                            if (metadataType == MetadataType.Single)
                            {
                                goto Label_00A8;
                            }
                            if (metadataType == MetadataType.ValueType)
                            {
                                goto Label_00D4;
                            }
                        }
                        else if (AssemblyWrapper.HasSerializeFieldAttribute(definition))
                        {
                            this.InjectGetterForField(structType, definition, this.booleanType);
                            this.InjectSetterForField(structType, definition, this.booleanType);
                        }
                    }
                    continue;
                Label_00A8:
                    if (AssemblyWrapper.HasSerializeFieldAttribute(definition))
                    {
                        this.InjectGetterForField(structType, definition, this.singleType);
                        this.InjectSetterForField(structType, definition, this.singleType);
                    }
                    continue;
                Label_00D4:
                    if (!AssemblyWrapper.specialStructs.Contains<string>(definition.FieldType.FullName))
                    {
                        StructType type2 = base.MetadataContainer.AddStruct(definition.FieldType);
                        this.InjectPrivateAccessorsForStruct(type2);
                        List<FieldDefinition> seq = new List<FieldDefinition> {
                            definition
                        };
                        foreach (FieldWrapper wrapper in this.GetNestedFieldsRecurse(definition, seq))
                        {
                            this.InjectGetterForNestedStructField(structType, definition, wrapper);
                            this.InjectSetterForNestedStructField(structType, definition, wrapper);
                        }
                    }
                    else if (AssemblyWrapper.HasSerializeFieldAttribute(definition))
                    {
                        this.InjectGetterForKnownStructMembers(structType, definition);
                        this.InjectSetterForKnownStructMembers(structType, definition);
                    }
                }
            }
        }

        private void InjectSetterForField(StructType structType, FieldDefinition field, TypeReference fieldType)
        {
            ParameterDefinition valueParameter = new ParameterDefinition("value", ParameterAttributes.None, fieldType);
            ParameterDefinition[] parameters = new ParameterDefinition[] { valueParameter };
            ILProcessor iLProcessor = this.AddMethodToStruct(structType, field.Name, this.voidType, parameters).GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            this.EmitSimpleFieldSetCode(structType.Type.Type.Module, iLProcessor, field, valueParameter);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void InjectSetterForKnownStructMembers(StructType structType, FieldDefinition field)
        {
            ParameterDefinition valueParameter = new ParameterDefinition("value", ParameterAttributes.None, this.singleType);
            ParameterDefinition indexParameter = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition[] parameters = new ParameterDefinition[] { valueParameter, indexParameter };
            MethodBody body = this.AddMethodToStruct(structType, field.Name, this.voidType, parameters);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            this.EmitKnownStructFieldSetReturn(structType.Type.Type.Module, iLProcessor, body, field, indexParameter, valueParameter);
        }

        private void InjectSetterForNestedStructField(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            if (AssemblyWrapper.specialStructs.Contains<string>(nestedStructField.Field.FieldType.FullName))
            {
                this.InjectSetterForNestedStructKnownStructMembers(structType, nestedStruct, nestedStructField);
            }
            else
            {
                this.InjectSetterForNestedStructSimpleField(structType, nestedStruct, nestedStructField);
            }
        }

        private void InjectSetterForNestedStructKnownStructMembers(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            ParameterDefinition valueParameter = new ParameterDefinition("value", ParameterAttributes.None, this.singleType);
            ParameterDefinition indexParameter = new ParameterDefinition("index", ParameterAttributes.None, this.int32Type);
            ParameterDefinition[] parameters = new ParameterDefinition[] { valueParameter, indexParameter };
            MethodBody body = this.AddMethodToStruct(structType, nestedStructField.Name, this.voidType, parameters);
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            if (nestedStructField.StructSequence != null)
            {
                this.InjectFieldSetCodeStructMember(structType.Type.Type.Module, iLProcessor, body, nestedStructField, indexParameter, valueParameter, 0);
            }
            else
            {
                this.EmitKnownStructFieldSetReturn(structType.Type.Type.Module, iLProcessor, body, nestedStructField.Field, indexParameter, valueParameter);
            }
        }

        private void InjectSetterForNestedStructSimpleField(StructType structType, FieldDefinition nestedStruct, FieldWrapper nestedStructField)
        {
            TypeReference fieldTypeReference = this.GetFieldTypeReference(nestedStructField);
            ParameterDefinition valueParameter = new ParameterDefinition("value", ParameterAttributes.None, fieldTypeReference);
            ParameterDefinition[] parameters = new ParameterDefinition[] { valueParameter };
            ILProcessor iLProcessor = this.AddMethodToStruct(structType, nestedStructField.Name, this.voidType, parameters).GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldarg_0);
            if (nestedStructField.StructSequence != null)
            {
                this.InjectFieldSetCode(structType.Type.Type.Module, iLProcessor, nestedStructField, valueParameter, 0);
            }
            else
            {
                this.EmitSimpleFieldSetCode(structType.Type.Type.Module, iLProcessor, nestedStructField.Field, valueParameter);
            }
            iLProcessor.Emit(OpCodes.Ret);
        }

        private string MakeName(FieldDefinition structField, FieldWrapper fieldWrapper)
        {
            int index = 0;
            while (index < fieldWrapper.StructSequence.Count<FieldDefinition>())
            {
                if (fieldWrapper.StructSequence[index].Equals(structField))
                {
                    break;
                }
                index++;
            }
            string str = "";
            for (int i = index + 1; i < fieldWrapper.StructSequence.Count<FieldDefinition>(); i++)
            {
                str = str + fieldWrapper.StructSequence[i].Name + ".";
            }
            return (str + fieldWrapper.Field.Name);
        }

        private void Process(TypeWrapper typeWrapper)
        {
            if (typeWrapper.Fields != null)
            {
                foreach (FieldWrapper wrapper in typeWrapper.Fields)
                {
                    string fullName = wrapper.Type.Type.FullName;
                    if (!(fullName == "UnityEngine.Color"))
                    {
                        if (fullName == "UnityEngine.Quaternion")
                        {
                            goto Label_00A3;
                        }
                        if (fullName == "UnityEngine.Vector2")
                        {
                            goto Label_00CE;
                        }
                        if (fullName == "UnityEngine.Vector3")
                        {
                            goto Label_00F9;
                        }
                        if (fullName == "UnityEngine.Vector4")
                        {
                            goto Label_0121;
                        }
                        goto Label_0149;
                    }
                    wrapper.Getter = this.AddGetterStruct(typeWrapper, wrapper, colorMembers);
                    wrapper.Setter = this.AddSetterStruct(typeWrapper, wrapper, colorMembers);
                    continue;
                Label_00A3:
                    wrapper.Getter = this.AddGetterStruct(typeWrapper, wrapper, quaternionMembers);
                    wrapper.Setter = this.AddSetterStruct(typeWrapper, wrapper, quaternionMembers);
                    continue;
                Label_00CE:
                    wrapper.Getter = this.AddGetterStruct(typeWrapper, wrapper, vector2Members);
                    wrapper.Setter = this.AddSetterStruct(typeWrapper, wrapper, vector2Members);
                    continue;
                Label_00F9:
                    wrapper.Getter = this.AddGetterStruct(typeWrapper, wrapper, vector3Members);
                    wrapper.Setter = this.AddSetterStruct(typeWrapper, wrapper, vector3Members);
                    continue;
                Label_0121:
                    wrapper.Getter = this.AddGetterStruct(typeWrapper, wrapper, vector4Members);
                    wrapper.Setter = this.AddSetterStruct(typeWrapper, wrapper, vector4Members);
                    continue;
                Label_0149:
                    if (wrapper.Type.Type.MetadataType == MetadataType.Class)
                    {
                        wrapper.Getter = this.AddGetterObject(typeWrapper, wrapper);
                        wrapper.Setter = this.AddSetterObject(typeWrapper, wrapper);
                    }
                    else
                    {
                        wrapper.Getter = this.AddGetter(typeWrapper, wrapper);
                        wrapper.Setter = this.AddSetter(typeWrapper, wrapper);
                    }
                }
            }
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                this.booleanType = base.ModuleContext.GetCorLibType("System.Boolean");
                this.int32Type = base.ModuleContext.GetCorLibType("System.Int32");
                this.int64Type = base.ModuleContext.GetCorLibType("System.Int64");
                this.singleType = base.ModuleContext.GetCorLibType("System.Single");
                this.objectType = base.ModuleContext.GetCorLibType("System.Object");
                this.voidType = base.ModuleContext.GetCorLibType("System.Void");
                this.objectToGCHandleMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsObjectToGCHandleMethod);
                this.gcHandleToObjectMethod = base.ModuleContext.Import(base.OperationContext.GCHandledObjectsGCHandleToObjectMethod);
                foreach (TypeWrapper wrapper2 in assemblyWrapper.Types)
                {
                    this.Process(wrapper2);
                }
                this.booleanType = null;
                this.int32Type = null;
                this.singleType = null;
                this.objectType = null;
                this.voidType = null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class InnerClass
        {
            public static readonly AddFieldGetterSetterStep.InnerClass InnerInstance = new AddFieldGetterSetterStep.InnerClass();
            public static Func<MethodDefinition, bool> FuncA;
            public static Func<MethodDefinition, bool> FuncB;

            internal bool <EmitKnownStructFieldReturn>b__28_0(MethodDefinition m)
            {
                return (m.IsConstructor && (m.Parameters.Count == 1));
            }

            internal bool <EmitKnownStructFieldSetReturn>b__29_0(MethodDefinition m)
            {
                return (m.IsConstructor && (m.Parameters.Count == 1));
            }
        }

    }
}


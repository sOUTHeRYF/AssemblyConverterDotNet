namespace Unity
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("{Name}")]
    internal sealed class AssemblyWrapper
    {
        private readonly MetadataContainer metadataContainer;
        private readonly string name;
        private static readonly string[] nativeInteropTypes = new string[] { "Windows.Foundation.Collections.IIterator`", "Windows.Foundation.Collections.IIterable`", "Windows.Foundation.Collections.IKeyValuePair`", "Windows.Foundation.Collections.IMap`", "Windows.Foundation.Collections.IMapView`", "Windows.Foundation.Collections.IVector`", "Windows.Foundation.Collections.IVectorView`" };
        public static readonly string[] specialStructs = new string[] { "UnityEngine.Color", "UnityEngine.Quaternion", "UnityEngine.Vector2", "UnityEngine.Vector3", "UnityEngine.Vector4" };
        private Dictionary<TypeWrapper, StructType> structs = new Dictionary<TypeWrapper, StructType>();
        private readonly bool system;
        private TypeStorage types = new TypeStorage();

        public AssemblyWrapper(MetadataContainer metadataContainer, string name, bool system)
        {
            this.metadataContainer = metadataContainer;
            this.name = name;
            this.system = system;
        }

        private void AddFieldWrappersForField(List<FieldWrapper> wrappers, FieldDefinition field, List<FieldDefinition> structSeq, ref int id)
        {
            if ((field.FieldType.MetadataType == MetadataType.ValueType) && !specialStructs.Contains<string>(field.FieldType.FullName))
            {
                List<FieldDefinition> structFields = this.GetStructFields(field.FieldType.Resolve());
                if (structFields != null)
                {
                    if (structSeq == null)
                    {
                        structSeq = new List<FieldDefinition>();
                    }
                    structSeq.Add(field);
                    List<FieldWrapper> list2 = new List<FieldWrapper>();
                    foreach (FieldDefinition definition in structFields)
                    {
                        this.AddFieldWrappersForField(list2, definition, structSeq, ref id);
                    }
                    foreach (FieldWrapper wrapper in list2)
                    {
                        wrapper.Name = string.Format("{0}.{1}", field.Name, wrapper.Name);
                    }
                    structSeq.RemoveAt(structSeq.Count - 1);
                    wrappers.AddRange(list2);
                }
            }
            else
            {
                int num = id;
                id = num + 1;
                FieldWrapper item = new FieldWrapper(num, field, this.metadataContainer.AddType(field.FieldType));
                if ((structSeq != null) && (structSeq.Count > 0))
                {
                    item.StructSequence = structSeq.ToArray();
                }
                wrappers.Add(item);
            }
        }

        public StructType AddStruct(TypeReference type)
        {
            StructType type2;
            TypeWrapper key = this.AddType(type);
            if (!this.structs.TryGetValue(key, out type2))
            {
                type2 = new StructType(key);
                this.structs.Add(key, type2);
            }
            return type2;
        }

        public TypeWrapper AddType(TypeReference type)
        {
            TypeWrapper wrapper;
            if (!this.types.TryGetType(type.FullName, out wrapper))
            {
                Mono.Cecil.TokenType tokenType = type.MetadataToken.TokenType;
                if (tokenType != Mono.Cecil.TokenType.TypeRef)
                {
                    if (tokenType != Mono.Cecil.TokenType.TypeDef)
                    {
                        if (tokenType != Mono.Cecil.TokenType.TypeSpec)
                        {
                            throw new NotSupportedException(string.Format("Metadata token type {0} is not supported.", type.MetadataToken.TokenType));
                        }
                        MetadataType metadataType = type.MetadataType;
                        if (metadataType != MetadataType.Array)
                        {
                            if (metadataType != MetadataType.GenericInstance)
                            {
                                throw new NotSupportedException(string.Format("Metadata type {0} is not supported.", type.MetadataType));
                            }
                        }
                        else
                        {
                            ArrayType type4 = (ArrayType) type;
                            wrapper = new TypeWrapper(this, type4);
                            this.types.Add(type.FullName, wrapper);
                            wrapper.BaseType = this.metadataContainer.AddType(this.metadataContainer.OperationContext.ArrayType);
                            return wrapper;
                        }
                        GenericInstanceType type5 = (GenericInstanceType) type;
                        wrapper = new TypeWrapper(this, type5);
                        this.types.Add(type.FullName, wrapper);
                        TypeDefinition definition2 = type5.ElementType.Resolve();
                        TypeReference baseType = definition2.BaseType;
                        if (baseType != null)
                        {
                            if (baseType.IsGenericInstance)
                            {
                                GenericInstanceType type6 = (GenericInstanceType) baseType;
                                baseType = MetadataContainer.MakeGenericType(type5, type6);
                            }
                            wrapper.BaseType = this.metadataContainer.AddType(baseType);
                        }
                        if (definition2.HasInterfaces)
                        {
                            TypeWrapper[] wrapperArray2 = new TypeWrapper[definition2.Interfaces.Count];
                            for (int i = 0; i < wrapperArray2.Length; i++)
                            {
                                TypeReference reference2 = definition2.Interfaces[i];
                                if (reference2.IsGenericInstance)
                                {
                                    GenericInstanceType type7 = (GenericInstanceType) reference2;
                                    reference2 = MetadataContainer.MakeGenericType(type5, type7);
                                }
                                wrapperArray2[i] = this.metadataContainer.AddType(reference2);
                            }
                            wrapper.Interfaces = wrapperArray2;
                        }
                        wrapper.Methods = this.GetCallableMethods(wrapper, definition2, this.system);
                        return wrapper;
                    }
                }
                else
                {
                    return this.metadataContainer.GetAssembly(type.Resolve().Scope).AddType(type.Resolve());
                }
                TypeDefinition definition = (TypeDefinition) type;
                wrapper = new TypeWrapper(this, definition);
                this.types.Add(type.FullName, wrapper);
                if (definition.BaseType != null)
                {
                    wrapper.BaseType = this.metadataContainer.AddType(definition.BaseType);
                }
                if (definition.DeclaringType != null)
                {
                    wrapper.DeclaringType = this.metadataContainer.AddType(definition.DeclaringType);
                }
                if (definition.HasInterfaces)
                {
                    TypeWrapper[] wrapperArray = new TypeWrapper[definition.Interfaces.Count];
                    for (int j = 0; j < wrapperArray.Length; j++)
                    {
                        wrapperArray[j] = this.metadataContainer.AddType(definition.Interfaces[j]);
                    }
                    wrapper.Interfaces = wrapperArray;
                }
                wrapper.Methods = this.GetCallableMethods(wrapper, definition, this.system);
                if (!this.system)
                {
                    wrapper.Fields = this.GetFields(definition);
                }
            }
            return wrapper;
        }

        public static MethodWrapper CreateMethodWrapper(MetadataContainer metadata, TypeWrapper typeWrapper, MethodDefinition method)
        {
            MethodReference invokeTargetMethod = typeWrapper.Type.IsGenericInstance ? method.MakeGenericMethod(((GenericInstanceType) typeWrapper.Type)) : method;
            TypeWrapper returnType = metadata.AddType(method.ReturnType);
            TypeWrapper[] parameters = null;
            if (method.HasParameters)
            {
                parameters = new TypeWrapper[method.Parameters.Count];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = metadata.AddType(method.Parameters[i].ParameterType);
                }
            }
            return new MethodWrapper(method, invokeTargetMethod, typeWrapper, returnType, parameters);
        }

        private MethodWrapper[] GetCallableMethods(TypeWrapper typeWrapper, TypeDefinition type, bool publicOnly)
        {
            if (type.IsValueType)
            {
                return null;
            }
            if (type.IsNestedPrivate)
            {
                return null;
            }
            if (this.metadataContainer.IsDelegate(type) || this.metadataContainer.IsException(type))
            {
                return null;
            }
            List<MethodWrapper> list = new List<MethodWrapper>(type.Methods.Count);
            foreach (MethodDefinition definition in type.Methods)
            {
                if ((!publicOnly || definition.IsPublic) && IsCallableMethod(typeWrapper, definition))
                {
                    list.Add(CreateMethodWrapper(this.metadataContainer, typeWrapper, definition));
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            list.Sort(<>c.<>9__31_0 ?? (<>c.<>9__31_0 = new Comparison<MethodWrapper>(<>c.<>9.<GetCallableMethods>b__31_0)));
            return list.ToArray();
        }

        private FieldWrapper[] GetFields(TypeDefinition type)
        {
            if (type.Module.Assembly.Name.Name == "UnityEngine")
            {
                return null;
            }
            TypeDefinition baseType = this.metadataContainer.OperationContext.UnityEngineModuleContext.Module.GetType("UnityEngine.MonoBehaviour");
            if (!this.metadataContainer.IsDerivedFrom(type, baseType))
            {
                return null;
            }
            List<FieldWrapper> wrappers = new List<FieldWrapper>(type.Fields.Count);
            int id = 0;
            foreach (FieldDefinition definition2 in type.Fields)
            {
                if (((definition2.IsStatic || definition2.IsInitOnly) || definition2.IsNotSerialized) || (!definition2.IsPublic && !HasSerializeFieldAttribute(definition2)))
                {
                    continue;
                }
                MetadataType metadataType = definition2.FieldType.MetadataType;
                if (metadataType <= MetadataType.Single)
                {
                    switch (metadataType)
                    {
                        case MetadataType.Boolean:
                        case MetadataType.Single:
                            goto Label_0133;
                    }
                    continue;
                }
                if (metadataType != MetadataType.ValueType)
                {
                    if (metadataType == MetadataType.Class)
                    {
                        goto Label_011B;
                    }
                    continue;
                }
                if ((definition2.FieldType.GetAssemblyName() == "UnityEngine") && !specialStructs.Contains<string>(definition2.FieldType.FullName))
                {
                    continue;
                }
                goto Label_0133;
            Label_011B:
                if (!Utility.InheritsFrom(definition2.FieldType, "UnityEngine.Object", "UnityEngine"))
                {
                    continue;
                }
            Label_0133:
                this.AddFieldWrappersForField(wrappers, definition2, null, ref id);
            }
            if (wrappers.Count == 0)
            {
                return null;
            }
            return wrappers.ToArray();
        }

        private List<FieldDefinition> GetStructFields(TypeDefinition type)
        {
            if (type == null)
            {
                return null;
            }
            if (type.Module.Assembly.Name.Name == "UnityEngine")
            {
                return null;
            }
            List<FieldDefinition> list = new List<FieldDefinition>(type.Fields.Count);
            foreach (FieldDefinition definition in type.Fields)
            {
                if (((definition.IsStatic || definition.IsInitOnly) || definition.IsNotSerialized) || (!definition.IsPublic && !HasSerializeFieldAttribute(definition)))
                {
                    continue;
                }
                MetadataType metadataType = definition.FieldType.MetadataType;
                if (metadataType <= MetadataType.Single)
                {
                    switch (metadataType)
                    {
                        case MetadataType.Boolean:
                        case MetadataType.Single:
                            goto Label_00F2;
                    }
                    continue;
                }
                if (metadataType != MetadataType.ValueType)
                {
                    if (metadataType == MetadataType.Class)
                    {
                        goto Label_00DB;
                    }
                    continue;
                }
                if ((definition.FieldType.GetAssemblyName() == "UnityEngine") && !specialStructs.Contains<string>(definition.FieldType.FullName))
                {
                    continue;
                }
                goto Label_00F2;
            Label_00DB:
                if (!Utility.InheritsFrom(definition.FieldType, "UnityEngine.Object", "UnityEngine"))
                {
                    continue;
                }
            Label_00F2:
                list.Add(definition);
            }
            return list;
        }

        public static bool HasSerializeFieldAttribute(FieldDefinition field)
        {
            return field.CustomAttributes.Any<CustomAttribute>((<>c.<>9__38_0 ?? (<>c.<>9__38_0 = new Func<CustomAttribute, bool>(<>c.<>9.<HasSerializeFieldAttribute>b__38_0))));
        }

        private static bool IsCallableMethod(TypeWrapper typeWrapper, MethodDefinition method)
        {
            if (method.IsConstructor)
            {
                return false;
            }
            if (method.IsGenericInstance || method.HasGenericParameters)
            {
                return false;
            }
            if (!IsCallableParameter(method.ReturnType, true))
            {
                return false;
            }
            if (method.Parameters.Any<ParameterDefinition>(<>c.<>9__34_0 ?? (<>c.<>9__34_0 = new Func<ParameterDefinition, bool>(<>c.<>9.<IsCallableMethod>b__34_0))))
            {
                return false;
            }
            if (typeWrapper.Assembly.System)
            {
                if (nativeInteropTypes.Any<string>(t => method.DeclaringType.FullName.StartsWith(t)))
                {
                    return false;
                }
                if (method.HasOverrides && method.Overrides.Any<MethodReference>((<>c.<>9__34_2 ?? (<>c.<>9__34_2 = new Func<MethodReference, bool>(<>c.<>9.<IsCallableMethod>b__34_2)))))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsCallableParameter(TypeReference type, bool returnType)
        {
            switch (type.MetadataType)
            {
                case MetadataType.Void:
                    return returnType;

                case MetadataType.Boolean:
                case MetadataType.Byte:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.Single:
                case MetadataType.String:
                case MetadataType.ValueType:
                case MetadataType.Class:
                case MetadataType.Object:
                    return true;

                case MetadataType.Double:
                case MetadataType.IntPtr:
                    return !returnType;

                case MetadataType.Array:
                    return IsCallableParameter(((ArrayType) type).ElementType, returnType);

                case MetadataType.GenericInstance:
                {
                    GenericInstanceType type3 = (GenericInstanceType) type;
                    if (!type3.HasGenericArguments)
                    {
                        return false;
                    }
                    using (Collection<TypeReference>.Enumerator enumerator = type3.GenericArguments.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (!IsCallableParameter(enumerator.Current, false))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private TypeDefinition ResolveType(TypeReference typeRef)
        {
            try
            {
                return typeRef.Resolve();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public int FirstTypeId { get; set; }

        public MethodDefinition GetUnityTypeMethod { get; set; }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public int Offset { get; set; }

        public bool System
        {
            get
            {
                return this.system;
            }
        }

        public int SystemAssemblyId { get; set; }

        public TypeWrapper[] Types
        {
            get
            {
                return this.types.ToArray();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AssemblyWrapper.<>c <>9 = new AssemblyWrapper.<>c();
            public static Comparison<MethodWrapper> <>9__31_0;
            public static Func<ParameterDefinition, bool> <>9__34_0;
            public static Func<MethodReference, bool> <>9__34_2;
            public static Func<CustomAttribute, bool> <>9__38_0;

            internal int <GetCallableMethods>b__31_0(MethodWrapper left, MethodWrapper right)
            {
                int num = string.Compare(left.Name, right.Name, StringComparison.InvariantCulture);
                if (num != 0)
                {
                    return num;
                }
                if (left.TargetMethod.IsPublic ^ right.TargetMethod.IsPublic)
                {
                    if (!left.TargetMethod.IsPublic)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (left.TargetMethod.IsStatic ^ right.TargetMethod.IsStatic)
                {
                    if (!left.TargetMethod.IsPublic)
                    {
                        return -1;
                    }
                    return 1;
                }
                int x = (left.Parameters != null) ? left.Parameters.Length : 0;
                int y = (right.Parameters != null) ? right.Parameters.Length : 0;
                num = Comparer<int>.Default.Compare(x, y);
                if (num != 0)
                {
                    return num;
                }
                for (int i = 0; i < x; i++)
                {
                    num = string.Compare(left.Parameters[i].FullName, right.Parameters[i].FullName, StringComparison.InvariantCulture);
                    if (num != 0)
                    {
                        return num;
                    }
                }
                return 0;
            }

            internal bool <HasSerializeFieldAttribute>b__38_0(CustomAttribute a)
            {
                TypeReference declaringType = a.Constructor.DeclaringType;
                return ((declaringType.FullName == "UnityEngine.SerializeField") && (declaringType.GetAssemblyName() == "UnityEngine"));
            }

            internal bool <IsCallableMethod>b__34_0(ParameterDefinition p)
            {
                return !AssemblyWrapper.IsCallableParameter(p.ParameterType, false);
            }

            internal bool <IsCallableMethod>b__34_2(MethodReference m)
            {
                return AssemblyWrapper.nativeInteropTypes.Any<string>(t => m.DeclaringType.FullName.StartsWith(t));
            }
        }

        private class TypeStorage
        {
            private readonly Dictionary<string, TypeWrapper> typeDictionary = new Dictionary<string, TypeWrapper>();
            private readonly SortedSet<TypeWrapper> types = new SortedSet<TypeWrapper>();

            public void Add(string key, TypeWrapper typeWrapper)
            {
                this.typeDictionary.Add(key, typeWrapper);
                this.types.Add(typeWrapper);
            }

            public TypeWrapper[] ToArray()
            {
                return this.types.ToArray<TypeWrapper>();
            }

            public bool TryGetType(string key, out TypeWrapper typeWrapper)
            {
                return this.typeDictionary.TryGetValue(key, out typeWrapper);
            }
        }
    }
}


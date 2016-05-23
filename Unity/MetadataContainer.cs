namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class MetadataContainer
    {
        private readonly SortedList<string, AssemblyWrapper> assemblies = new SortedList<string, AssemblyWrapper>(StringComparer.Ordinal);
        private TypeReference delegateType;
        private TypeReference exceptionType;

        public MetadataContainer(Unity.OperationContext operationContext)
        {
            this.OperationContext = operationContext;
        }

        public StructType AddStruct(TypeReference type)
        {
            return this.GetAssemblyForType(type).AddStruct(type);
        }

        public TypeWrapper AddType(TypeReference type)
        {
            return this.GetAssemblyForType(type).AddType(type);
        }

        public AssemblyWrapper GetAssembly(IMetadataScope scope)
        {
            AssemblyWrapper wrapper;
            string assemblyName = scope.GetAssemblyName();
            string key = assemblyName + ".dll";
            if (!this.assemblies.TryGetValue(key, out wrapper))
            {
                bool system = this.OperationContext.IsSystemAssembly(assemblyName);
                wrapper = new AssemblyWrapper(this, key, system);
                this.assemblies.Add(wrapper.Name, wrapper);
            }
            return wrapper;
        }

        public AssemblyWrapper GetAssembly(string name)
        {
            AssemblyWrapper wrapper;
            if (!this.assemblies.TryGetValue(name + ".dll", out wrapper))
            {
                wrapper = null;
            }
            return wrapper;
        }

        private AssemblyWrapper GetAssemblyForType(TypeReference type)
        {
            if (type.IsArray)
            {
                TypeDefinition definition = ResolveType(((ArrayType) type).ElementType);
                return this.GetAssembly(definition.Scope);
            }
            if (type.IsGenericInstance)
            {
                TypeDefinition definition2 = ResolveType(((GenericInstanceType) type).ElementType);
                return this.GetAssembly(definition2.Scope);
            }
            return this.GetAssembly(ResolveType(type).Scope);
        }

        private static TypeReference InflateGenericType(GenericInstanceType genericInstanceProvider, TypeReference typeToInflate)
        {
            ArrayType type = typeToInflate as ArrayType;
            if (type != null)
            {
                TypeReference reference = InflateGenericType(genericInstanceProvider, type.ElementType);
                if (reference != type.ElementType)
                {
                    return new ArrayType(reference, type.Rank);
                }
                return type;
            }
            GenericInstanceType baseType = typeToInflate as GenericInstanceType;
            if (baseType != null)
            {
                return MakeGenericType(genericInstanceProvider, baseType);
            }
            GenericParameter genericParameter = typeToInflate as GenericParameter;
            if (genericParameter != null)
            {
                GenericParameter parameter = ResolveType(genericInstanceProvider.ElementType).GenericParameters.Single<GenericParameter>(p => p == genericParameter);
                return genericInstanceProvider.GenericArguments[parameter.Position];
            }
            FunctionPointerType type3 = typeToInflate as FunctionPointerType;
            if (type3 != null)
            {
                FunctionPointerType type9 = new FunctionPointerType {
                    ReturnType = InflateGenericType(genericInstanceProvider, type3.ReturnType)
                };
                for (int i = 0; i < type3.Parameters.Count; i++)
                {
                    TypeReference parameterType = InflateGenericType(genericInstanceProvider, type3.Parameters[i].ParameterType);
                    type9.Parameters.Add(new ParameterDefinition(parameterType));
                }
                return type9;
            }
            IModifierType type4 = typeToInflate as IModifierType;
            if (type4 != null)
            {
                TypeReference modifierType = InflateGenericType(genericInstanceProvider, type4.ModifierType);
                TypeReference reference4 = InflateGenericType(genericInstanceProvider, type4.ElementType);
                if (type4 is OptionalModifierType)
                {
                    return new OptionalModifierType(modifierType, reference4);
                }
                return new RequiredModifierType(modifierType, reference4);
            }
            PinnedType type5 = typeToInflate as PinnedType;
            if (type5 != null)
            {
                TypeReference reference5 = InflateGenericType(genericInstanceProvider, type5.ElementType);
                if (reference5 != type5.ElementType)
                {
                    return new PinnedType(reference5);
                }
                return type5;
            }
            PointerType type6 = typeToInflate as PointerType;
            if (type6 != null)
            {
                TypeReference reference6 = InflateGenericType(genericInstanceProvider, type6.ElementType);
                if (reference6 != type6.ElementType)
                {
                    return new PointerType(reference6);
                }
                return type6;
            }
            ByReferenceType type7 = typeToInflate as ByReferenceType;
            if (type7 != null)
            {
                TypeReference reference7 = InflateGenericType(genericInstanceProvider, type7.ElementType);
                if (reference7 != type7.ElementType)
                {
                    return new ByReferenceType(reference7);
                }
                return type7;
            }
            SentinelType type8 = typeToInflate as SentinelType;
            if (type8 == null)
            {
                return typeToInflate;
            }
            TypeReference reference8 = InflateGenericType(genericInstanceProvider, type8.ElementType);
            if (reference8 != type8.ElementType)
            {
                return new SentinelType(reference8);
            }
            return type8;
        }

        public bool IsDelegate(TypeReference type)
        {
            return this.IsDerivedFrom(type, this.DelegateType);
        }

        public bool IsDerivedFrom(TypeReference type, params TypeReference[] baseTypes)
        {
            foreach (TypeReference reference in baseTypes)
            {
                if (this.IsDerivedFrom(type, reference))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsDerivedFrom(TypeReference type, TypeReference baseType)
        {
            if (baseType.MetadataType == MetadataType.Array)
            {
                throw new NotImplementedException();
            }
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
                        return false;
                    }
                    GenericInstanceType type4 = (GenericInstanceType) type;
                    return this.IsDerivedFrom(type4.ElementType, baseType);
                }
            }
            else
            {
                return this.IsDerivedFrom(ResolveType(type), baseType);
            }
            TypeDefinition definition = (TypeDefinition) type;
            if ((definition.FullName == baseType.FullName) && (definition.Scope.GetAssemblyName() == ResolveType(baseType).Scope.GetAssemblyName()))
            {
                return true;
            }
            if (definition.BaseType == null)
            {
                return false;
            }
            return this.IsDerivedFrom(definition.BaseType, baseType);
        }

        public bool IsException(TypeReference type)
        {
            return this.IsDerivedFrom(type, this.ExceptionType);
        }

        public static GenericInstanceType MakeGenericType(GenericInstanceType type, GenericInstanceType baseType)
        {
            TypeReference[] referenceArray = new TypeReference[baseType.GenericArguments.Count];
            for (int i = 0; i < referenceArray.Length; i++)
            {
                referenceArray[i] = InflateGenericType(type, baseType.GenericArguments[i]);
            }
            GenericInstanceType type2 = new GenericInstanceType(baseType.ElementType);
            foreach (TypeReference reference in referenceArray)
            {
                TypeReference reference2;
                if ((reference.MetadataToken.TokenType == Mono.Cecil.TokenType.TypeRef) && (type2.GetAssemblyName() == reference.GetAssemblyName()))
                {
                    reference2 = ResolveType(reference);
                }
                else
                {
                    reference2 = type2.Module.ImportReference(reference);
                }
                type2.GenericArguments.Add(reference2);
            }
            return type2;
        }

        public static TypeDefinition ResolveType(TypeReference type)
        {
            TypeDefinition definition1 = type.Resolve();
            if (definition1 != null)
            {
                return definition1;
            }
            if (type.FullName.StartsWith("System."))
            {
                throw new Exception(string.Format("Failed to resolve type {0}. Is type unavailable in target framework?", type.FullName));
            }
            throw new Exception(string.Format("Failed to resolve type {0}. Missing assembly reference?", type.FullName));
        }

        public AssemblyWrapper[] Assemblies
        {
            get
            {
                return this.assemblies.Values.ToArray<AssemblyWrapper>();
            }
        }

        private TypeReference DelegateType
        {
            get
            {
                if (this.delegateType == null)
                {
                    ModuleDefinition corLib = this.OperationContext.CorLib;
                    this.delegateType = new TypeReference("System", "Delegate", corLib, corLib, false);
                }
                return this.delegateType;
            }
        }

        private TypeReference ExceptionType
        {
            get
            {
                if (this.exceptionType == null)
                {
                    ModuleDefinition corLib = this.OperationContext.CorLib;
                    this.exceptionType = new TypeReference("System", "Exception", corLib, corLib, false);
                }
                return this.exceptionType;
            }
        }

        public Unity.OperationContext OperationContext { get; private set; }
    }
}


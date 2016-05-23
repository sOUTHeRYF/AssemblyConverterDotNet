namespace Unity
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class ModuleContext
    {
        private TypeReference int32Type;
        private TypeReference intPtrType;
        private TypeReference objectType;
        private readonly Unity.OperationContext operationContext;
        private TypeReference stringType;

        public ModuleContext(Unity.OperationContext operationContext, ModuleDefinition module)
        {
            this.operationContext = operationContext;
            this.Module = module;
        }

        public AssemblyWrapper GetAssemblyWrapper()
        {
            return this.operationContext.MetadataContainer.GetAssembly(this.Module.Assembly.Name.Name);
        }

        public FieldReference GetCorLibField(TypeReference type, Func<FieldDefinition, bool> predicate)
        {
            FieldDefinition field = type.Resolve().Fields.Single<FieldDefinition>(predicate);
            return this.Import(field);
        }

        public FieldReference GetCorLibField(TypeReference type, string name)
        {
            return this.GetCorLibField(type, (Func<FieldDefinition, bool>) (m => (m.Name == name)));
        }

        public FieldReference GetCorLibField(string typeFullName, Func<FieldDefinition, bool> predicate)
        {
            TypeReference corLibType = this.GetCorLibType(typeFullName);
            return this.GetCorLibField(corLibType, predicate);
        }

        public MethodReference GetCorLibMethod(TypeReference type, Func<MethodDefinition, bool> predicate)
        {
            MethodDefinition method = type.Resolve().Methods.Single<MethodDefinition>(predicate);
            return this.Import(method);
        }

        public MethodReference GetCorLibMethod(TypeReference type, string name)
        {
            return this.GetCorLibMethod(type, (Func<MethodDefinition, bool>) (m => (m.Name == name)));
        }

        public MethodReference GetCorLibMethod(string typeFullName, Func<MethodDefinition, bool> predicate)
        {
            TypeReference corLibType = this.GetCorLibType(typeFullName);
            return this.GetCorLibMethod(corLibType, predicate);
        }

        public MethodReference GetCorLibMethod(string typeFullName, string methodName)
        {
            return this.GetCorLibMethod(typeFullName, (Func<MethodDefinition, bool>) (m => (m.Name == methodName)));
        }

        public TypeReference GetCorLibType(string fullName)
        {
            ModuleDefinition corLib = this.operationContext.CorLib;
            TypeDefinition type = corLib.GetType(fullName) ?? corLib.ExportedTypes.Single<ExportedType>(t => (t.FullName == fullName)).Resolve();
            return this.Import(type);
        }

        public FieldReference Import(FieldReference field)
        {
            return this.Retarget(field, new GenericContext());
        }

        public MethodReference Import(MethodReference method)
        {
            return this.Retarget(method, new GenericContext());
        }

        public TypeReference Import(TypeReference type)
        {
            return this.Retarget(type, new GenericContext());
        }

        public CustomAttributeArgument Retarget(CustomAttributeArgument argument, GenericContext context)
        {
            object obj2;
            TypeReference left = this.Retarget(argument.Type, context);
            if (left.IsArray)
            {
                CustomAttributeArgument[] argumentArray = (CustomAttributeArgument[]) argument.Value;
                if (argumentArray == null)
                {
                    obj2 = null;
                }
                else
                {
                    CustomAttributeArgument[] argumentArray2 = new CustomAttributeArgument[argumentArray.Length];
                    for (int i = 0; i < argumentArray.Length; i++)
                    {
                        argumentArray2[i] = this.Retarget(argumentArray[i], context);
                    }
                    obj2 = argumentArray2;
                }
            }
            else if (Utility.SameType(left, this.OperationContext.TypeType))
            {
                TypeReference type = (TypeReference) argument.Value;
                obj2 = this.Retarget(type, context);
            }
            else
            {
                obj2 = argument.Value;
            }
            return new CustomAttributeArgument(left, obj2);
        }

        public FieldReference Retarget(FieldReference field, GenericContext context)
        {
            FieldReference reference;
            if (field.IsDefinition)
            {
                reference = field;
            }
            else
            {
                TypeReference declaringType = this.Retarget(field.DeclaringType, context);
                context.PushType(declaringType.GetElementType());
                TypeReference fieldType = this.Retarget(field.FieldType, context);
                context.PopType();
                reference = new FieldReference(field.Name, fieldType, declaringType);
            }
            return this.Module.ImportReference(reference);
        }

        public MethodReference Retarget(MethodReference method, GenericContext context)
        {
            MethodReference reference;
            if (method.IsDefinition && Utility.SameScope(method.DeclaringType.Scope, this.Module))
            {
                reference = method;
            }
            else if (method.IsGenericInstance)
            {
                GenericInstanceMethod method2 = (GenericInstanceMethod) method;
                GenericInstanceMethod method3 = new GenericInstanceMethod(this.Retarget(method2.ElementMethod, context));
                foreach (TypeReference reference2 in method2.GenericArguments)
                {
                    TypeReference item = this.Retarget(reference2, context);
                    method3.GenericArguments.Add(item);
                }
                reference = method3;
            }
            else
            {
                TypeReference declaringType = this.Retarget(method.DeclaringType, context);
                context.PushType(declaringType.GetElementType());
                MethodReference owner = new MethodReference(method.Name, method.ReturnType, declaringType) {
                    CallingConvention = method.CallingConvention,
                    ExplicitThis = method.ExplicitThis,
                    HasThis = method.HasThis
                };
                using (Collection<GenericParameter>.Enumerator enumerator2 = method.GenericParameters.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        GenericParameter parameter = new GenericParameter(enumerator2.Current.Name, owner);
                        owner.GenericParameters.Add(parameter);
                    }
                }
                context.PushMethod(owner);
                owner.ReturnType = this.Retarget(method.ReturnType, context);
                foreach (ParameterDefinition definition in method.Parameters)
                {
                    TypeReference parameterType = this.Retarget(definition.ParameterType, context);
                    ParameterDefinition definition2 = new ParameterDefinition(definition.Name, definition.Attributes, parameterType);
                    owner.Parameters.Add(definition2);
                }
                reference = owner;
                context.PopMethod();
                context.PopType();
            }
            return this.Module.ImportReference(reference);
        }

        public TypeReference Retarget(TypeReference type, GenericContext context)
        {
            TypeReference reference;
            if (type == null)
            {
                return type;
            }
            if ((type.IsFunctionPointer || type.IsOptionalModifier) || type.IsSentinel)
            {
                throw new NotImplementedException();
            }
            if (type.IsArray)
            {
                ArrayType type2 = (ArrayType) type;
                ArrayType type3 = new ArrayType(this.Retarget(type2.ElementType, context), type2.Rank);
                for (int i = 0; i < type2.Dimensions.Count; i++)
                {
                    ArrayDimension dimension = type2.Dimensions[i];
                    type3.Dimensions[i] = new ArrayDimension(dimension.LowerBound, dimension.UpperBound);
                }
                reference = type3;
            }
            else if (type.IsByReference)
            {
                ByReferenceType type4 = (ByReferenceType) type;
                reference = new ByReferenceType(this.Retarget(type4.ElementType, context));
            }
            else if (type.IsDefinition && Utility.SameScope(type.Scope, this.Module))
            {
                reference = type;
            }
            else if (type.IsGenericInstance)
            {
                GenericInstanceType type5 = (GenericInstanceType) type;
                GenericInstanceType type6 = new GenericInstanceType(this.Retarget(type5.ElementType, context));
                foreach (TypeReference reference2 in type5.GenericArguments)
                {
                    TypeReference item = this.Retarget(reference2, context);
                    type6.GenericArguments.Add(item);
                }
                reference = type6;
            }
            else
            {
                if (type.IsGenericParameter)
                {
                    GenericParameter parameter = (GenericParameter) type;
                    return context.Retarget(parameter);
                }
                if (type.IsPinned)
                {
                    PinnedType type7 = (PinnedType) type;
                    reference = new PinnedType(this.Retarget(type7.ElementType, context));
                }
                else if (type.IsPointer)
                {
                    PointerType type8 = (PointerType) type;
                    reference = new PointerType(this.Retarget(type8.ElementType, context));
                }
                else if (type.IsRequiredModifier)
                {
                    RequiredModifierType type9 = (RequiredModifierType) type;
                    reference = new RequiredModifierType(this.Retarget(type9.ModifierType, context), this.Retarget(type9.ElementType, context));
                }
                else
                {
                    reference = type.Resolve();
                    if ((reference == null) && (this.OperationContext.Platform == Platform.UAP))
                    {
                        string fullName = null;
                        string str2 = type.FullName;
                        if ((!(str2 == "System.Collections.ArrayList") && !(str2 == "System.Collections.CollectionBase")) && (!(str2 == "System.Collections.Hashtable") && !(str2 == "System.Collections.Stack")))
                        {
                            if (str2 == "System.Reflection.BindingFlags")
                            {
                                fullName = "System.Reflection.TypeExtensions";
                            }
                        }
                        else
                        {
                            fullName = "System.Collections.NonGeneric";
                        }
                        if (fullName != null)
                        {
                            IMetadataScope scope = type.Scope;
                            type.Scope = AssemblyNameReference.Parse(fullName);
                            reference = type.Resolve();
                            type.Scope = scope;
                        }
                    }
                }
            }
            return this.Module.ImportReference(reference);
        }

        public TypeReference Int32Type
        {
            get
            {
                if (this.int32Type == null)
                {
                    this.int32Type = this.Module.ImportReference(this.operationContext.Int32Type);
                }
                return this.int32Type;
            }
        }

        public TypeReference IntPtrType
        {
            get
            {
                if (this.intPtrType == null)
                {
                    this.intPtrType = this.Module.ImportReference(this.operationContext.IntPtrType);
                }
                return this.intPtrType;
            }
        }

        public bool IsUnityEngine
        {
            get
            {
                return (this.Module.Assembly.Name.Name == "UnityEngine");
            }
        }

        public ModuleDefinition Module { get; private set; }

        public TypeReference ObjectType
        {
            get
            {
                if (this.objectType == null)
                {
                    this.objectType = this.Module.ImportReference(this.operationContext.ObjectType);
                }
                return this.objectType;
            }
        }

        public Unity.OperationContext OperationContext
        {
            get
            {
                return this.operationContext;
            }
        }

        public TypeReference StringType
        {
            get
            {
                if (this.stringType == null)
                {
                    this.stringType = this.Module.ImportReference(this.operationContext.StringType);
                }
                return this.stringType;
            }
        }
    }
}


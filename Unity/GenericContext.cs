namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class GenericContext
    {
        private readonly Stack<MethodReference> methods = new Stack<MethodReference>();
        private readonly Stack<TypeReference> types = new Stack<TypeReference>();

        public void PopMethod()
        {
            this.methods.Pop();
        }

        public void PopType()
        {
            this.types.Pop();
        }

        public void PushMethod(MethodReference method)
        {
            this.methods.Push(method);
        }

        public void PushType(TypeReference type)
        {
            this.types.Push(type);
        }

        public GenericParameter Retarget(GenericParameter value)
        {
            IGenericParameterProvider provider;
            GenericParameterType type = value.Type;
            if (type != GenericParameterType.Type)
            {
                if (type != GenericParameterType.Method)
                {
                    throw new Exception(string.Format("Unknown GenericParameterType value {0}.", value.Type));
                }
            }
            else
            {
                provider = this.types.Peek();
                goto Label_00DC;
            }
            provider = this.methods.Peek();
            if (!provider.HasGenericParameters)
            {
                MethodReference reference = (MethodReference) provider;
                if (reference.DeclaringType.IsArray)
                {
                    ArrayType declaringType = (ArrayType) reference.DeclaringType;
                    if (declaringType.ElementType.IsGenericParameter)
                    {
                        GenericParameter elementType = (GenericParameter) declaringType.ElementType;
                        if (elementType.FullName.Equals(value.FullName))
                        {
                            return elementType;
                        }
                    }
                }
                throw new Exception(string.Format("Unknown generic parameter {0}.", value));
            }
        Label_00DC:
            if (provider.GenericParameters.Count == 0)
            {
                return value;
            }
            return provider.GenericParameters.Single<GenericParameter>(p => (p.Position == value.Position));
        }
    }
}


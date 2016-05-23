namespace Unity
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{FullName}")]
    internal sealed class TypeWrapper : IComparable<TypeWrapper>
    {
        public TypeWrapper(AssemblyWrapper assembly, TypeReference type)
        {
            this.Assembly = assembly;
            this.Type = type;
            this.Name = GetName(type);
            this.FullName = GetFullName(type);
            TypeDefinition definition = type.Resolve();
            TypeFlag none = TypeFlag.None;
            using (Collection<CustomAttribute>.Enumerator enumerator = definition.CustomAttributes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TypeReference attributeType = enumerator.Current.AttributeType;
                    if (!(attributeType.GetAssemblyName() != "UnityEngine") && (attributeType.FullName == "UnityEngine.SharedBetweenAnimatorsAttribute"))
                    {
                        none |= TypeFlag.HasSharedBetweenAnimatorsAttribute;
                    }
                }
            }
            if (definition.IsInterface)
            {
                none |= TypeFlag.Interface;
            }
            if (definition.IsAbstract)
            {
                none |= TypeFlag.Abstract;
            }
            if (type.IsGenericInstance)
            {
                none |= TypeFlag.GenericInstance;
            }
            if (type.HasGenericParameters)
            {
                none |= TypeFlag.GenericDefinition;
            }
            this.Flags = none;
            this.DefaultConstructor = definition.Methods.SingleOrDefault<MethodDefinition>(<>c.<>9__70_0 ?? (<>c.<>9__70_0 = new Func<MethodDefinition, bool>(<>c.<>9.<.ctor>b__70_0)));
            if (type.IsGenericInstance && (this.DefaultConstructor != null))
            {
                this.DefaultConstructor = this.DefaultConstructor.MakeGenericMethod(((GenericInstanceType) type).GenericArguments.ToArray());
            }
        }

        public int CompareTo(TypeWrapper other)
        {
            int num = string.CompareOrdinal(this.Namespace, other.Namespace);
            if (num == 0)
            {
                return string.CompareOrdinal(this.Name, other.Name);
            }
            return num;
        }

        private static string GetFullName(TypeReference type)
        {
            string str = type.Namespace;
            if (!string.IsNullOrEmpty(str))
            {
                str = str + ".";
            }
            return (str + GetName(type));
        }

        private static string GetName(TypeReference type)
        {
            string name = type.Name;
            if (type.IsNested)
            {
                name = GetName(type.DeclaringType) + "+" + name;
            }
            if (!type.IsGenericInstance)
            {
                return name;
            }
            name = name + "[";
            foreach (TypeReference reference in ((GenericInstanceType) type).GenericArguments)
            {
                if (name.Last<char>() != '[')
                {
                    name = name + ",";
                }
                name = name + GetFullName(reference);
            }
            return (name + "]");
        }

        public AssemblyWrapper Assembly { get; private set; }

        public TypeWrapper BaseType { get; set; }

        public int ClassId
        {
            get
            {
                return -1;
            }
        }

        public TypeWrapper DeclaringType { get; set; }

        public MethodReference DefaultConstructor { get; private set; }

        public FieldWrapper[] Fields { get; set; }

        public TypeFlag Flags { get; set; }

        public string FullName { get; private set; }

        public int Id { get; set; }

        public bool ImplementCreateInstanceMethod { get; set; }

        public TypeWrapper[] Interfaces { get; set; }

        public MethodWrapper[] Methods { get; set; }

        public string Name { get; private set; }

        public string Namespace
        {
            get
            {
                return this.Type.Namespace;
            }
        }

        public int Offset { get; set; }

        public MethodDefinition SpecialConstructor { get; set; }

        public TypeReference Type { get; private set; }

        public TypeDefinition UnityType { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TypeWrapper.<>c <>9 = new TypeWrapper.<>c();
            public static Func<MethodDefinition, bool> <>9__70_0;

            internal bool <.ctor>b__70_0(MethodDefinition m)
            {
                return ((m.IsConstructor && !m.IsStatic) && !m.HasParameters);
            }
        }
    }
}


namespace Unity
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}")]
    internal sealed class MethodWrapper
    {
        public MethodWrapper(MethodDefinition targetMethod, MethodReference invokeTargetMethod, TypeWrapper declaringType, TypeWrapper returnType, TypeWrapper[] parameters)
        {
            this.TargetMethod = targetMethod;
            this.InvokeTargetMethod = invokeTargetMethod;
            this.DeclaringType = declaringType;
            this.ReturnType = returnType;
            this.Parameters = parameters;
            MethodFlag none = MethodFlag.None;
            if (targetMethod.IsStatic)
            {
                none |= MethodFlag.Static;
            }
            using (Collection<CustomAttribute>.Enumerator enumerator = targetMethod.CustomAttributes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TypeReference attributeType = enumerator.Current.AttributeType;
                    if (attributeType.GetAssemblyName() == "UnityEngine")
                    {
                        if (attributeType.FullName == "UnityEngine.ImageEffectOpaque")
                        {
                            none |= MethodFlag.HasImageEffectOpaqueAttribute;
                        }
                        if (attributeType.FullName == "UnityEngine.ImageEffectTransformsToLDR")
                        {
                            none |= MethodFlag.HasImageEffectTransformsToLDRAttribute;
                        }
                    }
                }
            }
            this.Flags = none;
        }

        public TypeWrapper DeclaringType { get; private set; }

        public MethodFlag Flags { get; private set; }

        public MethodReference InvokeMethod { get; set; }

        public MethodReference InvokeTargetMethod { get; private set; }

        public string Name
        {
            get
            {
                return this.TargetMethod.Name;
            }
        }

        public int Offset { get; set; }

        public TypeWrapper[] Parameters { get; private set; }

        public TypeWrapper ReturnType { get; private set; }

        public MethodDefinition TargetMethod { get; private set; }
    }
}


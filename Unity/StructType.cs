namespace Unity
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Mono;
    using Mono.Cecil;
    internal class StructType
    {
        public StructType(TypeWrapper type)
        {
            this.Type = type;
        }

        public Dictionary<string, MethodDefinition> PrivateGetters { get; set; }

        public Dictionary<string, MethodDefinition> PrivateSetters { get; set; }

        public TypeWrapper Type { get; private set; }
    }
}


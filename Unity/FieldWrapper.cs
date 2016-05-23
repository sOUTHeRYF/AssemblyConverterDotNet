namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}")]
    internal sealed class FieldWrapper
    {
        public FieldWrapper(int id, FieldDefinition field, TypeWrapper type)
        {
            this.Id = id;
            this.Field = field;
            this.Type = type;
            this.Name = field.Name;
        }

        public FieldDefinition Field { get; private set; }

        public MethodDefinition Getter { get; set; }

        public int Id { get; private set; }

        public string Name { get; set; }

        public int Offset { get; set; }

        public MethodDefinition Setter { get; set; }

        public FieldDefinition[] StructSequence { get; set; }

        public TypeWrapper Type { get; private set; }
    }
}


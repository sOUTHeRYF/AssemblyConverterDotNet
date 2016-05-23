namespace Unity
{
    using Mono.Cecil;
    using System;

    public interface ITypeDefinitionVisitor
    {
        void Visit(TypeDefinition type);
    }
}


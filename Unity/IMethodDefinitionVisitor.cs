namespace Unity
{
    using Mono.Cecil;
    using System;

    public interface IMethodDefinitionVisitor
    {
        void Visit(MethodDefinition method);
    }
}


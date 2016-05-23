namespace Unity
{
    using Mono.Cecil;
    using System;

    public sealed class MethodDefinitionDispatcher : ITypeDefinitionVisitor
    {
        private readonly IMethodDefinitionVisitor visitor;

        private MethodDefinitionDispatcher(ModuleDefinition module, IMethodDefinitionVisitor visitor)
        {
            this.visitor = visitor;
            TypeDefinitionDispatcher.Dispatch(module, this);
        }

        public static void Dispatch(ModuleDefinition module, IMethodDefinitionVisitor visitor)
        {
            new MethodDefinitionDispatcher(module, visitor);
        }

        public void Visit(TypeDefinition type)
        {
            foreach (MethodDefinition definition in type.Methods)
            {
                this.visitor.Visit(definition);
            }
        }
    }
}


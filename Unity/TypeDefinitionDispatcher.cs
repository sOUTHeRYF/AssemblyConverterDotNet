namespace Unity
{
    using Mono.Cecil;
    using System;

    public sealed class TypeDefinitionDispatcher
    {
        private readonly ITypeDefinitionVisitor visitor;

        private TypeDefinitionDispatcher(ModuleDefinition module, ITypeDefinitionVisitor visitor)
        {
            this.visitor = visitor;
            foreach (TypeDefinition definition in module.Types)
            {
                this.DispatchType(definition);
            }
        }

        public static void Dispatch(ModuleDefinition module, ITypeDefinitionVisitor visitor)
        {
            new TypeDefinitionDispatcher(module, visitor);
        }

        private void DispatchType(TypeDefinition type)
        {
            this.visitor.Visit(type);
            foreach (TypeDefinition definition in type.NestedTypes)
            {
                this.DispatchType(definition);
            }
        }
    }
}


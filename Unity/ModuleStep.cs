namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal abstract class ModuleStep : Step
    {
        protected ModuleStep()
        {
        }

        protected TypeDefinition AddStaticClass(string @namespace, string name)
        {
            TypeDefinition item = new TypeDefinition(@namespace, name, TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed, this.ModuleContext.ObjectType);
            this.Module.Types.Add(item);
            return item;
        }

        protected override IStepContext Execute()
        {
            foreach (Unity.ModuleContext context in base.OperationContext.ModuleContexts)
            {
                base.OperationContext.AssemblyResolver.CurrentDirectory = Path.GetDirectoryName(context.Module.FullyQualifiedName);
                this.ModuleContext = context;
                this.ProcessModule();
                this.ModuleContext = null;
                base.OperationContext.AssemblyResolver.CurrentDirectory = null;
            }
            return null;
        }

        protected abstract void ProcessModule();

        public bool IsUnityEngine
        {
            get
            {
                return this.ModuleContext.IsUnityEngine;
            }
        }

        public ModuleDefinition Module
        {
            get
            {
                return this.ModuleContext.Module;
            }
        }

        public Unity.ModuleContext ModuleContext { get; private set; }
    }
}


namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Pdb;
    using System;

    internal sealed class SaveModulesStep : ModuleStep
    {
        protected override void ProcessModule()
        {
            WriterParameters parameters = new WriterParameters();
            if (base.Module.SymbolReader != null)
            {
                parameters.SymbolWriterProvider = new PdbWriterProvider();
            }
            base.Module.Write(base.Module.FullyQualifiedName, parameters);
        }
    }
}


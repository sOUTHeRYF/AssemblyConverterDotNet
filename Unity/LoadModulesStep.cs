namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Mdb;
    using Mono.Cecil.Pdb;
    using System;
    using System.IO;

    internal sealed class LoadModulesStep : Step
    {
        protected override IStepContext Execute()
        {
            foreach (string str in ((ParseArgsStep.StepContext) base.PreviousStepContext).FileNames)
            {
                ModuleDefinition definition;
                ReaderParameters parameters = new ReaderParameters {
                    AssemblyResolver = base.OperationContext.AssemblyResolver
                };
                if (File.Exists(Path.ChangeExtension(str, ".pdb")))
                {
                    parameters.SymbolReaderProvider = new PdbReaderProvider();
                }
                else if (File.Exists(str + ".mdb"))
                {
                    parameters.SymbolReaderProvider = new MdbReaderProvider();
                }
                try
                {
                    definition = ModuleDefinition.ReadModule(str, parameters);
                }
                catch (InvalidOperationException)
                {
                    parameters.SymbolReaderProvider = null;
                    definition = ModuleDefinition.ReadModule(str, parameters);
                }
                base.OperationContext.AssemblyResolver.AddAssembly(definition.Assembly);
                if (Path.GetExtension(str).ToLower() != ".winmd")
                {
                    base.OperationContext.AddModule(definition);
                }
            }
            return null;
        }
    }
}


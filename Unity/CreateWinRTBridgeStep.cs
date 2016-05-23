namespace Unity
{
    using Mono.Cecil;
    using System;

    internal sealed class CreateWinRTBridgeStep : Step
    {
        protected override IStepContext Execute()
        {
            AssemblyDefinition assembly = base.OperationContext.AssemblyResolver.Resolve(new AssemblyNameReference("WinRTBridge", new Version(1, 0, 0, 0)));
            base.OperationContext.SetWinRTBridge(assembly);
            return null;
        }
    }
}


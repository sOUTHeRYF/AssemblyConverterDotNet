namespace Unity
{
    using System;

    internal sealed class GenerateNewMVIDsStep : ModuleStep
    {
        protected override void ProcessModule()
        {
            base.Module.Mvid = Guid.NewGuid();
        }
    }
}


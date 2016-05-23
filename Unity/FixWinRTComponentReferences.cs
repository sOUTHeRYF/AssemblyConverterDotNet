namespace Unity
{
    using System;

    internal sealed class FixWinRTComponentReferences : ModuleStep
    {
        protected override void ProcessModule()
        {
            for (int i = 0; i < base.Module.AssemblyReferences.Count; i++)
            {
                if (base.Module.AssemblyResolver.Resolve(base.Module.AssemblyReferences[i]).Name.IsWindowsRuntime)
                {
                    base.Module.AssemblyReferences[i].IsWindowsRuntime = true;
                }
            }
        }
    }
}


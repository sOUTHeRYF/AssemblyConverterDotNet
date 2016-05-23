namespace Unity
{
    using System;

    internal sealed class SetTypeIdsStep : Step
    {
        protected override IStepContext Execute()
        {
            int num = 0;
            int num2 = 1;
            foreach (AssemblyWrapper wrapper in base.MetadataContainer.Assemblies)
            {
                if (wrapper.System)
                {
                    wrapper.SystemAssemblyId = num++;
                }
                wrapper.FirstTypeId = num2;
                TypeWrapper[] types = wrapper.Types;
                for (int i = 0; i < types.Length; i++)
                {
                    types[i].Id = num2++;
                }
            }
            return null;
        }
    }
}


namespace Unity
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class RemoveDebuggableAttributeStep : ModuleStep
    {
        protected override void ProcessModule()
        {
            if (base.OperationContext.RemoveDebuggableAttribute)
            {
                Collection<CustomAttribute> customAttributes = base.Module.Assembly.CustomAttributes;
                for (int i = 0; i < customAttributes.Count; i++)
                {
                    if (customAttributes[i].AttributeType.FullName == "System.Diagnostics.DebuggableAttribute")
                    {
                        customAttributes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}


namespace Unity
{
    using Mono.Cecil;
    using System;

    internal sealed class SaveMetadataStep : ModuleStep
    {
        protected override void ProcessModule()
        {
            if (base.Module.Assembly.Name.Name == "UnityEngine")
            {
                GenerateMetadataStep.StepContext previousStepContext = (GenerateMetadataStep.StepContext) base.PreviousStepContext;
                EmbeddedResource item = new EmbeddedResource("UnityMetadata", ManifestResourceAttributes.Private, previousStepContext.MetadataBuffer.Data);
                base.Module.Resources.Add(item);
            }
        }
    }
}


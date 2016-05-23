namespace Unity
{
    using System;

    internal sealed class GenerateMetadataStep : Step
    {
        protected override IStepContext Execute()
        {
            MetadataBuffer metadataBuffer = new MetadataBuffer();
            metadataBuffer.Initialize(base.OperationContext.MetadataContainer.Assemblies, base.OperationContext.Is64);
            return new StepContext(metadataBuffer);
        }

        public sealed class StepContext : IStepContext
        {
            private readonly Unity.MetadataBuffer metadataBuffer;

            public StepContext(Unity.MetadataBuffer metadataBuffer)
            {
                this.metadataBuffer = metadataBuffer;
            }

            public Unity.MetadataBuffer MetadataBuffer
            {
                get
                {
                    return this.metadataBuffer;
                }
            }
        }
    }
}


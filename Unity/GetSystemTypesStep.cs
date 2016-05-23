namespace Unity
{
    using Mono.Cecil;

    internal sealed class GetSystemTypesStep : Step
    {
        protected override IStepContext Execute()
        {
            TypeDefinition corLibType = base.OperationContext.GetCorLibType("System.Single");
            MetadataContainer metadataContainer = base.OperationContext.MetadataContainer;
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Boolean"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Byte"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Char"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Collections.IEnumerator"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Double"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Enum"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.IDisposable"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Int16"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Int32"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Int64"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.IntPtr"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.MulticastDelegate"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.Object"));
            metadataContainer.AddType(corLibType);
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.String"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.UInt16"));
            metadataContainer.AddType(base.OperationContext.GetCorLibType("System.UInt32"));
            metadataContainer.AddType(new ArrayType(corLibType, 1));
            return null;
        }
    }
}


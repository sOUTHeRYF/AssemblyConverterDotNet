namespace Unity
{
    using Mono.Cecil;
    using System;

    internal sealed class AddSpecialConstructorStep : ModuleStep
    {
        private TypeReference attributeType;
        private TypeReference delegateType;
        private TypeReference exceptionType;
        private TypeReference weakReferenceType;

        private MethodDefinition CreateSpecialConstructor(bool isSealed)
        {
            return new MethodDefinition(".ctor", (MethodAttributes.CompilerControlled | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName) | (isSealed ? MethodAttributes.Assembly : (MethodAttributes.CompilerControlled | MethodAttributes.Family | MethodAttributes.Private)), base.ModuleContext.GetCorLibType("System.Void")) { Parameters = { new ParameterDefinition("dummy", ParameterAttributes.None, base.ModuleContext.GetCorLibType("System.UIntPtr")) } };
        }

        private void Process(TypeWrapper typeWrapper)
        {
            TypeDefinition type;
            if (typeWrapper.Type.IsDefinition)
            {
                type = (TypeDefinition) typeWrapper.Type;
            }
            else if (typeWrapper.Type.IsGenericInstance)
            {
                type = ((GenericInstanceType) typeWrapper.Type).ElementType.Resolve();
                if (base.OperationContext.IsSystemAssembly(type.Scope.GetAssemblyName()))
                {
                    return;
                }
            }
            else
            {
                return;
            }
            if (((type.IsClass && !type.IsEnum) && ((!type.IsNestedPrivate && !type.IsNestedFamily) && !type.IsNestedFamilyAndAssembly)) && (!type.IsAbstract || !type.IsSealed))
            {
                TypeReference[] baseTypes = new TypeReference[] { this.attributeType, this.exceptionType, this.delegateType, this.weakReferenceType };
                if (!base.MetadataContainer.IsDerivedFrom(type, baseTypes))
                {
                    typeWrapper.ImplementCreateInstanceMethod = !type.IsAbstract;
                    if (!type.IsValueType)
                    {
                        typeWrapper.SpecialConstructor = this.CreateSpecialConstructor(type.IsSealed);
                    }
                }
            }
        }

        protected override void ProcessModule()
        {
            AssemblyWrapper assemblyWrapper = base.ModuleContext.GetAssemblyWrapper();
            if (assemblyWrapper != null)
            {
                ModuleDefinition corLib = base.OperationContext.CorLib;
                this.attributeType = new TypeReference("System", "Attribute", corLib, corLib, false);
                this.exceptionType = new TypeReference("System", "Exception", corLib, corLib, false);
                this.delegateType = new TypeReference("System", "Delegate", corLib, corLib, false);
                this.weakReferenceType = new TypeReference("System", "WeakReference", corLib, corLib, false);
                foreach (TypeWrapper wrapper2 in assemblyWrapper.Types)
                {
                    this.Process(wrapper2);
                }
                this.attributeType = null;
                this.exceptionType = null;
                this.delegateType = null;
                this.weakReferenceType = null;
            }
        }
    }
}


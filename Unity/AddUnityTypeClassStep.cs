namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AddUnityTypeClassStep : ModuleStep
    {
        private TypeDefinition unityType;

        private void AddConstructor()
        {
            MethodDefinition item = new MethodDefinition(".ctor", MethodAttributes.CompilerControlled | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, base.ModuleContext.GetCorLibType("System.Void"));
            this.unityType.Methods.Add(item);
            MethodBody body = item.Body;
            ILProcessor iLProcessor = body.GetILProcessor();
            iLProcessor.EmitLdarg(body.ThisParameter);
            iLProcessor.Emit(OpCodes.Call, base.ModuleContext.GetCorLibMethod(this.unityType.BaseType, ".ctor"));
            iLProcessor.Emit(OpCodes.Ret);
        }

        private void AddCreateInstanceMethod()
        {
            MethodDefinition item = new MethodDefinition("CreateInstance", MethodAttributes.CompilerControlled | MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, base.ModuleContext.GetCorLibType("System.Object"));
            this.unityType.Methods.Add(item);
            ILProcessor iLProcessor = item.Body.GetILProcessor();
            iLProcessor.Emit(OpCodes.Ldnull);
            iLProcessor.Emit(OpCodes.Ret);
        }

        private TypeDefinition AddUnityTypeClass()
        {
            TypeDefinition item = new TypeDefinition("UnityEngine.Internal", "$UnityType", TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit | TypeAttributes.Public, base.ModuleContext.GetCorLibType("System.Object"));
            base.Module.Types.Add(item);
            return item;
        }

        protected override IStepContext Execute()
        {
            base.Execute();
            this.unityType = null;
            return new StepContext(this.unityType);
        }

        protected override void ProcessModule()
        {
            if (base.IsUnityEngine)
            {
                this.unityType = this.AddUnityTypeClass();
                this.AddConstructor();
                this.AddCreateInstanceMethod();
            }
        }

        public sealed class StepContext : IStepContext
        {
            public StepContext(TypeDefinition unityType)
            {
                this.UnityType = unityType;
            }

            public TypeDefinition UnityType { get; private set; }
        }
    }
}


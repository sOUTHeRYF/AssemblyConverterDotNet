namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class RemoveUnverifiableCodeAttributeStep : ModuleStep
    {
        protected override IStepContext Execute()
        {
            if (!base.OperationContext.IsWSA)
            {
                return null;
            }
            return base.Execute();
        }

        protected override void ProcessModule()
        {
            CustomAttribute item = base.Module.CustomAttributes.SingleOrDefault<CustomAttribute>(InnerClass.FuncA ?? (InnerClass.FuncA = new Func<CustomAttribute, bool>(InnerClass.InnerInstance.IsProcessModuleSpecial)));
            if (item != null)
            {
                base.Module.CustomAttributes.Remove(item);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class InnerClass
        {
            public static readonly RemoveUnverifiableCodeAttributeStep.InnerClass InnerInstance = new RemoveUnverifiableCodeAttributeStep.InnerClass();
            public static Func<CustomAttribute, bool> FuncA;

            internal bool IsProcessModuleSpecial(CustomAttribute a)
            {
                return ((a.AttributeType.FullName == "System.Security.UnverifiableCodeAttribute") && (a.AttributeType.Scope.GetAssemblyName() == "mscorlib"));
            }
        }
    }
}


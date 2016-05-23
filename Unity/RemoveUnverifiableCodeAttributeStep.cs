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
            CustomAttribute item = base.Module.CustomAttributes.SingleOrDefault<CustomAttribute>(<>c.<>9__1_0 ?? (<>c.<>9__1_0 = new Func<CustomAttribute, bool>(<>c.<>9.<ProcessModule>b__1_0)));
            if (item != null)
            {
                base.Module.CustomAttributes.Remove(item);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly RemoveUnverifiableCodeAttributeStep.<>c <>9 = new RemoveUnverifiableCodeAttributeStep.<>c();
            public static Func<CustomAttribute, bool> <>9__1_0;

            internal bool <ProcessModule>b__1_0(CustomAttribute a)
            {
                return ((a.AttributeType.FullName == "System.Security.UnverifiableCodeAttribute") && (a.AttributeType.Scope.GetAssemblyName() == "mscorlib"));
            }
        }
    }
}


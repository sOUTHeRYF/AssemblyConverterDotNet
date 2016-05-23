namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class FixInternalsVisibleToStep : ModuleStep
    {
        private void FixPublicKey(CustomAttribute attribute)
        {
            if ((attribute.AttributeType.FullName == "System.Runtime.CompilerServices.InternalsVisibleToAttribute") && (attribute.ConstructorArguments.Count >= 1))
            {
                CustomAttributeArgument argument = attribute.ConstructorArguments[0];
                char[] separator = new char[] { ',', ' ' };
                string str = ((string) argument.Value).Split(separator, StringSplitOptions.RemoveEmptyEntries).Where<string>((<>c.<>9__1_0 ?? (<>c.<>9__1_0 = new Func<string, bool>(<>c.<>9.<FixPublicKey>b__1_0)))).Aggregate<string>(<>c.<>9__1_1 ?? (<>c.<>9__1_1 = new Func<string, string, string>(<>c.<>9.<FixPublicKey>b__1_1)));
                attribute.ConstructorArguments[0] = new CustomAttributeArgument(base.ModuleContext.StringType, str);
            }
        }

        protected override void ProcessModule()
        {
            foreach (CustomAttribute attribute in base.Module.Assembly.CustomAttributes)
            {
                this.FixPublicKey(attribute);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly FixInternalsVisibleToStep.<>c <>9 = new FixInternalsVisibleToStep.<>c();
            public static Func<string, bool> <>9__1_0;
            public static Func<string, string, string> <>9__1_1;

            internal bool <FixPublicKey>b__1_0(string x)
            {
                return !x.StartsWith("PublicKey");
            }

            internal string <FixPublicKey>b__1_1(string a, string b)
            {
                return (a + ", " + b);
            }
        }
    }
}


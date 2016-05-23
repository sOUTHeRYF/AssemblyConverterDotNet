namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class FixMicrosoftMistakes : ModuleStep, IMethodDefinitionVisitor
    {
        private readonly List<string> errors = new List<string>();
        private const string message = "One or more methods have been found that use array of generic type where generic type is defined in the same assembly as the calling method but is instantiated with a type from a different assembly. This can potentially make app unstable on Windows Phone 8.0. Please refactor the code:\r\n- Use non-generic type as the array element (e.g. System.Object);\r\n- Create local type to wrap the type from the other assembly and use the local type for the generic instantiation.\r\nViolating method(s):\r\n";

        protected override IStepContext Execute()
        {
            if (base.OperationContext.Platform == Platform.WP80)
            {
                this.errors.Clear();
                base.Execute();
                if (this.errors.Count != 0)
                {
                    Console.WriteLine("One or more methods have been found that use array of generic type where generic type is defined in the same assembly as the calling method but is instantiated with a type from a different assembly. This can potentially make app unstable on Windows Phone 8.0. Please refactor the code:\r\n- Use non-generic type as the array element (e.g. System.Object);\r\n- Create local type to wrap the type from the other assembly and use the local type for the generic instantiation.\r\nViolating method(s):\r\n" + string.Join(";\r\n", this.errors) + ".");
                }
            }
            return null;
        }

        protected override void ProcessModule()
        {
            MethodDefinitionDispatcher.Dispatch(base.Module, this);
        }

        public void Visit(MethodDefinition method)
        {
            if (method.HasBody)
            {
                foreach (Instruction instruction in method.Body.Instructions)
                {
                    GenericInstanceType genericInstanceType;
                    if (!(instruction.OpCode != OpCodes.Newarr))
                    {
                        genericInstanceType = instruction.Operand as GenericInstanceType;
                        if (((genericInstanceType != null) && Utility.SameScope(genericInstanceType.Scope, method.DeclaringType.Scope)) && !genericInstanceType.GenericArguments.All<TypeReference>(delegate (TypeReference a) {
                            if (!Utility.SameScope(a.Scope, genericInstanceType.Scope))
                            {
                                return !a.IsValueType;
                            }
                            return true;
                        }))
                        {
                            this.errors.Add(string.Format("- {0} creates array of type {1}", method.FullName, genericInstanceType.FullName));
                        }
                    }
                }
            }
        }
    }
}


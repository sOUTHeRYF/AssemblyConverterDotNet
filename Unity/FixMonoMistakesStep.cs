namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;

    internal sealed class FixMonoMistakesStep : ModuleStep, IMethodDefinitionVisitor
    {
        protected override void ProcessModule()
        {
            MethodDefinitionDispatcher.Dispatch(base.Module, this);
            base.Module.Characteristics = ModuleCharacteristics.TerminalServerAware | ModuleCharacteristics.NoSEH | ModuleCharacteristics.NXCompat | ModuleCharacteristics.DynamicBase | ModuleCharacteristics.HighEntropyVA;
        }

        public void Visit(MethodDefinition method)
        {
            if (method.HasBody)
            {
                foreach (Instruction instruction in method.Body.Instructions)
                {
                    TypeReference operand = instruction.Operand as TypeReference;
                    if (operand != null)
                    {
                        if (((!operand.IsDefinition && !operand.IsArray) && (!operand.IsGenericInstance && !operand.IsGenericParameter)) && Utility.SameScope(operand.Scope, method.DeclaringType.Scope))
                        {
                            instruction.Operand = operand.Resolve();
                        }
                    }
                    else
                    {
                        MemberReference reference2 = instruction.Operand as MemberReference;
                        if (((reference2 != null) && !reference2.IsDefinition) && (!reference2.DeclaringType.IsArray && !reference2.DeclaringType.IsGenericInstance))
                        {
                            MethodReference reference3 = reference2 as MethodReference;
                            if (reference3 != null)
                            {
                                GenericInstanceMethod method2 = reference3 as GenericInstanceMethod;
                                if (method2 != null)
                                {
                                    MethodReference elementMethod = method2.ElementMethod;
                                    if (Utility.SameScope(elementMethod.DeclaringType.Scope, method.DeclaringType.Scope))
                                    {
                                        GenericInstanceMethod method3 = new GenericInstanceMethod(elementMethod.Resolve());
                                        foreach (TypeReference reference6 in method2.GenericArguments)
                                        {
                                            method3.GenericArguments.Add(reference6);
                                        }
                                        instruction.Operand = method3;
                                    }
                                }
                                else if (Utility.SameScope(reference3.DeclaringType.Scope, method.DeclaringType.Scope))
                                {
                                    instruction.Operand = reference3.Resolve();
                                }
                            }
                            else
                            {
                                FieldReference reference4 = reference2 as FieldReference;
                                if ((reference4 != null) && Utility.SameScope(reference4.DeclaringType.Scope, method.DeclaringType.Scope))
                                {
                                    instruction.Operand = reference4.Resolve();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}


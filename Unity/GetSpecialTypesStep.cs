namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;

    internal sealed class GetSpecialTypesStep : ModuleStep, IMethodDefinitionVisitor
    {
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
                    TypeReference reference3;
                    if ((instruction.OpCode != OpCodes.Call) && (instruction.OpCode != OpCodes.Callvirt))
                    {
                        continue;
                    }
                    MethodReference operand = (MethodReference) instruction.Operand;
                    if (!operand.IsGenericInstance)
                    {
                        continue;
                    }
                    GenericInstanceMethod method2 = (GenericInstanceMethod) operand;
                    TypeReference declaringType = method2.DeclaringType;
                    if (method2.Name == "GetComponents")
                    {
                        if (((declaringType.FullName == "UnityEngine.Component") || (declaringType.FullName == "UnityEngine.GameObject")) && !(declaringType.GetAssemblyName() != "UnityEngine"))
                        {
                            goto Label_00F9;
                        }
                        continue;
                    }
                    if ((!(method2.Name == "AddComponent") || (declaringType.FullName != "UnityEngine.GameObject")) || (declaringType.GetAssemblyName() != "UnityEngine"))
                    {
                        continue;
                    }
                Label_00F9:
                    reference3 = method2.GenericArguments[0];
                    if (!reference3.IsGenericParameter)
                    {
                        base.MetadataContainer.AddType(reference3);
                    }
                }
            }
        }
    }
}


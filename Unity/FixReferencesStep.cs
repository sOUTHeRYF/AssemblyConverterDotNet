namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using System;
    using System.Linq;

    internal sealed class FixReferencesStep : ModuleStep, ITypeDefinitionVisitor
    {
        protected override IStepContext Execute()
        {
            if (base.OperationContext.Platform != Platform.UAP)
            {
                return null;
            }
            return base.Execute();
        }

        protected override void ProcessModule()
        {
            for (int i = 0; i < base.Module.AssemblyReferences.Count; i++)
            {
                if (!(base.Module.AssemblyReferences[i].Name == "mscorlib"))
                {
                    base.Module.AssemblyReferences.RemoveAt(i);
                    i--;
                }
            }
            this.Visit(base.Module.Assembly, new GenericContext());
            this.Visit(base.Module, new GenericContext());
            TypeDefinitionDispatcher.Dispatch(base.Module, this);
            for (int j = 0; j < base.Module.AssemblyReferences.Count; j++)
            {
                if (!(base.Module.AssemblyReferences[j].Name != "mscorlib"))
                {
                    base.Module.AssemblyReferences.RemoveAt(j);
                    j--;
                }
            }
        }

        public void Visit(TypeDefinition type)
        {
            try
            {
                GenericContext context = new GenericContext();
                context.PushType(type);
                this.Visit(type, context);
                type.BaseType = base.ModuleContext.Retarget(type.BaseType, context);
                for (int i = 0; i < type.Interfaces.Count; i++)
                {
                    type.Interfaces[i] = base.ModuleContext.Retarget(type.Interfaces[i], context);
                }
                this.Visit(type.GenericParameters, context);
                foreach (FieldDefinition definition in type.Fields)
                {
                    this.Visit(definition, context);
                }
                foreach (MethodDefinition definition2 in type.Methods)
                {
                    this.Visit(definition2, context);
                }
                foreach (PropertyDefinition definition3 in type.Properties)
                {
                    this.Visit(definition3, context);
                }
                foreach (EventDefinition definition4 in type.Events)
                {
                    this.Visit(definition4, context);
                }
                context.PopType();
            }
            catch
            {
                Console.Error.WriteLine("Failed to fix references for type {0}", type.FullName);
                throw;
            }
        }

        private void Visit(EventDefinition @event, GenericContext context)
        {
            try
            {
                this.Visit((ICustomAttributeProvider) @event, context);
                @event.EventType = base.ModuleContext.Retarget(@event.EventType, context);
            }
            catch
            {
                Console.Error.WriteLine("Failed to fix references for event {0}", @event.FullName);
                throw;
            }
        }

        private void Visit(FieldDefinition field, GenericContext context)
        {
            try
            {
                this.Visit((ICustomAttributeProvider) field, context);
                field.FieldType = base.ModuleContext.Retarget(field.FieldType, context);
            }
            catch
            {
                Console.Error.WriteLine("Failed to fix references for field {0}", field.FullName);
                throw;
            }
        }

        private void Visit(ICustomAttributeProvider provider, GenericContext context)
        {
            using (Collection<CustomAttribute>.Enumerator enumerator = provider.CustomAttributes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CustomAttribute customAttribute = enumerator.Current;
                    try
                    {
                        MethodDefinition method = customAttribute.AttributeType.Resolve().Methods.Single<MethodDefinition>(m => m.FullName == customAttribute.Constructor.FullName);
                        customAttribute.Constructor = base.ModuleContext.Retarget(method, context);
                        for (int i = 0; i < customAttribute.ConstructorArguments.Count; i++)
                        {
                            CustomAttributeArgument argument = customAttribute.ConstructorArguments[i];
                            customAttribute.ConstructorArguments[i] = base.ModuleContext.Retarget(argument, context);
                        }
                        for (int j = 0; j < customAttribute.Fields.Count; j++)
                        {
                            CustomAttributeNamedArgument argument2 = customAttribute.Fields[j];
                            CustomAttributeArgument argument3 = base.ModuleContext.Retarget(argument2.Argument, context);
                            customAttribute.Fields[j] = new CustomAttributeNamedArgument(argument2.Name, argument3);
                        }
                        for (int k = 0; k < customAttribute.Properties.Count; k++)
                        {
                            CustomAttributeNamedArgument argument4 = customAttribute.Properties[k];
                            CustomAttributeArgument argument5 = base.ModuleContext.Retarget(argument4.Argument, context);
                            customAttribute.Properties[k] = new CustomAttributeNamedArgument(argument4.Name, argument5);
                        }
                        continue;
                    }
                    catch
                    {
                        Console.Error.WriteLine("Failed to fix references for attribute {0}", customAttribute.AttributeType.FullName);
                        throw;
                    }
                }
            }
        }

        private void Visit(MethodDefinition method, GenericContext context)
        {
            try
            {
                context.PushMethod(method);
                this.Visit((ICustomAttributeProvider) method, context);
                this.Visit(method.GenericParameters, context);
                this.Visit(method.MethodReturnType, context);
                method.ReturnType = base.ModuleContext.Retarget(method.ReturnType, context);
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    this.Visit(method.Parameters[i], context);
                    method.Parameters[i].ParameterType = base.ModuleContext.Retarget(method.Parameters[i].ParameterType, context);
                }
                for (int j = 0; j < method.Overrides.Count; j++)
                {
                    method.Overrides[j] = base.ModuleContext.Retarget(method.Overrides[j], context);
                }
                if (method.HasBody)
                {
                    foreach (ExceptionHandler handler in method.Body.ExceptionHandlers)
                    {
                        handler.CatchType = base.ModuleContext.Retarget(handler.CatchType, context);
                    }
                    foreach (VariableDefinition definition in method.Body.Variables)
                    {
                        definition.VariableType = base.ModuleContext.Retarget(definition.VariableType, context);
                    }
                    foreach (Instruction instruction in method.Body.Instructions)
                    {
                        MemberReference operand = instruction.Operand as MemberReference;
                        if (operand != null)
                        {
                            MethodReference reference3 = operand as MethodReference;
                            if (reference3 == null)
                            {
                                FieldReference field = operand as FieldReference;
                                if (field == null)
                                {
                                    TypeReference type = operand as TypeReference;
                                    if (type == null)
                                    {
                                        throw new NotImplementedException();
                                    }
                                    instruction.Operand = base.ModuleContext.Retarget(type, context);
                                }
                                else
                                {
                                    instruction.Operand = base.ModuleContext.Retarget(field, context);
                                }
                            }
                            else
                            {
                                instruction.Operand = base.ModuleContext.Retarget(reference3, context);
                            }
                        }
                        else
                        {
                            ParameterReference reference2 = instruction.Operand as ParameterReference;
                            if (reference2 != null)
                            {
                                reference2.ParameterType = base.ModuleContext.Retarget(reference2.ParameterType, context);
                            }
                        }
                    }
                    context.PopMethod();
                }
            }
            catch
            {
                Console.Error.WriteLine("Failed to fix references for method {0}", method.FullName);
                throw;
            }
        }

        private void Visit(PropertyDefinition property, GenericContext context)
        {
            try
            {
                this.Visit((ICustomAttributeProvider) property, context);
                property.PropertyType = base.ModuleContext.Retarget(property.PropertyType, context);
            }
            catch
            {
                Console.Error.WriteLine("Failed to fix references for property {0}", property.FullName);
                throw;
            }
        }

        private void Visit(Collection<GenericParameter> genericParameters, GenericContext context)
        {
            foreach (GenericParameter parameter in genericParameters)
            {
                for (int i = 0; i < parameter.Constraints.Count; i++)
                {
                    try
                    {
                        parameter.Constraints[i] = base.ModuleContext.Retarget(parameter.Constraints[i], context);
                    }
                    catch
                    {
                        Console.Error.WriteLine("Failed to fix references for generic parameter {0}", parameter.FullName);
                        throw;
                    }
                }
            }
        }
    }
}


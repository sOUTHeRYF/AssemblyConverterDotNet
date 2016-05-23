namespace Unity
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class Utility
    {
        public const string InternalNamespace = "UnityEngine.Internal";

        public static string ConvertToWindowsPath(this string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static Instruction CreateLdarg(ParameterDefinition parameter)
        {
            switch (parameter.Sequence)
            {
                case 0:
                    return Instruction.Create(OpCodes.Ldarg_0);

                case 1:
                    return Instruction.Create(OpCodes.Ldarg_1);

                case 2:
                    return Instruction.Create(OpCodes.Ldarg_2);

                case 3:
                    return Instruction.Create(OpCodes.Ldarg_3);
            }
            return Instruction.Create((parameter.Sequence < 0x100) ? OpCodes.Ldarg_S : OpCodes.Ldarg, parameter);
        }

        public static Instruction CreateLdc_I4(int value)
        {
            switch (value)
            {
                case -1:
                    return Instruction.Create(OpCodes.Ldc_I4_M1);

                case 0:
                    return Instruction.Create(OpCodes.Ldc_I4_0);

                case 1:
                    return Instruction.Create(OpCodes.Ldc_I4_1);

                case 2:
                    return Instruction.Create(OpCodes.Ldc_I4_2);

                case 3:
                    return Instruction.Create(OpCodes.Ldc_I4_3);

                case 4:
                    return Instruction.Create(OpCodes.Ldc_I4_4);

                case 5:
                    return Instruction.Create(OpCodes.Ldc_I4_5);

                case 6:
                    return Instruction.Create(OpCodes.Ldc_I4_6);

                case 7:
                    return Instruction.Create(OpCodes.Ldc_I4_7);

                case 8:
                    return Instruction.Create(OpCodes.Ldc_I4_8);
            }
            if ((value >= -128) && (value <= 0x7f))
            {
                return Instruction.Create(OpCodes.Ldc_I4_S, (sbyte) value);
            }
            return Instruction.Create(OpCodes.Ldc_I4, value);
        }

        public static Instruction CreateLdloc(VariableDefinition variable)
        {
            switch (variable.Index)
            {
                case 0:
                    return Instruction.Create(OpCodes.Ldloc_0);

                case 1:
                    return Instruction.Create(OpCodes.Ldloc_1);

                case 2:
                    return Instruction.Create(OpCodes.Ldloc_2);

                case 3:
                    return Instruction.Create(OpCodes.Ldloc_3);
            }
            return Instruction.Create((variable.Index < 0x100) ? OpCodes.Ldloc_S : OpCodes.Ldloc, variable);
        }

        public static Instruction CreateStloc(VariableDefinition variable)
        {
            switch (variable.Index)
            {
                case 0:
                    return Instruction.Create(OpCodes.Stloc_0);

                case 1:
                    return Instruction.Create(OpCodes.Stloc_1);

                case 2:
                    return Instruction.Create(OpCodes.Stloc_2);

                case 3:
                    return Instruction.Create(OpCodes.Stloc_3);
            }
            return Instruction.Create((variable.Index < 0x100) ? OpCodes.Stloc_S : OpCodes.Stloc, variable);
        }

        public static void EmitLdarg(this ILProcessor ilProcessor, ParameterDefinition parameter)
        {
            Instruction instruction = CreateLdarg(parameter);
            ilProcessor.Append(instruction);
        }

        public static void EmitLdarga(this ILProcessor ilProcessor, ParameterDefinition parameter)
        {
            Instruction instruction = Instruction.Create((parameter.Sequence < 0x100) ? OpCodes.Ldarga_S : OpCodes.Ldarga, parameter);
            ilProcessor.Append(instruction);
        }

        public static void EmitLdc_I4(this ILProcessor ilProcessor, int value)
        {
            Instruction instruction = CreateLdc_I4(value);
            ilProcessor.Append(instruction);
        }

        public static void EmitLdloc(this ILProcessor ilProcessor, VariableDefinition variable)
        {
            Instruction instruction = CreateLdloc(variable);
            ilProcessor.Append(instruction);
        }

        public static void EmitLdloca(this ILProcessor ilProcessor, VariableDefinition variable)
        {
            Instruction instruction = Instruction.Create((variable.Index < 0x100) ? OpCodes.Ldloca_S : OpCodes.Ldloca, variable);
            ilProcessor.Append(instruction);
        }

        public static void EmitStloc(this ILProcessor ilProcessor, VariableDefinition variable)
        {
            Instruction instruction = CreateStloc(variable);
            ilProcessor.Append(instruction);
        }

        public static string GetAssemblyName(this IMetadataScope scope)
        {
            return scope.GetAssemblyNameReference().Name;
        }

        public static string GetAssemblyName(this TypeReference type)
        {
            return type.Scope.GetAssemblyName();
        }

        public static AssemblyNameReference GetAssemblyNameReference(this IMetadataScope scope)
        {
            MetadataScopeType metadataScopeType = scope.MetadataScopeType;
            if (metadataScopeType != MetadataScopeType.AssemblyNameReference)
            {
                if (metadataScopeType != MetadataScopeType.ModuleDefinition)
                {
                    throw new NotSupportedException(string.Format("Metadata scope type {0} is not supported.", scope.MetadataScopeType));
                }
                return ((ModuleDefinition) scope).Assembly.Name;
            }
            return (AssemblyNameReference) scope;
        }

        public static AssemblyNameReference GetAssemblyNameReference(this TypeReference type)
        {
            return type.Scope.GetAssemblyNameReference();
        }

        public static bool InheritsFrom(TypeReference type, string baseTypeFullName, string baseTypeAssemblyName)
        {
            if (type.FullName == baseTypeFullName)
            {
                return true;
            }
            for (TypeReference reference = type.Resolve().BaseType; reference != null; reference = reference.Resolve().BaseType)
            {
                if ((reference.FullName == baseTypeFullName) && (reference.GetAssemblyName() == baseTypeAssemblyName))
                {
                    return true;
                }
            }
            return false;
        }

        public static MethodReference MakeGenericMethod(this MethodReference method, params TypeReference[] arguments)
        {
            GenericInstanceType declaringType = method.DeclaringType.MakeGenericType(arguments);
            return method.MakeGenericMethod(declaringType);
        }

        public static MethodReference MakeGenericMethod(this MethodReference method, GenericInstanceType declaringType)
        {
            MethodReference owner = new MethodReference(method.Name, method.ReturnType) {
                DeclaringType = declaringType ?? method.DeclaringType,
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention
            };
            foreach (ParameterDefinition definition in method.Parameters)
            {
                owner.Parameters.Add(definition);
            }
            foreach (GenericParameter parameter in method.GenericParameters)
            {
                owner.GenericParameters.Add(new GenericParameter(parameter.Name, owner));
            }
            return owner;
        }

        public static GenericInstanceType MakeGenericType(this TypeReference type, params TypeReference[] arguments)
        {
            if (type.GenericParameters.Count != arguments.Length)
            {
                throw new ArgumentException("Invalid number of generic arguments.");
            }
            GenericInstanceType type2 = new GenericInstanceType(type);
            foreach (TypeReference reference in arguments)
            {
                type2.GenericArguments.Add(reference);
            }
            return type2;
        }

        public static bool SameScope(IMetadataScope left, IMetadataScope right)
        {
            return (left.GetAssemblyName() == right.GetAssemblyName());
        }

        public static bool SameType(TypeReference left, TypeReference right)
        {
            return ((left.FullName == right.FullName) && SameScope(left.Scope, right.Scope));
        }
    }
}


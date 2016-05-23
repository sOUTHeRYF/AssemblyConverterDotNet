namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Linq;

    internal sealed class GetTypesStep : ModuleStep, ITypeDefinitionVisitor
    {
        private static readonly string[] ignoreAssemblies = new string[] { "Boo.Lang", "UnityScript.Lang", "WinRTLegacy" };

        private static bool IsModuleTypeOrNestedUnderModuleType(TypeDefinition type)
        {
            return ((type.FullName == "<Module>") || ((type.DeclaringType != null) && IsModuleTypeOrNestedUnderModuleType(type.DeclaringType)));
        }

        protected override void ProcessModule()
        {
            if (!ignoreAssemblies.Contains<string>(base.Module.Assembly.Name.Name))
            {
                TypeDefinitionDispatcher.Dispatch(base.Module, this);
            }
        }

        public void Visit(TypeDefinition type)
        {
            if ((((((!type.IsNestedPrivate && !type.HasGenericParameters) && !IsModuleTypeOrNestedUnderModuleType(type)) && !base.MetadataContainer.IsDelegate(type)) && (!base.MetadataContainer.IsException(type) || (base.IsUnityEngine && (type.FullName == "UnityEngine.UnityException")))) && (((type.Namespace != "UnityEngine.Internal") || (type.Name == "$FuncPtrs")) || !base.IsUnityEngine)) && !type.FullName.Contains("$FieldNamesStorage"))
            {
                base.MetadataContainer.AddType(type);
            }
        }
    }
}


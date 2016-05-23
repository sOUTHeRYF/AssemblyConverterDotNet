using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Unity
{
    internal sealed class ImplementUnityTypeConstructorsStep : Step
    {
        [CompilerGenerated]
        [Serializable]
        private sealed class <>c
		{
			public static readonly ImplementUnityTypeConstructorsStep.<>c<>9 = new ImplementUnityTypeConstructorsStep.<>c();

        public static Func<ExportedType, bool> <>9__3_0;

			public static Func<MethodDefinition, bool> <>9__3_1;

			public static Func<FieldDefinition, bool> <>9__3_2;

			public static Func<MethodDefinition, bool> <>9__4_0;

			internal bool <Execute>b__3_0(ExportedType t)
        {
            return t.FullName == "System.IntPtr";
        }

        internal bool <Execute>b__3_1(MethodDefinition m)
        {
            return m.IsConstructor && !m.IsStatic && !m.HasParameters;
        }

        internal bool <Execute>b__3_2(FieldDefinition f)
        {
            return f.Name == "data";
        }

        internal bool <Process>b__4_0(MethodDefinition m)
        {
            return m.IsConstructor && !m.IsStatic && !m.HasParameters;
        }
    }

    private TypeDefinition intPtrType;

    private MethodDefinition unityTypeConstructor;

    private FieldDefinition dataField;

    protected override IStepContext Execute()
    {
        OperationContext expr_06 = base.OperationContext;
        ModuleDefinition corLib = expr_06.CorLib;
        ModuleDefinition expr_17 = expr_06.UnityEngineModuleContext.Module;
        TypeDefinition type = expr_17.GetType("UnityEngine.Internal", "$UnityType");
        TypeDefinition type2 = expr_17.GetType("UnityEngine.Internal", "$Metadata");
        TypeDefinition arg_77_1;
        if ((arg_77_1 = corLib.GetType("System.IntPtr")) == null)
        {
            IEnumerable<ExportedType> arg_6D_0 = corLib.ExportedTypes;
            Func<ExportedType, bool> arg_6D_1;
            if ((arg_6D_1 = ImplementUnityTypeConstructorsStep.<> c.<> 9__3_0) == null)
				{
                arg_6D_1 = (ImplementUnityTypeConstructorsStep.<> c.<> 9__3_0 = new Func<ExportedType, bool>(ImplementUnityTypeConstructorsStep.<> c.<> 9.< Execute > b__3_0));
            }
            arg_77_1 = arg_6D_0.Single(arg_6D_1).Resolve();
        }
        this.intPtrType = arg_77_1;
        IEnumerable<MethodDefinition> arg_A2_0 = type.Methods;
        Func<MethodDefinition, bool> arg_A2_1;
        if ((arg_A2_1 = ImplementUnityTypeConstructorsStep.<> c.<> 9__3_1) == null)
			{
            arg_A2_1 = (ImplementUnityTypeConstructorsStep.<> c.<> 9__3_1 = new Func<MethodDefinition, bool>(ImplementUnityTypeConstructorsStep.<> c.<> 9.< Execute > b__3_1));
        }
        this.unityTypeConstructor = arg_A2_0.Single(arg_A2_1);
        IEnumerable<FieldDefinition> arg_D2_0 = type2.Fields;
        Func<FieldDefinition, bool> arg_D2_1;
        if ((arg_D2_1 = ImplementUnityTypeConstructorsStep.<> c.<> 9__3_2) == null)
			{
            arg_D2_1 = (ImplementUnityTypeConstructorsStep.<> c.<> 9__3_2 = new Func<FieldDefinition, bool>(ImplementUnityTypeConstructorsStep.<> c.<> 9.< Execute > b__3_2));
        }
        this.dataField = arg_D2_0.Single(arg_D2_1);
        AssemblyWrapper[] assemblies = base.MetadataContainer.Assemblies;
        for (int i = 0; i < assemblies.Length; i++)
        {
            TypeWrapper[] types = assemblies[i].Types;
            for (int j = 0; j < types.Length; j++)
            {
                TypeWrapper typeWrapper = types[j];
                this.Process(typeWrapper);
            }
        }
        this.intPtrType = null;
        this.unityTypeConstructor = null;
        this.dataField = null;
        return null;
    }

    private void Process(TypeWrapper typeWrapper)
    {
        TypeDefinition expr_06 = typeWrapper.UnityType;
        ModuleDefinition module = expr_06.Module;
        IEnumerable<MethodDefinition> arg_31_0 = expr_06.Methods;
        Func<MethodDefinition, bool> arg_31_1;
        if ((arg_31_1 = ImplementUnityTypeConstructorsStep.<> c.<> 9__4_0) == null)
			{
            arg_31_1 = (ImplementUnityTypeConstructorsStep.<> c.<> 9__4_0 = new Func<MethodDefinition, bool>(ImplementUnityTypeConstructorsStep.<> c.<> 9.< Process > b__4_0));
        }
        MethodBody body = arg_31_0.Single(arg_31_1).Body;
        ILProcessor iLProcessor = body.GetILProcessor();
        iLProcessor.EmitLdarg(body.ThisParameter);
        iLProcessor.Emit(OpCodes.Call, base.OperationContext.Import(module, this.unityTypeConstructor));
        int num = ((typeWrapper.Methods != null) ? typeWrapper.Methods.Length : 0) + ((typeWrapper.Fields != null) ? (typeWrapper.Fields.Length * 2) : 0);
        if (num != 0)
        {
            FieldReference field = module.ImportReference(this.dataField);
            VariableDefinition variableDefinition = null;
            if (num > 1)
            {
                body.InitLocals = true;
                variableDefinition = new VariableDefinition("data", module.ImportReference(this.intPtrType));
                body.Variables.Add(variableDefinition);
                iLProcessor.Emit(OpCodes.Ldsfld, field);
                iLProcessor.EmitStloc(variableDefinition);
            }
            if (typeWrapper.Methods != null)
            {
                MethodWrapper[] methods = typeWrapper.Methods;
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodWrapper methodWrapper = methods[i];
                    if (variableDefinition != null)
                    {
                        iLProcessor.EmitLdloc(variableDefinition);
                    }
                    else
                    {
                        iLProcessor.Emit(OpCodes.Ldsfld, field);
                    }
                    iLProcessor.EmitLdc_I4(methodWrapper.Offset);
                    iLProcessor.Emit(OpCodes.Add);
                    iLProcessor.Emit(OpCodes.Ldftn, module.ImportReference(methodWrapper.InvokeMethod));
                    iLProcessor.Emit(OpCodes.Stind_I);
                }
            }
            if (typeWrapper.Fields != null)
            {
                int num2 = base.OperationContext.Is64 ? 8 : 4;
                FieldWrapper[] fields = typeWrapper.Fields;
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldWrapper fieldWrapper = fields[i];
                    iLProcessor.EmitLdloc(variableDefinition);
                    iLProcessor.EmitLdc_I4(fieldWrapper.Offset);
                    iLProcessor.Emit(OpCodes.Add);
                    iLProcessor.Emit(OpCodes.Ldftn, fieldWrapper.Getter);
                    iLProcessor.Emit(OpCodes.Stind_I);
                    iLProcessor.EmitLdloc(variableDefinition);
                    iLProcessor.EmitLdc_I4(fieldWrapper.Offset + num2);
                    iLProcessor.Emit(OpCodes.Add);
                    iLProcessor.Emit(OpCodes.Ldftn, fieldWrapper.Setter);
                    iLProcessor.Emit(OpCodes.Stind_I);
                }
            }
        }
        iLProcessor.Emit(OpCodes.Ret);
    }
}
}

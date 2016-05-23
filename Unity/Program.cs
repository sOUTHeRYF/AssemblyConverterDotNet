namespace Unity
{
    using System;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                Operation operation1 = new Operation();
                operation1.AddStep(new ParseArgsStep(args), null);
                operation1.AddStep(new LoadModulesStep(), null);
                operation1.AddStep(new RemoveUnverifiableCodeAttributeStep(), null);
                operation1.AddStep(new FixReferencesStep(), null);
                operation1.AddStep(new FixMicrosoftMistakes(), null);
                operation1.AddStep(new FixMonoMistakesStep(), null);
                operation1.AddStep(new GetSystemTypesStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new GetTypesStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new GetSpecialTypesStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new SetTypeIdsStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new CreateWinRTBridgeStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddSpecialConstructorStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new ImplementSpecialConstructorStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddFieldGetterSetterStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddUnityTypeClassStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddUnityTypeClassesStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddMethodUtilityClassStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddInvokeMethodMethodsStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new GenerateMetadataStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new SaveMetadataStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddGetUnityTypeMethodStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddMetadataLoadStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new ImplementUnityTypeConstructorsStep(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new AddFillTypeMapsMethodBody(), new Func<OperationContext, bool>(Program.SkipMetadata));
                operation1.AddStep(new FixInternalsVisibleToStep(), null);
                operation1.AddStep(new RemoveDebuggableAttributeStep(), null);
                operation1.AddStep(new FixWinRTComponentReferences(), null);
                operation1.AddStep(new GenerateNewMVIDsStep(), null);
                operation1.AddStep(new SaveModulesStep(), null);
                operation1.Execute();
                return 0;
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1);
                return 1;
            }
        }

        private static bool SkipMetadata(OperationContext context)
        {
            return context.SkipMetadata;
        }
    }
}


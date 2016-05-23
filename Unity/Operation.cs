namespace Unity
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal sealed class Operation
    {
        private readonly List<Step> steps = new List<Step>();

        public void AddStep(Step step, Func<OperationContext, bool> skip = null)
        {
            step.Skip = skip;
            this.steps.Add(step);
        }

        public void Execute()
        {
            OperationContext operationContext = new OperationContext();
            IStepContext previousStepContext = null;
            using (List<Step>.Enumerator enumerator = this.steps.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    previousStepContext = enumerator.Current.Execute(operationContext, previousStepContext);
                }
            }
        }
    }
}


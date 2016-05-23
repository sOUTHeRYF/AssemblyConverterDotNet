namespace Unity
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class Step
    {
        protected Step()
        {
        }

        protected abstract IStepContext Execute();
        public IStepContext Execute(Unity.OperationContext operationContext, IStepContext previousStepContext)
        {
            if ((this.Skip != null) && this.Skip(operationContext))
            {
                return null;
            }
            this.OperationContext = operationContext;
            this.PreviousStepContext = previousStepContext;
            this.OperationContext = null;
            this.PreviousStepContext = null;
            return this.Execute();
        }

        protected Unity.MetadataContainer MetadataContainer
        {
            get
            {
                return this.OperationContext.MetadataContainer;
            }
        }

        protected Unity.OperationContext OperationContext { get; private set; }

        protected IStepContext PreviousStepContext { get; private set; }

        public Func<Unity.OperationContext, bool> Skip { get; set; }
    }
}


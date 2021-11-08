using System;
using System.Collections.Generic;

namespace UniInject
{
    public class MultipleBindingsException : Exception
    {
        private readonly List<MissingBindingException> missingBindingExceptionList;

        public MultipleBindingsException(List<MissingBindingException> missingBindingExceptionList)
            : base("Missing bindings:")
        {
            this.missingBindingExceptionList = missingBindingExceptionList;
        }
    }
}

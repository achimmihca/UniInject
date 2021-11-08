using System;
using System.Collections.Generic;
using System.Linq;

namespace UniInject
{
    public class InjectionException : Exception
    {
        private const string MessageSeparator = "\n    ";

        public List<InjectionException> CauseExceptionList { get; private set; }

        public InjectionException(string message, List<InjectionException> causeExceptionList)
            : base(message
                   + MessageSeparator
                   + string.Join(MessageSeparator,
                       causeExceptionList.Select(ex => ex.Message)))
        {
            CauseExceptionList = causeExceptionList;
        }

        public InjectionException(string message) : base(message)
        {
        }

        public InjectionException(string message, Exception e) : base(message, e)
        {
        }
    }
}

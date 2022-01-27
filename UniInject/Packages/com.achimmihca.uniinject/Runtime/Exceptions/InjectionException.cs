using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UniInject
{
    public class InjectionException : Exception
    {
        private const string MessageSeparator = "\n    ";

        public List<Exception> CauseExceptionList { get; private set; }

        public InjectionException(string message, List<Exception> causeExceptionList)
            : base(message
                   + MessageSeparator
                   + CreateMergedExceptionMessage(causeExceptionList))
        {
            CauseExceptionList = causeExceptionList;
        }

        private static string CreateMergedExceptionMessage(List<Exception> causeExceptionList)
        {
            string CreateDeepExceptionMessage(Exception ex)
            {
                string causeMessage = ex.InnerException != null
                    ? MessageSeparator + "    Caused by: " + CreateDeepExceptionMessage(ex.InnerException)
                    : "";
                return ex.Message + causeMessage;
            }

            List<string> causeExceptionMessages = causeExceptionList
                .Select(ex => CreateDeepExceptionMessage(ex))
                .ToList();
            return string.Join(MessageSeparator, causeExceptionMessages);
        }

        public InjectionException(string message) : base(message)
        {
        }

        public InjectionException(string message, Exception e) : base(message, e)
        {
        }
    }
}

using System;

namespace UniInject
{
    public class RebindingException : Exception
    {
        public object InjectionKey { get; private set; }

        public RebindingException(object injectionKey)
            : base($"Re-binding of key {injectionKey}")
        {
            InjectionKey = injectionKey;
        }
    }
}

using System;

namespace UniInject
{
    public class MissingBindingException : Exception
    {
        public object ValueToBeInjected { get; private set; }
        public object InjectionKey { get; private set; }
        public object TargetObject { get; private set; }
        public Type TargetType { get; private set; }

        public MissingBindingException(object valueToBeInjected, object injectionKey, object targetObject)
            : base($"Value to be injected for key {injectionKey} is {NullableToString(valueToBeInjected)}" +
                   $" when injecting object {targetObject}" +
                   $" of type {targetObject.GetType()}")
        {
            ValueToBeInjected = valueToBeInjected;
            InjectionKey = injectionKey;
            TargetObject = targetObject;
            TargetType = targetObject.GetType();
        }

        public MissingBindingException(object valueToBeInjected, object injectionKey, Type targetType)
            : base($"Value to be injected for key {injectionKey} is {NullableToString(valueToBeInjected)}" +
                   $" when instantiating object of type {targetType}")
        {
            ValueToBeInjected = valueToBeInjected;
            InjectionKey = injectionKey;
            TargetType = targetType;
        }

        public MissingBindingException(object injectionKey, object target)
            : base($"Missing binding for key {injectionKey}" +
                   $" when injecting object {target}" +
                   $" of type {target.GetType()}")
        {
            InjectionKey = injectionKey;
            TargetObject = target;
        }

        public MissingBindingException(object injectionKey, Type targetType)
            : base($"Missing binding for key {injectionKey}" +
                   $" when instantiating object of type {targetType}")
        {
            InjectionKey = injectionKey;
            TargetType = targetType;
        }

        public MissingBindingException(object injectionKey)
            : base($"Missing binding for key {injectionKey}")
        {
            InjectionKey = injectionKey;
        }

        private static string NullableToString(object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            return obj.ToString();
        }
    }
}

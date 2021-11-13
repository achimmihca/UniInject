using System;

namespace UniInject
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectionKeyAttribute : Attribute
    {
        public object Key { get; set; }

        public InjectionKeyAttribute(string key)
        {
            this.Key = key;
        }
    }
}

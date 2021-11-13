using System;

namespace UniInject
{
    [AttributeUsage(AttributeTargets.Constructor
                  | AttributeTargets.Field
                  | AttributeTargets.Property
                  | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
        private object internalKey;
        public object key
        {
            get
            {
                if (internalKey != null)
                {
                    return internalKey;
                }
                else if (!string.IsNullOrEmpty(uxmlName))
                {
                    return "#" + uxmlName;
                }
                else if (!string.IsNullOrEmpty(uxmlClass))
                {
                    return "." + uxmlClass;
                }

                return null;
            }
            set
            {
                internalKey = value;
            }
        }

        public string uxmlName;
        public string uxmlClass;

        public bool optional;

        public SearchMethods searchMethod = SearchMethods.SearchInBindings;

        public InjectAttribute()
        {
        }
    }
}

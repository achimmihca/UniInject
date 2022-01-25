using System.Collections.Generic;

namespace UniInject.Extensions
{
    public static class InjectorExtensions
    {
        public static void AddBindingForInstance<T>(this Injector injector,
            T instance,
            RebindingBehavior rebindingBehavior = RebindingBehavior.LogWarning)
        {
            injector.AddBindingForInstance(typeof(T), instance, rebindingBehavior);
        }

        public static void AddBindingForInstance<T>(this Injector injector,
            object key,
            T instance,
            RebindingBehavior rebindingBehavior = RebindingBehavior.LogWarning)
        {
            IBinding binding = new Binding(key, new ExistingInstanceProvider<T>(instance));
            injector.AddBinding(binding, rebindingBehavior);
        }

        public static void AddBindings(this Injector injector,
            BindingBuilder bindingBuilder,
            RebindingBehavior rebindingBehavior = RebindingBehavior.LogWarning)
        {
            List<IBinding> newBindings = bindingBuilder.GetBindings();
            newBindings.ForEach(newBinding => injector.AddBinding(newBinding, rebindingBehavior));
        }

        public static Injector CreateChildInjector(this Injector injector)
        {
            return UniInjectUtils.CreateInjector(injector);
        }
    }
}

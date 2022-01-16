using UniInject;
using UnityEngine;
using UnityEngine.UIElements;

public static class UniInjectExtensions
{
    public static void InjectAllComponentsInChildren(this Injector injector, GameObject gameObject, bool includeInactive = false)
    {
        foreach (INeedInjection childThatNeedsInjection in gameObject.GetComponentsInChildren<INeedInjection>(includeInactive))
        {
            injector.Inject(childThatNeedsInjection);
        }
    }

    public static void InjectAllComponentsInChildren(this Injector injector, MonoBehaviour monoBehaviour, bool includeInactive = false)
    {
        InjectAllComponentsInChildren(injector, monoBehaviour.gameObject, includeInactive);
    }

    public static Injector WithRootVisualElement(this Injector injector, VisualElement visualElement)
    {
        Injector childInjector = UniInjectUtils.CreateInjector(injector);
        IProvider provider = new ExistingInstanceProvider<VisualElement>(visualElement);
        childInjector.AddBinding(new Binding("rootVisualElement", provider), RebindingBehavior.Ignore);
        return childInjector;
    }
}

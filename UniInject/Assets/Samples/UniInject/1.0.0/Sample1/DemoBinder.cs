using System.Collections;
using System.Collections.Generic;
using UniInject;
using UnityEngine;
using UnityEngine.UIElements;
using IBinding = UniInject.IBinding;

public class DemoBinder : MonoBehaviour, IBinder
{
    public List<IBinding> GetBindings()
    {
        BindingBuilder bb = new BindingBuilder();
        bb.BindExistingInstance(this);
        bb.BindExistingInstanceLazy(() => LazyInjectionDemo.Instance);
        bb.BindExistingInstanceLazy(() => FindObjectOfType<UIDocument>());
        bb.Bind("author").ToExistingInstance("Tolkien");
        bb.Bind(typeof(int)).ToExistingInstance(42);
        bb.Bind("personWithAge").ToExistingInstance("Bob");
        bb.Bind(typeof(IDemoInterface)).ToNewInstancesOfType(typeof(DemoInterfaceImpl));
        bb.Bind(typeof(IDemoInterfaceWithConstructorParameters)).ToSingleInstanceOfType(typeof(DemoInterfaceWithConstructorParametersImpl));
        return bb.GetBindings();
    }
}

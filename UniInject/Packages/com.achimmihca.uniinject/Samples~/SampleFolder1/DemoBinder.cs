using System.Collections;
using System.Collections.Generic;
using UniInject;
using UnityEngine;

public class DemoBinder : MonoBehaviour, IBinder
{
    public List<IBinding> GetBindings()
    {
        BindingBuilder bb = new BindingBuilder();
        bb.BindExistingInstance(this);
        bb.BindExistingInstanceLazy(() => LazyInjectionDemo.Instance);
        bb.Bind("author").ToExistingInstance("Tolkien");
        bb.Bind(typeof(int)).ToExistingInstance(42);
        bb.Bind("personWithAge").ToExistingInstance("Bob");
        bb.Bind(typeof(IDemoInterface)).ToNewInstancesOfType(typeof(DemoInterfaceImpl));
        bb.Bind(typeof(IDemoInterfaceWithConstructorParameters)).ToSingleInstanceOfType(typeof(DemoInterfaceWithConstructorParametersImpl));
        return bb.GetBindings();
    }
}

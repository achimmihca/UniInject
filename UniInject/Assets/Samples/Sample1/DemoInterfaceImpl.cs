using UnityEngine;

public class DemoInterfaceImpl : IDemoInterface
{
    private static int instanceCount;

    private readonly int instanceIndex;

    public DemoInterfaceImpl()
    {
        instanceIndex = instanceCount;
        instanceCount++;
    }

    public string GetGreeting()
    {
        return $"Hello world from instance {instanceIndex}!";
    }

    public override string ToString()
    {
        return GetGreeting() + " (DependencyInjectionDemoInterfaceImpl)";
    }
}

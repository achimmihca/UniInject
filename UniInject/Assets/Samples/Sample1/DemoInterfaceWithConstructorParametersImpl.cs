using System.Collections.Generic;
using UniInject;

public class DemoInterfaceWithConstructorParametersImpl : IDemoInterfaceWithConstructorParameters
{
    private readonly string name;

    // This constructor should not be used for instantiation during dependency injection, because it is not annotated.
    public DemoInterfaceWithConstructorParametersImpl(List<string> names)
    {
        this.name = names[0];
    }

    [Inject]
    public DemoInterfaceWithConstructorParametersImpl([InjectionKey("author")] string name)
    {
        this.name = name;
    }

    public string GetByeBye()
    {
        return "Bye bye " + name + "!";
    }
}

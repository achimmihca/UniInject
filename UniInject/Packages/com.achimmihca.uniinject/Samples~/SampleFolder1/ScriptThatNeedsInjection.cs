﻿using UniInject;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UniInject.UniInjectUtils;

// Ignore warnings about unassigned fields.
// Their values are injected, but this is not visible to the compiler.
#pragma warning disable CS0649

public class ScriptThatNeedsInjection : MonoBehaviour, INeedInjection
{
    // The marker attribute can be used to check that the field has been set.
    // Use the corresponding menu item (under UniInject) to perform a check on the current scene.
    [InjectedInInspector]
    public Transform referencedTransform;

    // Inject field via GetComponentInChildren
    [Inject(searchMethod = SearchMethods.GetComponentInChildren)]
    private ChildOfScriptThatNeedsInjection child;

    // Inject property via GetComponentInParent
    [Inject(searchMethod = SearchMethods.GetComponentInParent)]
    private ParentOfScriptThatNeedsInjection Parent { get; set; }

    // Inject readonly field via GetComponentInParent
    [Inject(searchMethod = SearchMethods.GetComponentInParent)]
    private readonly OtherComponentOfScriptThatNeedsInjection siblingComponent;

    // Inject readonly property via FindObjectOfType
    [Inject(searchMethod = SearchMethods.FindObjectOfType)]
    private readonly Canvas canvas;

    // Inject property
    [Inject]
    private DemoBinder dependencyInjectionDemoBinder { get; set; }

    // Inject field. Binding this instance is done lazy, so it will not be loaded if not injected here.
    // Try it: Remove the [Inject] annotation and see if the instance is still created.
    [Inject]
    private LazyInjectionDemo lazyInjectionDemo;

    // Inject optional
    [Inject(optional = true)]
    private UnityEngine.UI.Image optionalImage;

    [Inject(searchMethod = SearchMethods.GetComponentInChildren, optional = true)]
    private Text uiText;

    // Inject property using a specific key instead of the type.
    [Inject(key = "author")]
    private string NameOfAuthor { get; set; }

    // Inject VisualElement by name (using '#' as prefix) or by class (using '.' as prefix).
    // By default, the VisualElement will be searched starting from the UIDocument, that is tagged as "UIDocument" (see SceneInjectionManager).
    // To use another root VisualElement for the search, set Injector.RootVisualElement.
    [Inject(key = "#theLabel")]
    private UnityEngine.UIElements.Label theUxmlLabel { get; set; }

    // The instance of this field is created during injection.
    // Depending how the interface is bound (singleton or not),
    // the instances of demoInterfaceInstance1 and demoInterfaceInstance2 will be the same or different objects.
    [Inject]
    private IDemoInterface demoInterfaceInstance1;

    [Inject]
    private IDemoInterface demoInterfaceInstance2;

    [Inject]
    private IDemoInterfaceWithConstructorParameters demoInterfaceInstanceWithConstructorParameters;

    // This field is set in a method via method injection
    private string methodInjectionField;

    [Inject]
    private void SetMethodInjectionField([InjectionKey("personWithAge")] string personWithAge, int age)
    {
        this.methodInjectionField = $"{personWithAge} is {age} years old";
    }

    // Inject the injector that was used to inject all the fields.
    // The injector can be used at runtime to inject newly created scripts.
    [Inject]
    private Injector injector;

    void Start()
    {
        Debug.Log("Parent: " + Parent);
        Debug.Log("Child: " + child);
        Debug.Log("Sibling Component: " + siblingComponent);

        Debug.Log("Canvas: " + canvas);

        Debug.Log("DependencyInjectionDemoBinder: " + dependencyInjectionDemoBinder);
        Debug.Log("LazyInjectionDemo: " + lazyInjectionDemo);

        Debug.Log("Author: " + NameOfAuthor);

        Debug.Log("Field from method injection:" + methodInjectionField);

        Debug.Log("Instance of an interface (field 1):" + demoInterfaceInstance1.GetGreeting());
        Debug.Log("Instance of an interface (field 2):" + demoInterfaceInstance2.GetGreeting());

        Debug.Log("Instance of an interface with constructor parameters:" + demoInterfaceInstanceWithConstructorParameters.GetByeBye());

        Debug.Log("Optional Image: " + optionalImage);
        Debug.Log("Optional uiText: " + uiText);

        Debug.Log("The bound int: " + SceneInjector.GetValueForInjectionKey<int>());
        Debug.Log("The bound instance of an interface: " + SceneInjector.GetValueForInjectionKey<IDemoInterface>());

        Debug.Log("theUxmlLabel.text: " + theUxmlLabel.text);

        Debug.Log("Injector:" + injector);
    }
}

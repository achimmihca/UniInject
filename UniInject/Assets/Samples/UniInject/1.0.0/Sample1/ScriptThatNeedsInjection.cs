using System.Linq;
using UniInject;
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
    [Inject(SearchMethod = SearchMethods.GetComponentInChildren)]
    private ChildOfScriptThatNeedsInjection child;

    [Inject(SearchMethod = SearchMethods.GetComponentsInChildren)]
    private Transform[] children;

    // Inject property via GetComponentInParent
    [Inject(SearchMethod = SearchMethods.GetComponentInParent)]
    private ParentOfScriptThatNeedsInjection Parent { get; set; }

    // Inject readonly field via GetComponentInParent
    [Inject(SearchMethod = SearchMethods.GetComponentInParent)]
    private readonly OtherComponentOfScriptThatNeedsInjection siblingComponent;

    // Inject readonly property via FindObjectOfType
    [Inject(SearchMethod = SearchMethods.FindObjectOfType)]
    private readonly Canvas canvas;

    [Inject(SearchMethod = SearchMethods.FindObjectsOfTypeIncludeInactive)]
    private RectTransform[] rectTransforms;

    // Inject property
    [Inject]
    private DemoBinder dependencyInjectionDemoBinder { get; set; }

    // Inject field. Binding this instance is done lazy, so it will not be loaded if not injected here.
    // Try it: Remove the [Inject] annotation and see if the instance is still created.
    [Inject]
    private LazyInjectionDemo lazyInjectionDemo;

    // Inject can be optional. This means the value may be null.
    [Inject(Optional = true)]
    private UnityEngine.UI.Image optionalImage;

    [Inject(SearchMethod = SearchMethods.GetComponentInChildren, Optional = true)]
    private Text uiText;

    // Inject property using a specific key instead of the type.
    [Inject(Key = "author")]
    private string NameOfAuthor { get; set; }

    // Inject VisualElement by name (using '#' as prefix) or by class (using '.' as prefix).
    // By default, the VisualElement will be searched starting from the UIDocument,
    // that is tagged as "UIDocument" (see SceneInjectionManager).
    // To use another root VisualElement for the search, set Injector.RootVisualElement.
    [Inject(Key = "#theLabel")]
    private UnityEngine.UIElements.Label theUxmlLabel;

    [Inject(Key = ".theLabelClass")]
    private UnityEngine.UIElements.Label theUxmlLabel2;

    // UxmlName will prefix the value with '#' and use it as the key.
    [Inject(UxmlName = "theLabel")]
    private UnityEngine.UIElements.Label theUxmlLabel3;

    // UxmlClass will prefix the value with '.' and use it as the key.
    [Inject(UxmlClass = "theLabelClass")]
    private UnityEngine.UIElements.Label theUxmlLabel4;

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
        SceneInjectionManager sceneInjectionManager = FindObjectOfType<SceneInjectionManager>();

        Debug.Log("Parent: " + Parent);
        Debug.Log("Child: " + child);
        string childrenCsv = string.Join(", ", children.Select(it => it.ToString()));
        Debug.Log("Children: " + children + " with elements: " + childrenCsv);
        string rectTransformsCsv = string.Join(", ", rectTransforms.Select(it => it.ToString()));
        Debug.Log("RectTransforms: " + rectTransforms + " with elements: " + rectTransformsCsv);
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

        Debug.Log("The bound int: " + sceneInjectionManager.SceneInjector.GetValueForInjectionKey<int>());
        Debug.Log("The bound instance of an interface: " + sceneInjectionManager.SceneInjector.GetValueForInjectionKey<IDemoInterface>());

        Debug.Log("theUxmlLabel.text: " + theUxmlLabel.text);
        Debug.Log("theUxmlLabel2.text: " + theUxmlLabel2.text);
        Debug.Log("theUxmlLabel3.text: " + theUxmlLabel3.text);
        Debug.Log("theUxmlLabel4.text: " + theUxmlLabel4.text);

        Debug.Log("Injector:" + injector);
    }
}

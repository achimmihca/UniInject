
[![Travis Build Status](https://travis-ci.org/achimmihca/UniInject.svg?branch=main)](https://travis-ci.org/achimmihca/UniInject)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/ebc1ffb98b524a38a9a5f93aa7254246)](https://www.codacy.com/gh/achimmihca/UniInject/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=achimmihca/UniInject&amp;utm_campaign=Badge_Grade)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/achimmihca/UniInject/blob/main/LICENSE)

# UniInject

Dependency Injection for Unity3D.

## Why Dependency Injection?
- Better separation of concerns and loose coupling, which leads to composable software and code reuse
- Better testability
- Less boilerplate code to get instances

Dependency injection (DI) is a general concept in object oriented programming.

If you are new to the topic, then I recommend you [this introduction](https://www.codementor.io/@copperstarconsulting/dependency-injection-a-gentle-introduction-697qjipog).

You might also be interested in the [introduction from the Zenject library](https://github.com/modesttree/Zenject#theory), which is another DI library for Unity.

## Why UniInject?
- The same Inject-annotation can be used to get an instance
    - from custom bindings
    - from components (e.g. GetComponent, GetComponentInChildren)
    - from VisualElements (when using Unity's new UIToolkit / UXML / UIDocument)
- Field, property, method and constructor injection
- Cyclic dependencies are handled (except for constructor injection)
- Optional injection
    - Marking something as optional will not throw an Exception when no value is present
- Custom key for injection
    - The default is the type of the field that should be injected
- Hierarchy of different injection contexts (e.g. for Player1 and Player2)
- Scene injection is finished after the Awake() method, such that the injected values can be used in the Start() and OnEnable() methods for further setup logic
- The values of GetComponent, GetComponentInChildren, etc. are mockable.
    - Thus, for tests the scene hierarchy can be simulated.
- Custom bindings can be created using normal MonoBehaviours that implement the IBinder interface
- Static validation in Edit Mode that there is a value for every symbol, which should be injected
- Mark fields that are set in the inspector via the InjectedInInspector-annotation.
    - It makes the origin of values easier to grasp.
    - The static validation can check that a non-null value has been set in such an annotated field.
- Calling injection methods is also possible in edit-mode (e.g. calling SceneInjectionManager.DoInjection())
- UniInject provides you with tools for DI that you can adapt for your own needs.
    - Build upon the given logic to change when, how, and what is injected.
    - The included SceneInjectionManager is a good starting point for inspiration.

## Other Dependency Injection Libraries for Unity3D
Before setting for a DI library, also check out these projects
- [Zenject](https://github.com/modesttree/Zenject)
- [adic](https://github.com/intentor/adic/)
- [RapidIoC](https://github.com/cpgames/RapidIoC)

# How to Use

## Get the Package
- You can add a dependency to your `Packages/manifest.json`  using a [Git URL](https://docs.unity3d.com/Documentation/Manual/upm-git.html) in the following form:
  `"com.achimmihca.uniinject": "https://github.com/achimmihca/UniInject.git?path=UniInject/Packages/com.achimmihca.uniinject#v1.0.0"`
    - Note that `#v1.0.0` can be used to specify a tag or commit hash.
- This package ships with a sample that can be imported to your project using Unity's Package Manager.

## SceneInjectionManager
The SceneInjectionManager is taking care of finding IBinder instances in the scene and injecting the bound objects into all scripts that implement the INeedInjection interface.
This is done in Awake(), such that injection is complete when the Start() method is entered:

- SceneInjectionManager.Awake()
    - Analyze the scene to find binders, scripts that need injection, and listeners
    - Create bindings
    - Perform injection
    - Notify listeners

Note that injection of the scene is done after binding. Thus, an IBinder cannot use injected fields to create new bindings.
```
public class MyCoolSceneControl : MonoBehaviour, IBinder
{
    [Inject]
    private SettingsManager settingsManager;
    
    public List<IBinding> GetBindings()
    {
        BindingBuilder bb = new BindingBuilder();
        // The following will not work, because settingsManager has not yet been injected
        // when GetBindings() is called by the SceneInjectionManager.
        bb.BindExistingInstance(settingsManager.Settings);
        return bb.GetBindings();
    }
}
```

### Custom SceneInjectionManager
You can write a SceneInjectionManager for your own needs to change when, how, and what is injected.

### ISceneInjectionFinishedListener / OnSceneInjectionFinished
After injection of the scene is complete, the SceneInjectionManager notifies all instances of ISceneInjectionFinishedListener.
This will be done before any Start() method is called by Unity.

## Get an instance that has been bound
```
...
using UniInject;

public class MyCoolScript2 : MonoBehaviour, INeedInjection
{
    [Inject]
    private SceneNavigator sceneNavigator;

    [Inject(optional = true)]
    private MyCoolButOptionalScript myCoolButOptionalScript;

    [Inject(key = "myCustomKey")]
    private int foo;

    [Inject(searchMethod = SearchMethods.GetComponent)]
    private RectTransform rectTransform;

    void Start() {
        // Do something with the injected instances.
    }
}
```

## Get a VisualElement (when using UIToolkit)
VisualElements can be searched by name (using a string as key with prefix '#') or by class (using a string as key with prefix '.')

```
...
using UniInject;
using UniInject.UIElements;

public class DialogControl : INeedInjection, IInjectionFinishedListener
{
    [Inject(key = "#theButtonName")]
    private Button theButton;

    [Inject(key = ".theLabelClass")]
    private Label theLabel;

    public void OnInjectionFinished() {
        // Do something with the injected instances.
    }
}
```

VisualElements are searched from the Injector's RootVisualElement.

- This field is set by the SceneInjectionManager to the VisualElement of the UIDocument that is tagged "UIDocument".
- This field can be set manually. This way it is possible to inject instances from any VisualElement, for example a dialog that is created at runtime:
    ```
    var uxmlDialogInstance = uxmlDialog.CloneTree();
    sceneInjector.WithRootVisualElement(uxmlDialogInstance).Inject(dialogControlInstance);
    ```

## Binding an instance
```
...
using UniInject;

public class MyCoolSceneControl : MonoBehaviour, IBinder
{
    [InjectedInInspector]
    public SongAudioPlayer songAudioPlayer;
    
    public List<IBinding> GetBindings()
    {
        BindingBuilder bb = new BindingBuilder();
        bb.BindExistingInstance(songAudioPlayer);
        bb.BindExistingInstance(this);
        return bb.GetBindings();
    }
}
```

## Inject scripts that are created at runtime
```
...
using UniInject;

public class MyCoolScriptThatInstantiatesAnotherScript : MonoBehaviour, INeedInjection
{
    [InjectedInInspector]
    public AnotherScript anotherScriptPrefab;

    // The SceneInjectionManager is binding the SceneInjector itself.
    [Inject]
    private Injector injector;
    
    public void InstantiateSomeOtherMonoBehaviour()
    {
        // this.transform will be the parent transform of the newly created instance.
        AnotherScript anotherScript = Instantiate(anotherScriptPrefab, this.transform);
        injector.Inject(anotherScript);
    }
}
```

## Mock Unity Search Methods
```
UniInjectUtils.GlobalInjector.MockUnitySearchMethod(scriptInstance, SearchMethods.GetComponentInChildren, new MockupImplementation());
```

## Verify Scene
The Menu Item **UniInject > Check current scene** will perform the following checks:

- There is a binding for every value that should be injected
- Fields marked with [InjectedInInspector] actually have a value

# Digging deeper
The [tests](https://github.com/achimmihca/UniInject/tree/main/UniInject/Packages/com.achimmihca.uniinject/Tests/Editor) for UniInject are a good way to get an idea what can and cannot be done using UniInject.

# Contributing
See the wiki page: https://github.com/achimmihca/UniInject/wiki/Contributing

# History
UniInject has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play).
If you like singing, karaoke, or SingStar then go check it out ;)

If you are interested in a bit of history on DI and the approach taken by other (Java) libs then I recommend you the first minutes of [this talk](https://www.youtube.com/watch?v=oK_XtfXPkqw) from Google Developers on YouTube.
The talk covers a bit of Spring and XML bean definitions, Google Guice (UniInject and Zenject are similar to Guice), and Dagger 2.
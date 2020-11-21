
[![Travis Build Status](https://travis-ci.org/achimmihca/UniInject.svg?branch=main)](https://travis-ci.org/achimmihca/UniInject)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/ebc1ffb98b524a38a9a5f93aa7254246)](https://www.codacy.com/gh/achimmihca/UniInject/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=achimmihca/UniInject&amp;utm_campaign=Badge_Grade)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/achimmihca/UniInject/blob/main/LICENSE)

# Introduction

Dependency injection (DI) is a general concept in object oriented programming.

If you are new to the topic, then I recommend you [this introduction](https://www.codementor.io/@copperstarconsulting/dependency-injection-a-gentle-introduction-697qjipog).

You might also be interested in the [introduction from the Zenject library](https://github.com/modesttree/Zenject#theory), which is another DI library for Unity.

If you are interested in a bit of history on DI and the approach taken by other (Java) libs then I recommend you the first minutes of [this talk](https://www.youtube.com/watch?v=oK_XtfXPkqw) from Google Developers on YouTube.
The talk covers a bit of Spring and XML bean definitions, Google Guice (UniInject and Zenject are similar to Guice), and Dagger 2.

# Why UniInject

- Less boilerplate code to get instances
- Better testability
- The same approach (the Inject-annotation) can be used to get an instance via Unity search methods (e.g. GetComponent, GetComponentInChildren) or from custom bindings
- Binding values can be done using normal MonoBehaviours that implement the IBinder interface (examples below)
- Injection is finished after the Awake() method, such that the injected values can be used in the Start() and OnEnable() methods for setup logic
- Hierarchy of different injection contexts (e.g. for Player1 and Player2)
- The values of GetComponent, GetComponentInChildren, etc. are mockable.
    - Thus, for tests the scene hierarchy can be simulated.
- Manual injection of newly created instances
- Field, property, method and constructor injection
- Cyclic dependencies are handled (except for constructor injection)
- Optional injection
    - Marking something as optional will not throw an Exception when no value is present
- Custom key for injection
    - The default is the type of the field that should be injected
- Static check in Edit Mode that there is a value for every symbol, which should be injected
    - Mark fields that are set in the inspector via the InjectedInInspector-annotation.
    This will check that a value has actually been provided (throw if null).
    Furthermore it makes origin of values easier to grasp.
- UniInject provides you with tools for DI that you can adapt for your own needs.
    - Build upon the given logic to change when, how, and what is injected.
    - The included SceneInjectionManager is a good starting point for inspiration.

The following is **not supported** (yet):

- Injection during Edit Mode

# Other Dependency Injection Libraries for Unity3D

Before setting for a DI library, also check out these projects
- [Zenject](https://github.com/modesttree/Zenject)
- [adic](https://github.com/intentor/adic/)
- [RapidIoC](https://github.com/cpgames/RapidIoC)

# Demo

Clone this repo, open the Unity project, and take a look at the demo scene.

# How to use

## SceneInjectionManager

The SceneInjectionManager is taking care of finding IBinder instances in the scene and injecting the bound objects into all scripts that implement the INeedInjection interface. This is done in Awake(), such that injection is complete when the Start() method is entered:

- SceneInjectionManager.Awake()
    - Analyze the scene to find binders, scripts that need injection, and listeners
    - Create bindings
    - Perform injection
    - Notify listeners

Note that injection of the scene is done after binding. Thus, an IBinder cannot use injected fields to create new bindings.
```
public class MyCoolSceneController : MonoBehaviour, IBinder
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

You can write a SceneInjectionManager for your own needs.

For example, one could create a marker interface for MonoBehaviours to bind these instances automatically.
This way, you could just add the marker interface to a class and then inject its (singleton) instance where needed.

### ISceneInjectionFinishedListener / OnSceneInjectionFinished

After injection of the scene is complete, the SceneInjectionManager notifies all instances of ISceneInjectionFinishedListener.
This will be done before any Start() method is called by Unity.

## Get an instance that has been bound

```
...
using UniInject;

public class MyCoolScript2 : INeedInjection
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

## Binding an instance

```
...
using UniInject;

public class MyCoolSceneController : MonoBehaviour, IBinder
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

See also the [demo binder](https://github.com/achimmihca/UniInject/blob/main/UniInject/Assets/Scenes/DependencyInjectionDemo/DependencyInjectionDemoBinder.cs).

## Inject scripts that are created at runtime

```
...
using UniInject;

public class MyCoolScriptThatInstantiatesAnotherScript : MonoBehaviour
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
GlobalInjector.MockUnitySearchMethod(scriptInstance, SearchMethods.GetComponentInChildren, new MockupImplementation());
```

## Verify Scene

The Menu Item **UniInject > Check current scene** will perform the following checks:

- There is a binding for every value that should be injected
- Fields marked with [InjectedInInspector] actually have a value

# Digging deeper

The [tests](https://github.com/achimmihca/UniInject/tree/main/UniInject/Assets/Editor/Tests) for UniInject are a good way to get an idea what can and cannot be done using UniInject.

# Contributing

See the wiki page: https://github.com/achimmihca/UniInject/wiki/Contributing

# History

UniInject has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play).
If you like singing, karaoke, or SingStar then go check it out ;)

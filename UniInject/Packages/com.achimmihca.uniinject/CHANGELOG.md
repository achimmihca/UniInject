# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [5.0.0] - TBA
- Injector injects itself if no other binding found
- Always do injection on newly created instances
- Added extension method to create child injector
- Added Unity's component search methods for collections (e.g. GetComponentsInChildren, FindObjectsOfType)
- Search in parent injector if no VisualElement found
- Option to disable search in parent injector

## [4.0.0] - 2022-01-16
- Allow injection of list of VisualElement
- Better error messages for nested exceptions
- Moved supplementary AddBinding method to extension methods

## [3.0.0] - 2021-11-13
- Dedicated properties 'UxmlName' and 'UxmlClass' in Inject-attribute
- Inject-attribute uses properties in PascalCase 

## [2.0.0] - 2021-11-08
- Collect multiple exceptions during injection of an object and re-throw as one single exception
- Still inject fields where possible if there are exceptions on some other fields

## [1.0.0] - 2021-04-17
- Field, property, method and constructor injection
- Cyclic dependencies are handled (except for constructor injection)
- Optional injection
- ISceneInjectionFinishedListener and IInjectionFinishedListener interfaced to react to injection
- The same Inject-annotation can be used to get an instance via Unity search methods (e.g. GetComponent, GetComponentInChildren) or from custom bindings
- Binding values can be done using normal MonoBehaviours that implement the IBinder interface
- Injection is finished after the Awake() method, such that the injected values can be used in the Start() and OnEnable() methods for further setup logic
- Injectors form a hierarchy with different injection contexts (e.g. for Player1 and Player2)
- The values of GetComponent, GetComponentInChildren, etc. are mockable.
- Static check of current scene in edit-mode that no binding is missing
- InjectedInInspector-annotation
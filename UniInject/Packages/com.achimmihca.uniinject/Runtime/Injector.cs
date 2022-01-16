﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniInject
{
    public class Injector
    {
        public const string RootVisualElementInjectionKey = "rootVisualElement";

        // The parent Injector.
        // If a binding is not found in this Injector, then it is searched in the parent injectors recursively.
        public Injector ParentInjector { get; private set; }

        private readonly Dictionary<object, IProvider> injectionKeyToProviderMap = new Dictionary<object, IProvider>();

        private readonly HashSet<Type> getValuesForConstructorInjectionVisitedTypes = new HashSet<Type>();
        private readonly Dictionary<object, object> injectionKeyToObjectWithOngoingInjectionMap = new Dictionary<object, object>();

        private readonly List<UnitySearchMethodMockup> unitySearchMethodMockups = new List<UnitySearchMethodMockup>();

        internal Injector(Injector parent)
        {
            this.ParentInjector = parent;
        }

        internal object Create(Type type)
        {
            object newInstance;
            object[] constructorParameters = GetValuesForConstructorInjection(type);
            if (constructorParameters == null)
            {
                newInstance = Activator.CreateInstance(type);
            }
            else
            {
                newInstance = Activator.CreateInstance(type, constructorParameters);
            }
            return newInstance;
        }

        public T Create<T>()
        {
            T newInstance = (T)Create(typeof(T));
            return newInstance;
        }

        public T CreateAndInject<T>()
        {
            T newInstance = Create<T>();
            Inject(newInstance);
            return newInstance;
        }

        public T GetValueForInjectionKey<T>()
        {
            object result = GetValueForInjectionKey(typeof(T));
            if (result is T t)
            {
                return t;
            }
            throw new InjectionException($"Cannot create instance of type {typeof(T)}. No binding found.");
        }

        public object GetValueForInjectionKey(object injectionKey)
        {
            IProvider provider = GetProvider(injectionKey);

            // A provider that creates new instances must be able
            // to resolve constructor parameters from the injector's context.
            object result = provider.Get(this, out bool resultNeedsInjection);

            // If the result is newly created, then it has to be injected as well.
            if (resultNeedsInjection)
            {
                Inject(result, injectionKey);
            }

            return result;
        }

        public void Inject(object target)
        {
            Inject(target, target.GetType());
        }

        private void Inject(object target, object injectionKey)
        {
            // For circular dependencies, the object that is currently created for the injectionKey is stored temporarily.
            // The map is prioritized when resolving dependencies.
            // Thus, further newly created objects (i.e. dependencies of the result that is constructed here)
            // that have a dependency to injectionKey (i.e. to the result that is constructed here),
            // can have the object injected that has already been instantiated here.
            injectionKeyToObjectWithOngoingInjectionMap.Add(injectionKey, target);

            // Find all members to be injected via reflection.
            List<InjectionData> injectionDatas = UniInjectUtils.GetInjectionDatas(target.GetType());

            // Inject existing bindings into the fields.
            List<InjectionException> exceptionList = null;
            foreach (InjectionData injectionData in injectionDatas)
            {
                try
                {
                    Inject(target, injectionData);
                }
                catch (InjectionException ex)
                {
                    if (exceptionList == null)
                    {
                        exceptionList = new List<InjectionException>();
                    }
                    exceptionList.Add(ex);
                }
            }

            injectionKeyToObjectWithOngoingInjectionMap.Remove(injectionKey);

            // Throw all missing binding issues in a single exception.
            if (exceptionList != null)
            {
                throw new InjectionException($"Failed injection of object {target}"
                                             + $" of type {target.GetType()}:", exceptionList);
            }

            // Notify target that its injection is now finished.
            if (target is IInjectionFinishedListener)
            {
                (target as IInjectionFinishedListener).OnInjectionFinished();
            }
        }

        public void Inject(object target, InjectionData injectionData)
        {
            // Check if the injection key is a USS Selector to query a VisualElement
            if (IsVisualElementQuery(injectionData))
            {
                InjectMemberFromUiDocument(target, injectionData.MemberInfo, injectionData.InjectionKeys[0] as string, injectionData.isOptional);
            }
            else if (injectionData.searchMethod == SearchMethods.SearchInBindings)
            {
                InjectMemberFromBindings(target, injectionData.MemberInfo, injectionData.InjectionKeys, injectionData.isOptional);
            }
            else if (target is MonoBehaviour)
            {
                InjectMemberFromUnitySearchMethod(target as MonoBehaviour, injectionData.MemberInfo, injectionData.searchMethod, injectionData.isOptional);
            }
            else
            {
                throw new InjectionException($"Cannot perform injection via {injectionData.searchMethod} into an object of type {target.GetType()}."
                    + " Only MonoBehaviour instances are supported.");
            }
        }

        private bool IsVisualElementQuery(InjectionData injectionData)
        {
            return injectionData.InjectionKeys.Length == 1
                && injectionData.InjectionKeys[0] is string injectionKeyString
                && (injectionKeyString.StartsWith("#") || injectionKeyString.StartsWith("."))
                && ((injectionData.MemberInfo is FieldInfo fieldInfo && typeof(VisualElement).IsAssignableFrom(fieldInfo.FieldType))
                    || (injectionData.MemberInfo is PropertyInfo propertyInfo && typeof(VisualElement).IsAssignableFrom(propertyInfo.PropertyType)));
        }

        private void InjectMemberFromUiDocument(object target, MemberInfo memberInfo, string injectionKeyString, bool isOptional)
        {
            if (!TryGetRootVisualElement(out VisualElement rootVisualElement))
            {
                throw new InjectionException($"Attempt to inject VisualElement for key '{injectionKeyString}'" +
                                             $" but no binding for string '{RootVisualElementInjectionKey}' " +
                                             $" or for UIDocument has been set.");
            }

            string elementName = injectionKeyString.StartsWith("#")
                ? injectionKeyString.Substring(1)
                : null;
            string elementClassName = injectionKeyString.StartsWith(".")
                ? injectionKeyString.Substring(1)
                : null;
            VisualElement visualElement = rootVisualElement.Q<VisualElement>(elementName, elementClassName);

            try
            {
                if (visualElement != null)
                {
                    if (memberInfo is FieldInfo)
                    {
                        (memberInfo as FieldInfo).SetValue(target, visualElement);
                    }
                    else if (memberInfo is PropertyInfo)
                    {
                        (memberInfo as PropertyInfo).SetValue(target, visualElement);
                    }
                    else
                    {
                        throw new InjectionException($"Only Fields and Properties are supported for injection of VisualElements but got {memberInfo.MemberType}.");
                    }
                }
                else if (!isOptional)
                {
                    throw new InjectionException($"No VisualElement found using elementName: {elementName}, className: {elementClassName}");
                }
            }
            catch (Exception ex)
            {
                throw new InjectionException($"Cannot inject member {memberInfo.Name} of {target}.", ex);
            }
        }

        private bool TryGetRootVisualElement(out VisualElement rootVisualElement)
        {
            try
            {
                object obj = GetValueForInjectionKey(RootVisualElementInjectionKey);
                if (obj is VisualElement visualElement)
                {
                    rootVisualElement = visualElement;
                    return true;
                }
                if (obj != null)
                {
                    Debug.LogError($"Object for injection key '{RootVisualElementInjectionKey}' was found, but it is not of type VisualElement." +
                                     $" Instead, it is of type: {obj.GetType()}");
                }
            }
            catch (MissingBindingException e)
            {
                // Ignore. Try to find UIDocument and use its rootVisualElement.
            }

            if (TryGetUIDocument(out UIDocument uiDocument))
            {
                rootVisualElement = uiDocument.rootVisualElement;
                return true;
            }
            rootVisualElement = null;
            return false;
        }

        private bool TryGetUIDocument(out UIDocument uiDocument)
        {
            try
            {
                object obj = GetValueForInjectionKey(typeof(UIDocument));
                if (obj is UIDocument uiDoc)
                {
                    uiDocument = uiDoc;
                    return true;
                }
                if (obj != null)
                {
                    Debug.LogError($"Object for injection key typeof(UIDocument) was found, but it is not of type UIDocument." +
                                   $" Instead, it is of type: {obj.GetType()}");
                }
            }
            catch (MissingBindingException e)
            {
                // Ignore. Error is thrown further above.
            }

            uiDocument = null;
            return false;
        }

        private void InjectMemberFromBindings(object target, MemberInfo memberInfo, object[] injectionKeys, bool isOptional)
        {
            object[] valuesToBeInjected;
            try
            {
                if (isOptional)
                {
                    try
                    {
                        valuesToBeInjected = GetValuesForInjectionKeys(injectionKeys, target, target.GetType());
                    }
                    catch (MissingBindingException)
                    {
                        // Ignore because the injection is optional.
                        return;
                    }
                }
                else
                {
                    valuesToBeInjected = GetValuesForInjectionKeys(injectionKeys, target, target.GetType());
                }

                if (valuesToBeInjected == null)
                {
                    throw new InjectionException("No values to be injected.");
                }

                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(target, valuesToBeInjected[0]);
                }
                else if (memberInfo is PropertyInfo)
                {
                    (memberInfo as PropertyInfo).SetValue(target, valuesToBeInjected[0]);
                }
                else if (memberInfo is MethodInfo)
                {
                    (memberInfo as MethodInfo).Invoke(target, valuesToBeInjected);
                }
                else
                {
                    throw new InjectionException($"Only Fields, Properties and Methods are supported for injection.");
                }
            }
            catch (Exception e)
            {
                throw new InjectionException($"Cannot inject {target}.{memberInfo.Name}: " + e.Message, e);
            }
        }

        private void InjectMemberFromUnitySearchMethod(MonoBehaviour script, MemberInfo memberInfo, SearchMethods searchMethod, bool isOptional)
        {
            Type componentType = ReflectionUtils.GetTypeOfFieldOrProperty(script, memberInfo);

            // For testing, searching in the scene hierarchy using a Unity method can be simulated to return a mockup for a component.
            object component = GetComponentFromUnitySearchMethodMockups(script, searchMethod, componentType);
            if (component == null)
            {
                // No mockup found, thus use the real Unity search method.
                component = UniInjectUtils.InvokeUnitySearchMethod(script, searchMethod, componentType);
            }

            if (component != null)
            {
                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(script, component);
                }
                else if (memberInfo is PropertyInfo)
                {
                    (memberInfo as PropertyInfo).SetValue(script, component);
                }
                else
                {
                    throw new InjectionException($"Cannot inject member {script.name}.{memberInfo}."
                        + $" Only Fields and Properties are supported for component injection via Unity methods.");
                }
            }
            else if (!isOptional)
            {
                throw new InjectionException($"Cannot inject member {script.name}.{memberInfo.Name}."
                    + $" No component of type {componentType} found using method {searchMethod}");
            }
        }

        private object GetComponentFromUnitySearchMethodMockups(MonoBehaviour script, SearchMethods searchMethod, Type componentType)
        {
            foreach (UnitySearchMethodMockup unitySearchMethodMockup in unitySearchMethodMockups)
            {
                Type mockedSearchReturnType = unitySearchMethodMockup.searchResult.GetType();
                bool callingScriptMatches = (unitySearchMethodMockup.callingScript == null || unitySearchMethodMockup.callingScript == script);
                bool searchMethodMatches = (unitySearchMethodMockup.searchMethod == searchMethod);
                bool returnTypeMatches = componentType.IsAssignableFrom(mockedSearchReturnType);
                if (callingScriptMatches && searchMethodMatches && returnTypeMatches)
                {
                    return unitySearchMethodMockup.searchResult;
                }
            }
            return null;
        }

        internal object[] GetValuesForConstructorInjection(Type type)
        {
            if (getValuesForConstructorInjectionVisitedTypes.Contains(type))
            {
                throw new CyclicConstructorDependenciesException($"Circular dependencies in the constructor parameters of type {type}");
            }
            getValuesForConstructorInjectionVisitedTypes.Add(type);

            ConstructorInjectionData constructorInjectionData = UniInjectUtils.GetConstructorInjectionData(type);
            object[] injectionKeys = constructorInjectionData.InjectionKeys;
            object[] result = GetValuesForInjectionKeys(injectionKeys, null, type);

            getValuesForConstructorInjectionVisitedTypes.Remove(type);
            return result;
        }

        private object[] GetValuesForInjectionKeys(object[] injectionKeys, object target, Type targetType)
        {
            if (injectionKeys == null)
            {
                return null;
            }

            object[] valuesToBeInjected = new object[injectionKeys.Length];
            int index = 0;
            foreach (object injectionKey in injectionKeys)
            {
                // Lookup in special map to resolve circular dependencies.
                // It checks if there is already an object for the injectionKey that has been instantiated, but is currently injected with its own dependencies.
                bool valueToBeInjectedFound = injectionKeyToObjectWithOngoingInjectionMap.TryGetValue(injectionKey, out object valueToBeInjected);
                if (!valueToBeInjectedFound)
                {
                    // Get (possibly newly created) instance.
                    valueToBeInjected = GetValueForInjectionKey(injectionKey);
                }

                if (valueToBeInjected == null)
                {
                    if (target != null)
                    {
                        throw new MissingBindingException(valueToBeInjected, injectionKey, target);
                    }
                    throw new MissingBindingException(valueToBeInjected, injectionKey, targetType);
                }

                valuesToBeInjected[index] = valueToBeInjected;
                index++;
            }
            return valuesToBeInjected;
        }

        private IProvider GetProvider(object injectionKey)
        {
            injectionKeyToProviderMap.TryGetValue(injectionKey, out IProvider provider);
            if (provider == null && ParentInjector != null)
            {
                provider = ParentInjector.GetProvider(injectionKey);
            }

            if (provider == null)
            {
                throw new MissingBindingException(injectionKey);
            }
            return provider;
        }

        public void AddBinding(IBinding binding,
            RebindingBehavior rebindingBehavior = RebindingBehavior.LogWarning)
        {
            object injectionKey = binding.GetKey();
            if (rebindingBehavior != RebindingBehavior.Ignore
                && injectionKeyToProviderMap.ContainsKey(injectionKey))
            {
                switch (rebindingBehavior)
                {
                    case RebindingBehavior.LogWarning:
                        Debug.LogWarning($"Re-binding of key {injectionKey}");
                        break;
                    case RebindingBehavior.LogError:
                        Debug.LogError($"Re-binding of key {injectionKey}");
                        break;
                    case RebindingBehavior.Throw:
                        throw new RebindingException(injectionKey);
                }
            }
            injectionKeyToProviderMap[injectionKey] = binding.GetProvider();
        }

        public void ClearBindings()
        {
            injectionKeyToProviderMap.Clear();
        }

        public void MockUnitySearchMethod(MonoBehaviour callingScript, SearchMethods searchMethod, object searchResult)
        {
            UnitySearchMethodMockup unitySearchMethodMockup = new UnitySearchMethodMockup(callingScript, searchMethod, searchResult);
            unitySearchMethodMockups.Add(unitySearchMethodMockup);
        }

        private class UnitySearchMethodMockup
        {
            public MonoBehaviour callingScript;
            public SearchMethods searchMethod;
            public object searchResult;

            public UnitySearchMethodMockup(MonoBehaviour callingScript, SearchMethods searchMethod, object mockup)
            {
                this.callingScript = callingScript;
                this.searchMethod = searchMethod;
                this.searchResult = mockup;
            }
        }
    }
}

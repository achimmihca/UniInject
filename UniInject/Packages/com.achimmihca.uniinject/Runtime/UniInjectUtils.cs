using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Ignore warnings about fields that always have the same value.
// The logDebugInfos flag will be set in code if a developer needs it.
#pragma warning disable CS0649

namespace UniInject
{
    public static class UniInjectUtils
    {
        // Set to "true" to see debug output.
        private static readonly bool logDebugInfos;

        // Global injector
        public static Injector GlobalInjector { get; set; } = new Injector(null);

        // Holds information how to instantiate objects of types during Dependency Injection.
        // This includes the parameters that must be resolved to call the constructor.
        private static readonly Dictionary<Type, ConstructorInjectionData> typeToConstructorInjectionDataMap = new Dictionary<Type, ConstructorInjectionData>();

        // Holds information about members (fields, properties, methods) that need injection.
        private static readonly Dictionary<Type, List<InjectionData>> typeToInjectionDatasMap = new Dictionary<Type, List<InjectionData>>();

        public static ConstructorInjectionData GetConstructorInjectionData(Type type)
        {
            bool found = typeToConstructorInjectionDataMap.TryGetValue(type, out ConstructorInjectionData injectionData);
            if (!found)
            {
                injectionData = ReflectionUtils.CreateConstructorInjectionData(type);
                typeToConstructorInjectionDataMap.Add(type, injectionData);
            }
            return injectionData;
        }

        public static List<InjectionData> GetInjectionDatas(Type type)
        {
            bool found = typeToInjectionDatasMap.TryGetValue(type, out List<InjectionData> injectionDatas);
            if (!found)
            {
                if (logDebugInfos)
                {
                    Debug.Log("Loading injection data of type: " + type);
                }
                injectionDatas = ReflectionUtils.CreateInjectionDatas(type);
                typeToInjectionDatasMap.Add(type, injectionDatas);
            }
            return injectionDatas;
        }

        // Creates a new Injector.
        // If no parent is given, then the GlobalInjector is used as parent.
        public static Injector CreateInjector(Injector parent = null)
        {
            if (parent != null)
            {
                return new Injector(parent);
            }
            else
            {
                return new Injector(GlobalInjector);
            }
        }

        public static void LoadInjectionDataForTypesInAssembly(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                // Load the data of the type. The data is stored in a static map.
                GetInjectionDatas(type);
            }
        }

        public static object InvokeUnitySearchMethod(MonoBehaviour script, SearchMethods searchMethod, Type componentType)
        {
            switch (searchMethod)
            {
                case SearchMethods.GetComponent:
                    return script.GetComponent(componentType);

                case SearchMethods.GetComponentInChildren:
                    return script.GetComponentInChildren(componentType);
                case SearchMethods.GetComponentInChildrenIncludeInactive:
                    return script.GetComponentInChildren(componentType, true);
                case SearchMethods.GetComponentsInChildren:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("GetComponentsInChildren")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { script, false });
                case SearchMethods.GetComponentsInChildrenIncludeInactive:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("GetComponentsInChildren")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { script, true });

                case SearchMethods.GetComponentInParent:
                    return script.GetComponentInParent(componentType);
                case SearchMethods.GetComponentInParentIncludeInactive:
                    return script.GetComponentInParent(componentType, true);
                case SearchMethods.GetComponentsInParent:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("GetComponentsInParent")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { script, false });
                case SearchMethods.GetComponentsInParentIncludeInactive:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("GetComponentsInParent")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { script, true });

                case SearchMethods.FindObjectOfType:
                    return GameObject.FindObjectOfType(componentType);
                case SearchMethods.FindObjectOfTypeIncludeInactive:
                    return GameObject.FindObjectOfType(componentType, true);
                case SearchMethods.FindObjectsOfType:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("FindObjectsOfType")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { false });
                case SearchMethods.FindObjectsOfTypeIncludeInactive:
                    ThrowIfNonArrayType(componentType, searchMethod);
                    return typeof(UniInjectGenericMethodHolder)
                        .GetMethod("FindObjectsOfType")
                        .MakeGenericMethod(componentType.GetElementType())
                        .Invoke(script, new object[] { true });
                default:
                    throw new InjectionException($" Unknown Unity search method {searchMethod}");
            }
        }

        private static void ThrowIfNonArrayType(Type type, SearchMethods searchMethod)
        {
            if (!type.IsArray)
            {
                throw new InjectionException("SearchMethod " + searchMethod + " requires an array type to be injected.");
            }
        }

        // Workaround for ambiguous generic method invocation when using reflection:
        // put generic methods that should be called here where there are unambiguous.
        // Methods must be public to be called via reflection but should not be part of public UniInject interface.
        private static class UniInjectGenericMethodHolder
        {
            public static T[] GetComponentsInChildren<T>(Component component, bool includeInactive)
                where T : Component
            {
                return component.GetComponentsInChildren<T>(includeInactive);
            }

            public static T[] GetComponentsInParent<T>(Component component, bool includeInactive)
                where T : Component
            {
                return component.GetComponentsInParent<T>(includeInactive);
            }

            public static T[] FindObjectsOfType<T>(bool includeInactive)
                where T : Component
            {
                return GameObject.FindObjectsOfType<T>(includeInactive);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniInject;
using UniInject.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

// Disable warning about never assigned fields. The values are injected.
#pragma warning disable CS0649

namespace UniInject.Tests
{

    public class ComponentSearchMethodTests
    {
        [Test]
        public void GetComponentInChildrenTest()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            ParentComponentNeedsChildComponent parent = CreateGameObjectWithComponent<ParentComponentNeedsChildComponent>();

            // Inject inactive child
            ChildComponent inactiveChild = CreateGameObjectWithComponent<ChildComponent>();
            inactiveChild.gameObject.SetActive(false);
            inactiveChild.transform.SetParent(parent.transform);

            injector.Inject(parent);
            Assert.IsTrue(parent.inactiveChildComponent == inactiveChild, "inactiveChildComponent not correctly injected");
            Assert.Null(parent.childComponent, "childComponent injected unexpectedly");

            // Inject active child
            ChildComponent child = CreateGameObjectWithComponent<ChildComponent>();
            child.transform.SetParent(parent.transform);
            injector.Inject(parent);
            Assert.IsTrue(parent.childComponent == child, "childComponent not correctly injected");
        }

        [Test]
        public void GetComponentInParentTest()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            ParentComponent parent = CreateGameObjectWithComponent<ParentComponent>();
            parent.gameObject.SetActive(false);

            ChildComponentNeedsParentComponent child = CreateGameObjectWithComponent<ChildComponentNeedsParentComponent>();
            child.transform.SetParent(parent.transform);

            // Inject inactive parent
            injector.Inject(child);
            Assert.IsTrue(child.inactiveParentComponent == parent, "inactiveParentComponent not correctly injected");
            Assert.Null(child.parentComponent, "parentComponent injected unexpectedly");

            // Inject active parent
            parent.gameObject.SetActive(true);
            injector.Inject(child);
            Assert.IsTrue(child.parentComponent == parent, "childComponent not correctly injected");
        }

        [Test]
        public void GetComponentsInChildrenTest()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            ParentComponentNeedsChildComponents parent = CreateGameObjectWithComponent<ParentComponentNeedsChildComponents>();

            // Inject inactive child
            ChildComponent inactiveChild = CreateGameObjectWithComponent<ChildComponent>();
            inactiveChild.gameObject.SetActive(false);
            inactiveChild.transform.SetParent(parent.transform);

            injector.Inject(parent);
            Assert.IsTrue(parent.inactiveChildComponents.Contains(inactiveChild), "inactiveChildComponents not correctly injected");
            Assert.IsEmpty(parent.childComponents, "childComponents injected unexpectedly");

            // Also inject active child
            ChildComponent child = CreateGameObjectWithComponent<ChildComponent>();
            child.transform.SetParent(parent.transform);
            injector.Inject(parent);
            Assert.NotNull(parent.childComponents, "childComponents is null");
            Assert.IsTrue(parent.childComponents.Contains(child), "childComponents not correctly injected");
        }

        [Test]
        public void GetComponentsInParentTest()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            ParentComponent parent = CreateGameObjectWithComponent<ParentComponent>();
            parent.gameObject.SetActive(false);

            ChildComponentNeedsParentComponents child = CreateGameObjectWithComponent<ChildComponentNeedsParentComponents>();
            child.transform.SetParent(parent.transform);

            // Inject inactive parent
            injector.Inject(child);
            Assert.IsTrue(child.inactiveParentComponents.Contains(parent), "inactiveParentComponent not correctly injected");
            Assert.IsEmpty(child.parentComponents, "parentComponent injected unexpectedly");

            // Inject active parent
            parent.gameObject.SetActive(true);
            injector.Inject(child);
            Assert.IsTrue(child.parentComponents.Contains(parent), "childComponent not correctly injected");
        }

        [Test]
        public void GetComponentsInChildrenRequiresArrayTypeTest()
        {
            Injector injector = UniInjectUtils.CreateInjector();
            InvalidParentComponentNeedsChildComponents parent = CreateGameObjectWithComponent<InvalidParentComponentNeedsChildComponents>();
            Assert.Throws<InjectionException>(delegate { injector.Inject(parent); });
        }

        private static T CreateGameObjectWithComponent<T>()
            where T : MonoBehaviour
        {
            return new GameObject().AddComponent<T>();
        }

        private class ParentComponentNeedsChildComponent : MonoBehaviour
        {
            [Inject(SearchMethod = SearchMethods.GetComponentInChildren, Optional = true)]
            public ChildComponent childComponent;

            [Inject(SearchMethod = SearchMethods.GetComponentInChildrenIncludeInactive, Optional = true)]
            public ChildComponent inactiveChildComponent;
        }

        private class ParentComponentNeedsChildComponents : MonoBehaviour
        {
            [Inject(SearchMethod = SearchMethods.GetComponentsInChildren, Optional = true)]
            public ChildComponent[] childComponents;

            [Inject(SearchMethod = SearchMethods.GetComponentsInChildrenIncludeInactive, Optional = true)]
            public ChildComponent[] inactiveChildComponents;
        }

        private class ChildComponentNeedsParentComponent : MonoBehaviour
        {
            [Inject(SearchMethod = SearchMethods.GetComponentInParent, Optional = true)]
            public ParentComponent parentComponent;

            [Inject(SearchMethod = SearchMethods.GetComponentInParentIncludeInactive, Optional = true)]
            public ParentComponent inactiveParentComponent;
        }

        private class ChildComponentNeedsParentComponents : MonoBehaviour
        {
            [Inject(SearchMethod = SearchMethods.GetComponentsInParent, Optional = true)]
            public ParentComponent[] parentComponents;

            [Inject(SearchMethod = SearchMethods.GetComponentsInParentIncludeInactive, Optional = true)]
            public ParentComponent[] inactiveParentComponents;
        }

        private class InvalidParentComponentNeedsChildComponents : MonoBehaviour
        {
            // Not supported, must be an array. Thus, should throw an exception.
            [Inject(SearchMethod = SearchMethods.GetComponentsInChildren, Optional = true)]
            public List<ChildComponent> childComponents;
        }

        private class ParentComponent : MonoBehaviour
        {
        }

        private class ChildComponent : MonoBehaviour
        {
        }
    }

}

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UniInject.Tests
{

    public class UnitySearchMethodTests
    {
        private readonly List<GameObject> createdGameObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            createdGameObjects.ForEach(Object.DestroyImmediate);
        }

        [Test]
        public void ShouldFindComponent()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            GameObject gameObject = new GameObject();
            createdGameObjects.Add(gameObject);
            TextHolder script = gameObject.AddComponent<TextHolder>();
            script.Text = "example";

            ClassThatNeedsInjectionFromSceneHierarchy needsInjection = new();

            // Because the required Unity search methods are not relative from a Component, they can be injected into any class.
            injector.Inject(needsInjection);

            Assert.AreEqual(script.Text, needsInjection.textHolder.Text);
            Assert.AreEqual(script.Text, needsInjection.textHolders[0].Text);

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ShouldFindInactiveGameObject()
        {
            Injector injector = UniInjectUtils.CreateInjector();

            GameObject gameObject = new GameObject();
            createdGameObjects.Add(gameObject);
            TextHolder inactiveScript = gameObject.AddComponent<TextHolder>();
            inactiveScript.Text = "inactive example";
            inactiveScript.gameObject.SetActive(false);

            ClassThatNeedsInjectionFromSceneHierarchyInactive needsInjection = new();

            // Because the required Unity search methods are not relative from a Component, they can be injected into any class.
            injector.Inject(needsInjection);

            Assert.AreEqual(inactiveScript.Text, needsInjection.textHolder.Text);
            Assert.AreEqual(inactiveScript.Text, needsInjection.textHolders[0].Text);

            Object.DestroyImmediate(gameObject);
        }

        public class ClassThatNeedsInjectionFromSceneHierarchy
        {
            [Inject(SearchMethod = SearchMethods.FindObjectOfType)]
            public TextHolder textHolder;

            [Inject(SearchMethod = SearchMethods.FindObjectsOfType)]
            public TextHolder[] textHolders;
        }

        public class ClassThatNeedsInjectionFromSceneHierarchyInactive
        {
            [Inject(SearchMethod = SearchMethods.FindObjectOfTypeIncludeInactive)]
            public TextHolder textHolder;

            [Inject(SearchMethod = SearchMethods.FindObjectsOfTypeIncludeInactive)]
            public TextHolder[] textHolders;
        }

        /////////////////////////////////////////////////////
        public class TextHolder : MonoBehaviour
        {
            public string Text { get; set; }
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using UniInject;
using UniInject.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Disable warning about never assigned fields. The values are injected.
#pragma warning disable CS0649

namespace UniInject.Tests
{
    public class InjectionErrorMessageTests
    {
        [Test]
        public void TellTypeIfBindingMismatch()
        {
            BindingBuilder bb = new BindingBuilder();
            bb.Bind(typeof(GameObject)).ToExistingInstance(bb);

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindings(bb);

            try
            {
                NeedsInjectionWithWrongType needsInjection = new NeedsInjectionWithWrongType();
                injector.Inject(needsInjection);
                Assert.Fail("Expected InjectionException");
            }
            catch (InjectionException e)
            {
                bool messageIsOk = e.Message.Contains("GameObject")
                                   && e.Message.Contains("BindingBuilder");
                Debug.Log(e);
                Assert.True(messageIsOk, "Error message does not tell desired and given type of binding." +
                                  " Original error message has been logged to console.");
            }
        }

        [Test]
        public void TellTypeIfBindingMismatchForVisualElement()
        {
            string uxmlFilePath =
                "Packages/com.achimmihca.uniinject/Tests/Editor/UniInjectEditorTestUxmlFile.uxml";
            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlFilePath);
            if (visualTreeAsset == null)
            {
                throw new UnityException("No visualTreeAsset set");
            }

            VisualElement rootVisualElement = visualTreeAsset.CloneTree()
                .Children()
                .FirstOrDefault();

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindingForInstance(Injector.RootVisualElementInjectionKey, rootVisualElement);

            try
            {
                NeedsVisualElementWithWrongType needsInjection = new NeedsVisualElementWithWrongType();
                injector.Inject(needsInjection);
                Assert.Fail("Expected InjectionException");
            }
            catch (InjectionException e)
            {
                bool messageIsOk = e.Message.Contains("Button")
                                   && e.Message.Contains("Label");
                Debug.Log(e);
                Assert.True(messageIsOk, "Error message does not tell desired and given type of binding." +
                                  " Original error message has been logged to console.");
            }
        }

        public class NeedsInjectionWithWrongType
        {
            [Inject]
            public GameObject gameObject;
        }

        public class NeedsVisualElementWithWrongType
        {
            [Inject(UxmlName = "theLabel")]
            public Button button;
        }
    }
}

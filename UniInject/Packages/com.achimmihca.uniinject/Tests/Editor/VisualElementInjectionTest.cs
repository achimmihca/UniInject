using System.Collections.Generic;
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
    public class VisualElementInjectionTest
    {
        [Test]
        public void InjectVisualElementByNameAndClass()
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

            NeedsVisualElements needsInjection = new NeedsVisualElements();
            injector.Inject(needsInjection);
            Assert.NotNull(needsInjection.label);
            Assert.NotNull(needsInjection.button);
            Assert.NotNull(needsInjection.dropdownField);
        }

        public class NeedsVisualElements
        {
            [Inject(UxmlName = "theLabel")]
            public Label label;

            [Inject(UxmlName = "theButton")]
            public Button button;

            [Inject(UxmlClass = "testDropdownClass")]
            public DropdownField dropdownField;
        }
    }
}

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
        private const string UxmlFilePath = "Packages/com.achimmihca.uniinject/Tests/Editor/UniInjectEditorTestUxmlFile.uxml";

        [Test]
        public void InjectVisualElementByNameAndClass()
        {
            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlFilePath);
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
            Assert.NotNull(needsInjection.label1);
            Assert.NotNull(needsInjection.Label2);

            Assert.NotNull(needsInjection.dropdownField1);
            Assert.NotNull(needsInjection.DropdownField2);

            Assert.NotNull(needsInjection.toggles1);
            Assert.AreEqual(3, needsInjection.toggles1.Count, "Unexpected list elements (toggles1)");
            Assert.NotNull(needsInjection.Toggles2);
            Assert.AreEqual(3, needsInjection.Toggles2.Count, "Unexpected list elements (toggles2)");
        }

        [Test]
        public void InjectVisualElementFromParentInjectorTest()
        {
            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlFilePath);
            if (visualTreeAsset == null)
            {
                throw new UnityException("No visualTreeAsset set");
            }
            VisualElement rootVisualElement = visualTreeAsset.CloneTree()
                .Children()
                .FirstOrDefault();

            Button button = rootVisualElement.Q<Button>("theButton");

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindingForInstance(Injector.RootVisualElementInjectionKey, rootVisualElement);

            Injector childInjector = injector.CreateChildInjector()
                .WithRootVisualElement(button);

            NeedsLabel needsLabel1 = new NeedsLabel();
            childInjector.Inject(needsLabel1);
            Assert.NotNull(needsLabel1.label);

            // Disable search in parent injector
            NeedsLabel needsLabel2 = new NeedsLabel();
            childInjector.SearchInParentInjector = false;
            Assert.Throws<InjectionException>(delegate { childInjector.Inject(needsLabel2); });
        }

        private class NeedsLabel
        {
            [Inject(UxmlName = "theLabel")]
            public Label label;
        }

        private class NeedsVisualElements
        {
            [Inject(Key = "#theLabel")]
            public Label label1;

            [Inject(UxmlName = "theLabel")]
            public Label Label2 { get; set; }

            [Inject(UxmlClass = "testDropdownClass")]
            public DropdownField dropdownField1;

            [Inject(Key = ".testDropdownClass")]
            public DropdownField DropdownField2 { get; set; }

            [Inject(UxmlClass = "testToggle")]
            public List<Toggle> toggles1;

            [Inject(Key = ".testToggle")]
            public List<Toggle> Toggles2 { get; set; }
        }
    }
}

using NUnit.Framework;
using UniInject;
using UniInject.Extensions;

// Disable warning about never assigned fields. The values are injected.
#pragma warning disable CS0649

namespace UniInject.Tests
{
    public class InjectInjectorTest
    {
        [Test]
        public void InjectorInjectsItself()
        {
            Injector parentInjector = UniInjectUtils.CreateInjector();
            Injector childInjector = UniInjectUtils.CreateInjector(parentInjector);

            NeedsInjector needsInjectionFromParentInjector = new NeedsInjector();
            parentInjector.Inject(needsInjectionFromParentInjector);
            Assert.IsTrue(parentInjector == needsInjectionFromParentInjector.theInjector, "parent injector test case error");

            NeedsInjector needsInjectionFromChildInjector = new NeedsInjector();
            childInjector.Inject(needsInjectionFromChildInjector);
            Assert.IsTrue(childInjector == needsInjectionFromChildInjector.theInjector, "child injector test case error");

            NeedsInjector needsInjectionFromParentInjectorViaChildInjector = new NeedsInjector();
            childInjector.AddBindingForInstance(parentInjector);
            childInjector.Inject(needsInjectionFromParentInjectorViaChildInjector);
            Assert.IsTrue(parentInjector == needsInjectionFromParentInjectorViaChildInjector.theInjector, "parent injector via child injector test case error");
        }

        private class NeedsInjector : INeedInjection
        {
            [Inject]
            public Injector theInjector;
        }
    }

}

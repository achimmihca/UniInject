using System;
using NUnit.Framework;
using UniInject;
using UniInject.Extensions;

// Disable warning about never assigned fields. The values are injected.
#pragma warning disable CS0649

namespace UniInject.Tests
{

    public class InjectionExceptionTests
    {
        [Test]
        public void CollectAllInjectionExceptionsTest()
        {
            BindingBuilder bb = new BindingBuilder();
            bb.Bind("someKey").ToExistingInstance("someStringValue");

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindings(bb);

            ScriptThanNeedsInjectionWithMissingBindings needsInjection = new ScriptThanNeedsInjectionWithMissingBindings();
            try
            {
                injector.Inject(needsInjection);
            }
            catch (InjectionException ex)
            {
                // All non-injected field names and missing bindings should be named at once
                Assert.True(ex.Message.Contains("someMissingKey1"));
                Assert.True(ex.Message.Contains("someMissingString1"));
                Assert.True(ex.Message.Contains("someMissingKey2"));
                Assert.True(ex.Message.Contains("someMissingString2"));
                // The available values should still be injected
                Assert.AreEqual("someStringValue", needsInjection.someString);
                // Everything OK
                return;
            }
            Assert.Fail("No exception was thrown");
        }

        private class ScriptThanNeedsInjectionWithMissingBindings : INeedInjection
        {
            [Inject(Key = "someMissingKey1")]
            public string someMissingString1;

            [Inject(Key = "someMissingKey2")]
            public string someMissingString2;

            [Inject(Key = "someKey")]
            public string someString;
        }
    }

    public class RebindingBehaviorTests
    {
        [TearDown]
        public void TearDown()
        {
            UniInjectUtils.DefaultRebindingBehavior = DefaultRebindingBehavior.Ignore;
        }

        [Test]
        public void GlobalDefaultRebindingBehaviorThrowTakesEffectOnAddBinding()
        {
            UniInjectUtils.DefaultRebindingBehavior = DefaultRebindingBehavior.Throw;

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindingForInstance("value1");

            Assert.Throws<RebindingException>(() =>
            {
                injector.AddBindingForInstance("value2");
            });
        }

        [Test]
        public void ExplicitRebindingBehaviorOverridesGlobalDefault()
        {
            UniInjectUtils.DefaultRebindingBehavior = DefaultRebindingBehavior.Throw;

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindingForInstance("value1");

            // Explicit Ignore overrides global Throw
            Assert.DoesNotThrow(() =>
            {
                injector.AddBindingForInstance("value2", RebindingBehavior.Ignore);
            });
            Assert.AreEqual("value2", injector.GetValueForInjectionKey<string>());

            UniInjectUtils.DefaultRebindingBehavior = DefaultRebindingBehavior.Ignore;

            // Explicit Throw overrides global Ignore
            Assert.Throws<RebindingException>(() =>
            {
                injector.AddBindingForInstance("value3", RebindingBehavior.Throw);
            });
        }

        [Test]
        public void GlobalDefaultRebindingBehaviorIgnoreDoesNotThrow()
        {
            UniInjectUtils.DefaultRebindingBehavior = DefaultRebindingBehavior.Ignore;

            Injector injector = UniInjectUtils.CreateInjector();
            injector.AddBindingForInstance("value1");

            Assert.DoesNotThrow(() =>
            {
                injector.AddBindingForInstance("value2");
            });
            Assert.AreEqual("value2", injector.GetValueForInjectionKey<string>());
        }
    }
}

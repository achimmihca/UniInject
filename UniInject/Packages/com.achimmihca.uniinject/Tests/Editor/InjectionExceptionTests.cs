using System;
using NUnit.Framework;
using UniInject;

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
            [Inject(key = "someMissingKey1")]
            public string someMissingString1;

            [Inject(key = "someMissingKey2")]
            public string someMissingString2;

            [Inject(key = "someKey")]
            public string someString;
        }
    }

}

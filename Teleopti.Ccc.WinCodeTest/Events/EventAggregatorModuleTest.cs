using Autofac;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;

namespace Teleopti.Ccc.WinCodeTest.Events
{
    public class EventAggregatorModuleTest
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new EventAggregatorModule());
            container = containerBuilder.Build();
        }

        [Test]
        public void VerifyGlobalSame()
        {
            Assert.AreSame(container.Resolve<IEventAggregatorLocator>().GlobalAggregator(), container.Resolve<IEventAggregatorLocator>().GlobalAggregator());
        }

        [Test]
        public void VerifyGlobalSameIfDifferentScope()
        {
            IEventAggregator agg1, agg2;
            using (container.BeginLifetimeScope())
                agg1 = container.Resolve<IEventAggregatorLocator>().GlobalAggregator();
            using (container.BeginLifetimeScope())
                agg2 = container.Resolve<IEventAggregatorLocator>().GlobalAggregator();
            Assert.AreSame(agg1, agg2);
        }

        [Test]
        public void VerifyLocalSame()
        {
            Assert.AreSame(container.Resolve<IEventAggregatorLocator>().LocalAggregator(), container.Resolve<IEventAggregatorLocator>().LocalAggregator());
        }

        [Test]
        public void VerifyLocalSameIfBothInnerScope()
        {
            IEventAggregator agg1, agg2;
            using (container.BeginLifetimeScope())
            {
                agg1 = container.Resolve<IEventAggregatorLocator>().LocalAggregator();
                agg2 = container.Resolve<IEventAggregatorLocator>().LocalAggregator();
            }
            Assert.AreSame(agg1, agg2);
        }

        [Test]
        public void VerifyLocalNotSameIfDifferentScope()
        {
            IEventAggregator agg1, agg2;
            using (var inner = container.BeginLifetimeScope())
                agg1 = inner.Resolve<IEventAggregatorLocator>().LocalAggregator();
            using (var inner = container.BeginLifetimeScope())
                agg2 = inner.Resolve<IEventAggregatorLocator>().LocalAggregator();
            Assert.AreNotSame(agg1, agg2);
        }
    }
}
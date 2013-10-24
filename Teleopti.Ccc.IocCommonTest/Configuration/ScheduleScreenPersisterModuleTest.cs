using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    [TestFixture]
    public class ScheduleScreenPersisterModuleTest
    {
        private ContainerBuilder _containerBuilder;
        private ScheduleScreenPersisterModule _module;
        private IContainer _container;

        [SetUp]
        public void Setup()
        {
            _module = new ScheduleScreenPersisterModule();
            _containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new AuthenticationModule());
			_containerBuilder.RegisterModule(new UnitOfWorkModule());
			_containerBuilder.RegisterModule(new RepositoryModule());
			_containerBuilder.RegisterModule(_module);
            _container = _containerBuilder.Build();
        }

		 [Test]
		 public void ShouldNotReuseBatchPersister()
		 {
			 var mocks = new MockRepository();

			 var instance1 = _container.Resolve<IScheduleDictionaryBatchPersister>(
				TypedParameter.From(mocks.Stub<IMessageBrokerIdentifier>()),
				TypedParameter.From(mocks.Stub<IOwnMessageQueue>())
				);

			 var instance2 = _container.Resolve<IScheduleDictionaryBatchPersister>(
				TypedParameter.From(mocks.Stub<IMessageBrokerIdentifier>()),
				TypedParameter.From(mocks.Stub<IOwnMessageQueue>())
				);

		 	instance1.Should().Not.Be.SameInstanceAs(instance2);
		 }
    }
}

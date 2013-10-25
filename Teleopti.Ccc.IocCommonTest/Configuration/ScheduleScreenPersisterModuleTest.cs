using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Configuration;

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

    }
}

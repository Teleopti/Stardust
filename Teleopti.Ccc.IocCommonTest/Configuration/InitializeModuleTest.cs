using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class InitializeModuleTest
	{
		private ContainerBuilder containerBuilder;
		private IDataSourceConfigurationSetter dataSourceConfigurationSetter;

		[SetUp]
		public void Setup()
		{
			dataSourceConfigurationSetter = MockRepository.GenerateStub<IDataSourceConfigurationSetter>();
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule(new InitializeModule(dataSourceConfigurationSetter));
		}

		[Test]
		public void InitializeApplicationIsWired()
		{
			using (var container = containerBuilder.Build())
			{
				var init = container.Resolve<IInitializeApplication>();
				init.Should().Not.Be.Null();
			}
		}

		[Test]
		public void InitializeApplicationShouldBeSingleton()
		{
			using (var container = containerBuilder.Build())
			{
				var init = container.Resolve<IInitializeApplication>();
				init.Should().Be.SameInstanceAs(container.Resolve<IInitializeApplication>());
			}
		}

		[Test]
		public void DataSourceConfigurationSetterIsWired()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IDataSourceConfigurationSetter>()
					.Should().Be.SameInstanceAs(dataSourceConfigurationSetter);
			}
		}

		[Test]
		public void ConfigReaderIsWired()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IConfigReader>()
					.Should().Not.Be.Null();
			}
		}
	}
}
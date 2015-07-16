using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultipleConfig;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class InitializeModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest());
		}

		[Test]
		public void InitializeApplicationIsWired()
		{
			containerBuilder.Register(c => new ReadDataSourceConfigurationFromNhibFiles(new NhibFilePathFixed(""), new ParseNhibFile())).As<IReadDataSourceConfiguration>();
			using (var container = containerBuilder.Build())
			{
				var init = container.Resolve<IInitializeApplication>();
				init.Should().Not.Be.Null();
			}
		}

		[Test]
		public void InitializeApplicationShouldBeSingleton()
		{
			containerBuilder.Register(c => new ReadDataSourceConfigurationFromNhibFiles(new NhibFilePathFixed(""), new ParseNhibFile())).As<IReadDataSourceConfiguration>();
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
					.Should().Not.Be.Null();
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
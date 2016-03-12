using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		private ICurrentPersistCallbacks persistCallbacks;
		private DefaultDataCreator defaultDataCreator;
		private DataCreator dataCreator;

		[SetUp]
		public void Setup()
		{
			XmlConfigurator.Configure();

			TestSiteConfigurationSetup.Setup(TestSiteConfigurationSetup.PathToIISExpress64);

			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			builder.RegisterModule(new CommonModule(new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args))));
			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterType<TestConfiguration>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			var container = builder.Build();

			persistCallbacks = container.Resolve<ICurrentPersistCallbacks>();
			defaultDataCreator = container.Resolve<DefaultDataCreator>();
			dataCreator = container.Resolve<DataCreator>();

			var dataHash = defaultDataCreator.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Rta");

			var haveDatabases =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabases)
				createDatabase(path, dataHash);

			TestSiteConfigurationSetup.StartApplicationSync();
		}

		private void createDatabase(string path, int dataHash)
		{
			DataSourceHelper.CreateDatabases();

			StateHolderProxyHelper.SetupFakeState(
				DataSourceHelper.CreateDataSource(persistCallbacks),
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit,
				new ThreadPrincipalContext()
				);

			defaultDataCreator.Create();
			dataCreator.Create();

			DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
			DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

			StateHolderProxyHelper.Logout(new ThreadPrincipalContext());
		}

		[TearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}
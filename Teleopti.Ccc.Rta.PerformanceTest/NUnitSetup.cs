using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[SetUp]
		public void Setup()
		{
			XmlConfigurator.Configure();

			TestSiteConfigurationSetup.Setup(TestSiteConfigurationSetup.PathToIISExpress64);

			var defaultData = new DefaultData();
			var dataHash = defaultData.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabackBackupLocation, "Rta");

			var haveDatabase =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabase)
				createDatabase(defaultData, path, dataHash);

			TestSiteConfigurationSetup.StartApplicationSync();
		}

		private static void createDatabase(DefaultData defaultData, string path, int dataHash)
		{
			DataSourceHelper.CreateDatabases();

			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			builder.RegisterModule(new CommonModule(new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args))));
			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<TestConfiguration>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			var container = builder.Build();

			var persistCallbacks = container.Resolve<ICurrentPersistCallbacks>();
			var currentUnitOfWorkFactory = container.Resolve<ICurrentUnitOfWorkFactory>();
			var currentUnitOfWork = container.Resolve<ICurrentUnitOfWork>();

			StateHolderProxyHelper.SetupFakeState(
				DataSourceHelper.CreateDataSource(persistCallbacks),
				DefaultPersonThatCreatesDbData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnitFromFakeState,
				new ThreadPrincipalContext()
				);

			var dataFactory = new DataFactory(action =>
			{
				using (currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					action.Invoke(currentUnitOfWork);
					currentUnitOfWork.Current().PersistAll();
				}
			});
			defaultData.ForEach(dataFactory.Apply);

			container.Resolve<DataCreator>().Create();

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
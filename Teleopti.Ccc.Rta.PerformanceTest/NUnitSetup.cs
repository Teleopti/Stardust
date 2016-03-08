using System.Configuration;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
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

			if (new ConfigReader().ReadValue("SetThisToTrueAndYouHaveToWaitFor1Point5HoursThenErikKillsYou", false))
			{
				DataSourceHelper.CreateDatabases();

				TestSiteConfigurationSetup.StartApplicationAsync();

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

				var datasource = DataSourceHelper.CreateDataSource(container.Resolve<ICurrentPersistCallbacks>());

				StateHolderProxyHelper.SetupFakeState(
					datasource,
					DefaultPersonThatCreatesDbData.PersonThatCreatesDbData,
					DefaultBusinessUnit.BusinessUnitFromFakeState,
					new ThreadPrincipalContext()
					);
				GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

				var defaultData = new DefaultData();
				defaultData.ForEach(dataSetup => GlobalDataMaker.Data().Apply(dataSetup));

				container.Resolve<DataCreator>().Create();

			}
			else
			{
				TestSiteConfigurationSetup.StartApplicationAsync();
			}

		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}
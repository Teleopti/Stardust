using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

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

			DataSourceHelper.CreateDatabases();
			TestSiteConfigurationSetup.StartApplicationAsync();

			var datasource = DataSourceHelper.CreateDataSource(SystemSetup.PersistCallbacks);

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesDbData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnitFromFakeState,
				new ThreadPrincipalContext()
				);
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

			var defaultData = new DefaultData();
			defaultData.ForEach(dataSetup => GlobalDataMaker.Data().Apply(dataSetup));
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}
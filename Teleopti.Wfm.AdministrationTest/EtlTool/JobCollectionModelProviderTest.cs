using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.AdministrationTest.FakeData;

namespace Teleopti.Wfm.AdministrationTest.EtlTool
{
	[AdministrationTest]
	public class JobCollectionModelProviderTest
	{
		public JobCollectionModelProvider Target;
		public FakeTenants AllTenants;
		public FakeBaseConfigurationRepository BaseConfigurationRepository;
		public IGeneralInfrastructure GeneralInfrastructure;
		public IConfigurationHandler ConfigurationHandler;
		public FakePmInfoProvider PmInfoProvider;


		[Test]
		public void ShouldReturnJobCollection()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = Target.Create("Tenant");

			result.Count.Should().Be.GreaterThanOrEqualTo(13);
		}


	}
}

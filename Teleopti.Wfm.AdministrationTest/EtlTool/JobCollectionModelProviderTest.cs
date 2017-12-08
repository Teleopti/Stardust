using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.Administration.Models;
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

		[Test]
		public void ShouldReturnIfJobNeedsParameterDataSource()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = Target.Create("Tenant");

			result.First(x => x.JobName == "Initial").NeedsParameterDataSource.Should().Be(false);
			result.First(x => x.JobName == "Nightly").NeedsParameterDataSource.Should().Be(true);
		}

		[Test]
		public void ShouldReturnDatePeriodCollectionRequiredForJobs()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = Target.Create("Tenant");

			result.First(x => x.JobName == "Initial").NeededDatePeriod.Count.Should().Be(1);
			result.First(x => x.JobName == "Initial").NeededDatePeriod.Contains("Initial");
			result.First(x => x.JobName == "Permission").NeededDatePeriod.Count.Should().Be(0);
			result.First(x => x.JobName == "Nightly").NeededDatePeriod.Count.Should().Be(5);
		}


	}
}

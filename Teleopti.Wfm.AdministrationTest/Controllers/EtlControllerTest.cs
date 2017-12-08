using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.Administration.Models;
using Teleopti.Wfm.AdministrationTest.FakeData;

namespace Teleopti.Wfm.AdministrationTest.Controllers
{
	[AdministrationTest]
	public class EtlControllerTest : ISetup
	{
		public EtlController Target;
		public FakeToggleManager ToggleManager;
		public JobCollectionModelProvider JobCollectionModelProvider;
		public TenantLogDataSourcesProvider TenantLogDataSourcesProvider;
		public FakeTenants AllTenants;
		public FakeBaseConfigurationRepository BaseConfigurationRepository;
		public FakeGeneralInfrastructure GeneralInfrastructure;
		public IConfigurationHandler ConfigurationHandler;
		public FakePmInfoProvider PmInfoProvider;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<EtlController>();
		}

		[Test]
		public void ShouldReturnJobCollection()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>)Target.Jobs("Tenant");
			result.Content.Count.Should().Be.GreaterThanOrEqualTo(13);
		}

		[Test]
		public void ShouldReturnNotFoundTenantForJobs()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (NegotiatedContentResult<string>)Target.Jobs("TenantNotFound");
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldReturnIfJobNeedsParameterDataSource()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>)Target.Jobs("Tenant");

			result.Content.First(x => x.JobName == "Initial").NeedsParameterDataSource.Should().Be(false);
			result.Content.First(x => x.JobName == "Nightly").NeedsParameterDataSource.Should().Be(true);
		}

		[Test]
		public void ShouldReturnDatePeriodCollectionRequiredForJobs()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>)Target.Jobs("Tenant");

			result.Content.First(x => x.JobName == "Initial").NeededDatePeriod.Count.Should().Be(1);
			result.Content.First(x => x.JobName == "Initial").NeededDatePeriod.Contains("Initial");
			result.Content.First(x => x.JobName == "Permission").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Nightly").NeededDatePeriod.Count.Should().Be(5);
		}

		[Test]
		public void ShouldReturnTenantLogDataSources()
		{
			const string connectionString = "Server=.;DataBase=a";
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 15, false));

			var result = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantLogDataSources("Tenant");
			result.Content.Count.Should().Be(1);
			result.Content.First().Id.Should().Be(3);
			result.Content.First().Name.Should().Be("myDs");
		}

		[Test]
		public void ShouldReturnNotFoundTenantForDataSources()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (NegotiatedContentResult<string>)Target.TenantLogDataSources("TenantNotFound");
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

	}
}

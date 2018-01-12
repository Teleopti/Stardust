using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
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
		public FakeJobScheduleRepository JobScheduleRepository;
		public MutableNow Now;


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
			result.Content.First(x => x.JobName == "Initial").NeededDatePeriod.Should().Contain("Initial");
			result.Content.First(x => x.JobName == "Intraday").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Nightly").NeededDatePeriod.Count.Should().Be(4);
			result.Content.First(x => x.JobName == "Nightly").NeededDatePeriod.Should().Not.Contain("Initial");
			result.Content.First(x => x.JobName == "Workload Queues").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Queue Statistics").NeededDatePeriod.Count.Should().Be(1);
			result.Content.First(x => x.JobName == "Queue Statistics").NeededDatePeriod.Should().Contain("QueueStatistics");
			result.Content.First(x => x.JobName == "Agent Statistics").NeededDatePeriod.Count.Should().Be(1);
			result.Content.First(x => x.JobName == "Schedule").NeededDatePeriod.Count.Should().Be(1);
			result.Content.First(x => x.JobName == "Forecast").NeededDatePeriod.Count.Should().Be(1);
			result.Content.First(x => x.JobName == "Permission").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Person Skill").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "KPI").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Queue and Agent login synchronization").NeededDatePeriod.Count.Should().Be(0);
			result.Content.First(x => x.JobName == "Cleanup").NeededDatePeriod.Count.Should().Be(0);
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

		[Test]
		public void ShouldSaveManualJobScheduleForSingleTenant()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "W. Europe Standard Time", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			
			var localToday = new DateTime(2017, 12, 11);
			var utcToday = TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			Now.Is(utcToday);

			var jobToEnqueue = new JobEnqueModel
			{
				JobName = "Initial",
				JobPeriods = new List<JobPeriod>{new JobPeriod
				{
					Start = utcToday.AddDays(-1),
					End = utcToday.AddDays(1),
					JobCategoryName = "Initial",
				}},
				LogDataSourceId = -2,
				TenantName = "Tenant"
			};
			var result = (OkResult)Target.EnqueueJob(jobToEnqueue);

			var scheduledJob = JobScheduleRepository.GetEtlJobSchedules().First();
			var scheduledPeriods = JobScheduleRepository.GetEtlJobSchedulePeriods(1);
			result.Should().Be.OfType<OkResult>();
			scheduledJob.JobName.Should().Be("Initial");
			scheduledJob.ScheduleName.Should().Be("Manual ETL");
			scheduledJob.DataSourceId.Should().Be(-2);
			scheduledJob.Enabled.Should().Be(true);
			scheduledJob.ScheduleId.Should().Be(1);
			scheduledJob.ScheduleType.Should().Be(JobScheduleType.Manual);
			scheduledJob.Description.Should().Be("Manual ETL");
			scheduledPeriods.Count.Should().Be(1);
			scheduledPeriods.First().JobCategoryName.Should().Be(nameof(JobCategoryType.Initial));
			scheduledPeriods.First().RelativePeriod.Minimum.Should().Be(-1);
			scheduledPeriods.First().RelativePeriod.Maximum.Should().Be(1);
		}

		[Test]
		public void ShouldReturnNotFoundTenantForEnqueueJob()
		{
			const string connectionString = "Server=.;DataBase=a";
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			var result = (NegotiatedContentResult<string>) Target.EnqueueJob(new JobEnqueModel(){ TenantName = "TenantNotFound" });
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}


	}
}

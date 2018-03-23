using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
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
		private const string testTenantName = "Test Tenant";
		private const string connectionString = "Server=.;DataBase=a";
		private const string timezoneName = "W. Europe Standard Time";
		private const string reloadDatamartJobName = "Reload datamart (old nightly)";
		private const string processCubeJobName = "Process Cube";
		private const int basicJobCount = 14;

		public EtlController Target;
		public FakeToggleManager ToggleManager;
		public JobCollectionModelProvider JobCollectionModelProvider;
		public TenantLogDataSourcesProvider TenantLogDataSourcesProvider;
		public FakeTenants AllTenants;
		public FakeBaseConfigurationRepository BaseConfigurationRepository;
		public FakeGeneralInfrastructure GeneralInfrastructure;
		public FakeConfigurationHandler ConfigurationHandler;
		public FakeConfigReader ConfigReader;
		public FakePmInfoProvider PmInfoProvider;
		public FakeJobScheduleRepository JobScheduleRepository;
		public MutableNow Now;
		public FakeConfigReader FakeConfigReader;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<EtlController>();
			system.UseTestDouble<FakeConfigurationHandler>().For<IConfigurationHandler>();
		}

		[Test]
		public void ShouldReturnJobCollection()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigurationHandler.AddBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>) Target.Jobs(testTenantName);
			result.Content.Count.Should().Be.GreaterThanOrEqualTo(13);
		}

		[Test]
		public void ShouldReturnNotFoundTenantForJobs()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			var result = (NegotiatedContentResult<string>) Target.Jobs("TenantNotFound");
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldReturnIfJobNeedsParameterDataSource()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigurationHandler.AddBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>) Target.Jobs(testTenantName);

			result.Content.First(x => x.JobName == "Initial").NeedsParameterDataSource.Should().Be(false);
			result.Content.First(x => x.JobName == "Nightly").NeedsParameterDataSource.Should().Be(true);
		}

		[Test]
		public void ShouldReturnDatePeriodCollectionRequiredForJobs()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigurationHandler.AddBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			var result = (OkNegotiatedContentResult<IList<JobCollectionModel>>) Target.Jobs(testTenantName);

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
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 15, false));

			var result = (OkNegotiatedContentResult<IList<DataSourceModel>>) Target.TenantLogDataSources(testTenantName);
			result.Content.Count.Should().Be(1);
			result.Content.First().Id.Should().Be(3);
			result.Content.First().Name.Should().Be("myDs");
		}

		[Test]
		public void ShouldReturnNotFoundTenantForDataSources()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			var result = (NegotiatedContentResult<string>) Target.TenantLogDataSources("TenantNotFound");
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldEnqueueInitialJob()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday = TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);

			var jobToEnqueue = new JobEnqueModel
			{
				JobName = "Initial",
				JobPeriods = new List<JobPeriod>
				{
					new JobPeriod
					{
						Start = utcToday.AddDays(-1),
						End = utcToday.AddDays(1),
						JobCategoryName = "Initial",
					}
				},
				LogDataSourceId = -2,
				TenantName = testTenantName
			};
			var result = (OkResult) Target.EnqueueJob(jobToEnqueue);

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
			scheduledJob.TenantName.Should().Be(testTenantName);
			scheduledPeriods.Count.Should().Be(1);
			scheduledPeriods.First().JobCategoryName.Should().Be("Initial");
			scheduledPeriods.First().RelativePeriod.Minimum.Should().Be(-1);
			scheduledPeriods.First().RelativePeriod.Maximum.Should().Be(1);
		}

		[Test]
		public void ShouldReturnNotFoundTenantForEnqueueJob()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			var result = (NegotiatedContentResult<string>) Target.EnqueueJob(new JobEnqueModel {TenantName = "TenantNotFound"});
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldEnqueueIntradayJobWithTwoJobCategoryPeriods()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday =
				TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);

			var jobToEnqueue = new JobEnqueModel
			{
				JobName = "Intraday",
				JobPeriods = new List<JobPeriod>(),
				LogDataSourceId = -2,
				TenantName = testTenantName
			};
			var result = (OkResult) Target.EnqueueJob(jobToEnqueue);

			var scheduledPeriods = JobScheduleRepository.GetEtlJobSchedulePeriods(1);
			result.Should().Be.OfType<OkResult>();
			scheduledPeriods.Count.Should().Be(2);
			scheduledPeriods.Any(x => x.JobCategoryName == "Agent Statistics"
									  && x.RelativePeriod.Minimum == 0
									  && x.RelativePeriod.Maximum == 0)
				.Should().Be.True();
			scheduledPeriods.Any(x => x.JobCategoryName == "Queue Statistics"
									  && x.RelativePeriod.Minimum == 0
									  && x.RelativePeriod.Maximum == 0)
				.Should().Be.True();
		}
		[Test]
		public void ShouldCheckThatMasterTenantIsConfigured()
		{
			var masterTenant = new Tenant(RandomName.Make());
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			masterTenant.DataSourceConfiguration.SetApplicationConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			AllTenants.HasWithAppConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.ApplicationConnectionString);

			FakeConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			FakeConfigReader.FakeConnectionString("Tenancy", masterTenant.DataSourceConfiguration.ApplicationConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			var result = (OkNegotiatedContentResult<TenantConfigurationModel>)Target.IsBaseConfigurationAvailable();
			result.Content.IsBaseConfigured.Should().Be(true);
			result.Content.TenantName.Should().Be(masterTenant.Name);
		}

		[Test]
		public void ShouldCheckThatMasterTenantIsNotConfigured()
		{
			FakeConfigReader.FakeConnectionString("Hangfire", connectionString);
			var result = (OkNegotiatedContentResult<TenantConfigurationModel>)Target.IsBaseConfigurationAvailable();
			result.Content.IsBaseConfigured.Should().Be(false);
		}

		[Test]
		public void ShouldSaveBaseConfigurationForTenant()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			var baseConfig = new BaseConfiguration(1053, 15, timezoneName, false);
			var tenantConfig = new TenantConfigurationModel()
			{
				TenantName = testTenantName,
				BaseConfig = baseConfig
			};
			Target.SaveConfigurationForTenant(tenantConfig);
			var savedConfig = BaseConfigurationRepository.LoadBaseConfiguration(connectionString);
			savedConfig.IntervalLength.Should().Be(baseConfig.IntervalLength);
			savedConfig.CultureId.Should().Be(baseConfig.CultureId);
			savedConfig.TimeZoneCode.Should().Be(baseConfig.TimeZoneCode);
		}

		[Test]
		public void ShouldReturnErrorMessageWhenTenantCouldNotBeFoundOnSaveBaseConfiguration()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			var baseConfig = new BaseConfiguration(1053, 15, timezoneName, false);
			var tenantConfig = new TenantConfigurationModel
			{
				TenantName = RandomName.Make(),
				BaseConfig = baseConfig
			};
			var result = (NegotiatedContentResult<string>)Target.SaveConfigurationForTenant(tenantConfig);
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldReturnAllTenants()
		{
			var masterTenant = new Tenant(RandomName.Make());
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			var childTenant = new Tenant(RandomName.Make());
			childTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");

			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			AllTenants.HasWithAnalyticsConnectionString(childTenant.Name, childTenant.DataSourceConfiguration.AnalyticsConnectionString);

			var baseConfig = new BaseConfiguration(1053, 15, timezoneName, false);
			ConfigurationHandler.AddBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString, baseConfig);

			var result = (OkNegotiatedContentResult<List<TenantConfigurationModel>>)Target.GetTenants();
			result.Should().Be.OfType<OkNegotiatedContentResult<List<TenantConfigurationModel>>>();

			var mTenant = result.Content.Single(x => x.TenantName == masterTenant.Name);
			var cTenant = result.Content.Single(x => x.TenantName == childTenant.Name);

			mTenant.TenantName.Should().Be(masterTenant.Name);
			mTenant.BaseConfig.IntervalLength.Should().Be(baseConfig.IntervalLength);
			mTenant.BaseConfig.CultureId.Should().Be(baseConfig.CultureId);
			mTenant.BaseConfig.TimeZoneCode.Should().Be(baseConfig.TimeZoneCode);
			mTenant.IsBaseConfigured.Should().Be(true);

			cTenant.TenantName.Should().Be(childTenant.Name);
			cTenant.BaseConfig.IntervalLength.Should().Be(null);
			cTenant.BaseConfig.CultureId.Should().Be(null);
			cTenant.BaseConfig.TimeZoneCode.Should().Be(null);
			cTenant.IsBaseConfigured.Should().Be(false);
		}

		[Test]
		public void ShouldReturnConfigurationOption()
		{
			var models = (OkNegotiatedContentResult<TenantConfigurationOption>)Target.GetConfigurationModel();
			models.Content.CultureList.Should().Not.Be.Empty();
			models.Content.IntervalLengthList.Should().Not.Be.Empty();
			models.Content.TimeZoneList.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPersistDataSourceConfiguration()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 1, "UTC", 15, false));

			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSource = new DataSourceModel
				{
					Id = dataSourceId,
					TimeZoneId = 13
				}
			};
			Target.PersistDataSource(tenantDataSource);
			var dataSources = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantLogDataSources(testTenantName);
			dataSources.Content.Single().TimeZoneId.Should().Be(tenantDataSource.DataSource.TimeZoneId);

			var scheduledJob = JobScheduleRepository.GetEtlJobSchedules().First();
			var scheduledPeriods = JobScheduleRepository.GetEtlJobSchedulePeriods(1);
			scheduledJob.JobName.Should().Be("Initial");
			scheduledJob.ScheduleName.Should().Be("Manual ETL");
			scheduledJob.DataSourceId.Should().Be(1);
			scheduledJob.Enabled.Should().Be(true);
			scheduledJob.ScheduleId.Should().Be(1);
			scheduledJob.ScheduleType.Should().Be(JobScheduleType.Manual);
			scheduledJob.Description.Should().Be("Manual ETL");
			scheduledJob.TenantName.Should().Be(testTenantName);
			scheduledPeriods.Count.Should().Be(1);
			scheduledPeriods.First().JobCategoryName.Should().Be("Initial");
			scheduledPeriods.First().RelativePeriod.Minimum.Should().Be(-1);
			scheduledPeriods.First().RelativePeriod.Maximum.Should().Be(1);
		}

		[Test]
		public void NotAllowToChangeDefaultDataSource()
		{
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(-1, "Not Defined", -1, null, 15, false));

			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSource = new DataSourceModel
				{
					Id = -1,
					TimeZoneId = 13
				}
			};
			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSource);
			result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}

		[Test]
		public void NotAllowToChangeInternalDataSource()
		{
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, $"Raptor{RandomName.Make()}", -1, null, 15, false));

			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSource = new DataSourceModel
				{
					Id = 3,
					TimeZoneId = 13
				}
			};
			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSource);
			result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldReturnErrorMessageWhenDataSourceNotFound()
		{
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);

			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSource = new DataSourceModel
				{
					Id = 3,
					TimeZoneId = 13
				}
			};
			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSource);
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldDoNothingIfNotChangeTimeZoneOfDataSource()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 1, "UTC", 15, false));

			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSource = new DataSourceModel
				{
					Id = dataSourceId,
					TimeZoneId = 15
				}
			};
			Target.PersistDataSource(tenantDataSource);
			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSource);
			result.StatusCode.Should().Be(HttpStatusCode.NotModified);
		}

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ShouldGetJobsForSpecificTenant(bool toggle38131Enabled, bool pmInstalled)
		{
			var jobs = getJobs(testTenantName, toggle38131Enabled, pmInstalled);

			foreach (var job in jobs)
			{
				foreach (var step in job.JobSteps)
				{
					step.DependsOnTenant.Should().Be.False();
				}
			}
		}

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ShouldGetJobsForAllTenant(bool toggle38131Enabled, bool pmInstalled)
		{
			var jobs = getJobs(Tenants.AllTenantName, toggle38131Enabled, pmInstalled);

			foreach (var job in jobs)
			{
				foreach (var step in job.JobSteps)
				{
					var shouldDependsOnTenant = JobCollectionModelProvider.IsJobStepDependsOnTenant(job.JobName, step.Name);
					step.DependsOnTenant.Should().Be(shouldDependsOnTenant);
				}
			}
		}

		private IEnumerable<JobCollectionModel> getJobs(string tenantName, bool toggle38131Enabled, bool pmInstalled)
		{
			ConfigReader.FakeConnectionString("Hangfire", connectionString);


			ConfigurationHandler.AddBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday = TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);

			if (toggle38131Enabled)
			{
				ToggleManager.Enable(Toggles.ETL_SpeedUpNightlyReloadDatamart_38131);
			}

			PmInfoProvider.SetPmInstalled(pmInstalled);

			var result = Target.Jobs(tenantName);
			result.Should().Be.OfType<OkNegotiatedContentResult<IList<JobCollectionModel>>>();

			var jobCollection = result as OkNegotiatedContentResult<IList<JobCollectionModel>>;
			Assert.NotNull(jobCollection);

			var jobs = jobCollection.Content;

			var expectedJobCount = basicJobCount + (toggle38131Enabled ? 1 : 0) + (pmInstalled ? 1 : 0);
			jobs.Count.Should().Be(expectedJobCount);

			jobs.Any(j => j.JobName == reloadDatamartJobName).Should().Be.EqualTo(toggle38131Enabled);
			jobs.Any(j => j.JobName == processCubeJobName).Should().Be.EqualTo(pmInstalled);

			return jobs;
		}
	}
}

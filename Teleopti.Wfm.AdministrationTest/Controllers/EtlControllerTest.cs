using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobHistory;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.Administration.Core.Modules;
using Teleopti.Wfm.Administration.Models;
using Teleopti.Wfm.AdministrationTest.FakeData;

namespace Teleopti.Wfm.AdministrationTest.Controllers
{
	[DomainTest]
	public class EtlControllerTest : IIsolateSystem, IExtendSystem
	{
		private const string testTenantName = "Test Tenant";
		private const string connectionString = "Server=.;DataBase=a";
		private const string timezoneName = "W. Europe Standard Time";
		private const string reloadDatamartJobName = "Reload datamart (old nightly)";
		private const string processCubeJobName = "Process Cube";
		private const int basicJobCount = 14;

		public EtlController Target;
		public FakeToggleManager ToggleManager;
		public FakeTenants AllTenants;
		public FakeBaseConfigurationRepository BaseConfigurationRepository;
		public FakeGeneralInfrastructure GeneralInfrastructure;
		public FakeConfigurationHandler ConfigurationHandler;
		public FakeConfigReader ConfigReader;
		public FakePmInfoProvider PmInfoProvider;
		public FakeJobScheduleRepository JobScheduleRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeConfigReader FakeConfigReader;
		public FakeAnalyticsTimeZoneRepository TimeZoneRepository;
		public FakeJobHistoryRepository JobHistoryRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<EtlController>();
			extend.AddModule(new EtlToolModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeConfigurationHandler>().For<IConfigurationHandler>();
			
			isolate.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
			isolate.UseTestDouble<FakePmInfoProvider>().For<IPmInfoProvider>();
			isolate.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			isolate.UseTestDouble<FakeGeneralInfrastructure>().For<IGeneralInfrastructure>();
			isolate.UseTestDouble<MutableNow>().For<INow, IMutateNow>();
			isolate.UseTestDouble<FakeJobScheduleRepository>().For<IJobScheduleRepository>();
			isolate.UseTestDouble<FakeJobHistoryRepository>().For<IJobHistoryRepository>();
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
			result.Content.First(x => x.JobName == "Nightly").NeededDatePeriod.Count.Should().Be(3);
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
		public void ShouldReturnValidTenantLogDataSources()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 15, false));

			var result = (OkNegotiatedContentResult<IList<DataSourceModel>>) Target.TenantValidLogDataSources(testTenantName);
			result.Content.Count.Should().Be(2);

			var firstDataSource = result.Content.First();
			firstDataSource.Id.Should().Be(-2);
			firstDataSource.Name.Should().Be(Tenants.NameForOptionAll);

			var secondDataSource = result.Content.Second();
			secondDataSource.Id.Should().Be(3);
			secondDataSource.Name.Should().Be("myDs");
		}

		[Test]
		public void ShouldReturnNotFoundTenantForDataSources()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, "UTC", false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var result = (NegotiatedContentResult<string>) Target.TenantValidLogDataSources("TenantNotFound");
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldReturnAllTenantLogDataSources()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 60, false));
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(4, "anotherDs", FakeGeneralInfrastructure.NullTimeZoneId,
				null, 15, false));

			var result = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantAllLogDataSources(testTenantName);
			result.Content.Count.Should().Be(3);

			var myds = result.Content.Single(x => x.Id == 3 && x.Name == "myDs");
			myds.TimeZoneCode.Should().Be("UTC");
			myds.IntervalLength.Should().Be(60);
			myds.IsIntervalLengthSameAsTenant.Should().Be(false);

			var anotherDs = result.Content.Single(x => x.Id == 4 && x.Name == "anotherDs");
			anotherDs.TimeZoneCode.Should().Be(null);
			anotherDs.IntervalLength.Should().Be(15);
			anotherDs.IsIntervalLengthSameAsTenant.Should().Be(true);

			result.Content.Single(x => x.Id == -2 && x.Name == Tenants.NameForOptionAll);
		}

		[Test]
		public void ShouldImportMissingDataSourceAndReturnTenantLogDataSources()
		{
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 15, false));
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(4, "anotherDs", FakeGeneralInfrastructure.NullTimeZoneId,
				null, 15, false));
			GeneralInfrastructure.HasAggDataSources(new DataSourceEtl(4, "anotherDs", 2, null, 15, false));
			GeneralInfrastructure.HasAggDataSources(new DataSourceEtl(89, "newDs", 2, timezoneName, 15, false));

			var result = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantLogDataSources(testTenantName);
			result.Content.Count.Should().Be(3);
			result.Content.Any(x => x.Id == -2 && x.Name == Tenants.NameForOptionAll).Should().Be.False();
			result.Content.Any(x => x.Id == 3 && x.Name == "myDs").Should().Be.True();
			result.Content.Any(x => x.Id == 4 && x.Name == "anotherDs" && x.TimeZoneCode == null)
				.Should().Be.True();
			result.Content.Any(x => x.Name == "newDs" && x.TimeZoneCode == null).Should().Be.True();
		}

		[Test]
		public void ShouldEnqueueInitialJob()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigReader.FakeConnectionString("Hangfire", connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday = TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);

			var jobToEnqueue = new EtlJobEnqueModel
			{
				JobName = "Initial",
				JobPeriods = new List<JobPeriodDate>
				{
					new JobPeriodDate
					{
						Start = localToday.AddDays(-1),
						End = localToday.AddDays(1),
						JobCategoryName = "Initial",
					}
				},
				LogDataSourceId = -2,
				TenantName = testTenantName
			};
			var result = (OkResult) Target.EnqueueJob(jobToEnqueue);

			var scheduledJob = JobScheduleRepository.GetSchedules(null, DateTime.Now).First();
			var scheduledPeriods = JobScheduleRepository.GetSchedulePeriods(1);
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
			var result = (NegotiatedContentResult<string>) Target.EnqueueJob(new EtlJobEnqueModel {TenantName = "TenantNotFound"});
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public void ShouldEnqueueIntradayJobWithTwoJobCategoryPeriods()
		{
			BaseConfigurationRepository.SaveBaseConfiguration(connectionString,
				new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigReader.FakeConnectionString("Hangfire", connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday =
				TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);

			var jobToEnqueue = new EtlJobEnqueModel
			{
				JobName = "Intraday",
				JobPeriods = new List<JobPeriodDate>(),
				LogDataSourceId = -2,
				TenantName = testTenantName
			};
			var result = (OkResult) Target.EnqueueJob(jobToEnqueue);

			var scheduledPeriods = JobScheduleRepository.GetSchedulePeriods(1);
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

			var baseConfig = new BaseConfiguration(1053, 15, timezoneName, false);
			ConfigurationHandler.AddBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString, baseConfig);
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
			TimeZone.IsNewYork();
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			ConfigReader.FakeConnectionString("Hangfire", connectionString);
			var localNow = new DateTime(2018, 3, 28, 1, 0, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(localNow, TimeZone.TimeZone()));

			var baseConfig = new BaseConfiguration(1053, 15, TimeZone.TimeZone().Id, false);
			var tenantConfig = new TenantConfigurationModel()
			{
				TenantName = testTenantName,
				BaseConfig = baseConfig
			};
			Target.SaveConfigurationForTenant(tenantConfig);
			var savedConfig = BaseConfigurationRepository.LoadBaseConfiguration(connectionString);
			var scheduledJob = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			var scheduledPeriods = JobScheduleRepository.GetSchedulePeriods(1);

			savedConfig.IntervalLength.Should().Be(baseConfig.IntervalLength);
			savedConfig.CultureId.Should().Be(baseConfig.CultureId);
			savedConfig.TimeZoneCode.Should().Be(baseConfig.TimeZoneCode);

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
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString, baseConfig);

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
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> {myDsModel}
			};

			Target.PersistDataSource(tenantDataSources);
			var dataSources = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantValidLogDataSources(testTenantName);
			var dataSourceInResultt = dataSources.Content.Single(x=>x.Name== "myDs");
			dataSourceInResultt.TimeZoneCode.Should().Be(myDsModel.TimeZoneCode);

			var scheduledJob = JobScheduleRepository.GetSchedules(null, DateTime.Now).First();
			var scheduledPeriods = JobScheduleRepository.GetSchedulePeriods(1);
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
		public void ShouldEnqueInitialJobFirst()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));

			var period = new DateOnlyPeriod(DateOnly.Today.AddDays(-5), DateOnly.Today.AddDays(15));
			GeneralInfrastructure.HasFactQueuePeriod(dataSourceId, period);
			GeneralInfrastructure.HasFactAgentPeriod(dataSourceId, period);

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			Target.PersistDataSource(tenantDataSources);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.MinValue);
			scheduledJobs.Count.Should().Be(3);
			scheduledJobs.First().JobName.Should().Be("Initial");
		}

		[Test]
		public void ShouldSplitJobsInOneMonthPeriod()
		{
			TimeZone.IsNewYork();
			var localNow = new DateTime(2018, 04, 05, 01, 0, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(localNow, TimeZone.TimeZone()));

			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));

			var period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-40), (new DateOnly(Now.UtcDateTime()).AddDays(40)));
			GeneralInfrastructure.HasFactQueuePeriod(dataSourceId, period);
			GeneralInfrastructure.HasFactAgentPeriod(dataSourceId, period);

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			Target.PersistDataSource(tenantDataSources);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.MinValue);
			var agentJobs = scheduledJobs.Where(x => x.JobName == "Agent Statistics").ToList();
			var queueJobs = scheduledJobs.Where(x => x.JobName == "Queue Statistics").ToList();

			scheduledJobs.Count.Should().Be(7);
			agentJobs.Count.Should().Be(3);
			queueJobs.Count.Should().Be(3);

			var agentScheduledPeriod = JobScheduleRepository.GetSchedules(null, DateTime.MinValue)
				.Where(x => x.JobName == "Agent Statistics").ToList();
			agentScheduledPeriod.First().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(-40);
			agentScheduledPeriod.First().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(-10);
			agentScheduledPeriod.Second().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(-10);
			agentScheduledPeriod.Second().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(20);
			agentScheduledPeriod.Last().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(20);
			agentScheduledPeriod.Last().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(40);

			var queueScheduledPeriod = JobScheduleRepository.GetSchedules(null, DateTime.MinValue)
				.Where(x => x.JobName == "Queue Statistics").ToList();
			queueScheduledPeriod.First().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(-40);
			queueScheduledPeriod.First().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(-10);
			queueScheduledPeriod.Second().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(-10);
			queueScheduledPeriod.Second().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(20);
			queueScheduledPeriod.Last().RelativePeriodCollection.First().RelativePeriod.Minimum.Should().Be(20);
			queueScheduledPeriod.Last().RelativePeriodCollection.First().RelativePeriod.Maximum.Should().Be(40);
		}

		[Test]
		public void ShouldEnqueueQueueStatisticsJobWhenTimeZoneOfDataSourceChanged()
		{
			TimeZone.IsChina();
			var localNow = new DateTime(2018, 3, 28, 01, 0, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(localNow, TimeZone.TimeZone()));

			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));

			var period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-5), (new DateOnly(Now.UtcDateTime()).AddDays(15)));
			GeneralInfrastructure.HasFactQueuePeriod(dataSourceId, period);

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			Target.PersistDataSource(tenantDataSources);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.MinValue);
			scheduledJobs.Count.Should().Be(2);

			scheduledJobs.Any(x => x.JobName == "Initial").Should().Be.True();
			scheduledJobs.Any(x => x.JobName == "Agent Statistics").Should().Be.False();

			var queueStatisticsJob = scheduledJobs.Single(x=>x.JobName == "Queue Statistics");
			queueStatisticsJob.ScheduleName.Should().Be("Manual ETL");
			queueStatisticsJob.DataSourceId.Should().Be(dataSourceId);
			queueStatisticsJob.Enabled.Should().Be(true);
			queueStatisticsJob.ScheduleType.Should().Be(JobScheduleType.Manual);
			queueStatisticsJob.Description.Should().Be("Manual ETL");
			queueStatisticsJob.TenantName.Should().Be(testTenantName);
			
			var scheduledPeriod = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single(x=>x.JobName == "Queue Statistics")
				.RelativePeriodCollection.Single();
			scheduledPeriod.JobCategoryName.Should().Be("Queue Statistics");
			scheduledPeriod.RelativePeriod.Minimum.Should().Be(-5);
			scheduledPeriod.RelativePeriod.Maximum.Should().Be(15);
		}

		[Test]
		public void ShouldEnqueueAgentStatisticsJobWhenTimeZoneOfDataSourceChanged()
		{
			TimeZone.IsChina();
			var localNow = new DateTime(2018, 3, 28, 01, 0, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(localNow, TimeZone.TimeZone()));

			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));

			var period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-5), new DateOnly(Now.UtcDateTime()).AddDays(15));
			GeneralInfrastructure.HasFactAgentPeriod(dataSourceId, period);

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			Target.PersistDataSource(tenantDataSources);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.MinValue);
			scheduledJobs.Count.Should().Be(2);

			scheduledJobs.Any(x => x.JobName == "Initial").Should().Be.True();
			scheduledJobs.Any(x => x.JobName == "Queue Statistics").Should().Be.False();

			var agentStatisticsJob = scheduledJobs.Single(x => x.JobName == "Agent Statistics");
			agentStatisticsJob.ScheduleName.Should().Be("Manual ETL");
			agentStatisticsJob.DataSourceId.Should().Be(dataSourceId);
			agentStatisticsJob.Enabled.Should().Be(true);
			agentStatisticsJob.ScheduleType.Should().Be(JobScheduleType.Manual);
			agentStatisticsJob.Description.Should().Be("Manual ETL");
			agentStatisticsJob.TenantName.Should().Be(testTenantName);

			var scheduledPeriod = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single(x => x.JobName == "Agent Statistics")
				.RelativePeriodCollection.Single();
			scheduledPeriod.JobCategoryName.Should().Be("Agent Statistics");
			scheduledPeriod.RelativePeriod.Minimum.Should().Be(-5);
			scheduledPeriod.RelativePeriod.Maximum.Should().Be(15);
		}

		[Test]
		public void ShouldPersistTimezoneNotExistsInAnalyticsDb()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, null, 15, false));
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId + 1, "anotherDs", 12, null, 15, false));

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				TimeZoneCode = "UTC"
			};
			var anotherDsModel = new DataSourceModel
			{
				Id = dataSourceId + 1,
				TimeZoneCode = timezoneName
			};

			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> {myDsModel, anotherDsModel}
			};

			var result = (OkResult)Target.PersistDataSource(tenantDataSources);
			result.Should().Not.Be.Null();

			var dataSources = (OkNegotiatedContentResult<IList<DataSourceModel>>)Target.TenantValidLogDataSources(testTenantName);
			dataSources.Content.Single(x=>x.Id == myDsModel.Id).TimeZoneCode.Should().Be(myDsModel.TimeZoneCode);
			dataSources.Content.Single(x => x.Id == anotherDsModel.Id).TimeZoneCode.Should().Be(myDsModel.TimeZoneCode);

			var allTimeZones = TimeZoneRepository.GetAll();
			allTimeZones.Count.Should().Be(2);
			allTimeZones.SingleOrDefault(x => x.TimeZoneCode == timezoneName).Should().Not.Be.Null();
			allTimeZones.SingleOrDefault(x => x.TimeZoneCode == "UTC").Should().Not.Be.Null();
		}

		[TestCase(1)]
		[TestCase(-1)]
		public void NotAllowToChangeDefaultDataSource(int dataSourceId)
		{
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "Not Defined", -1, null, 15, false));

			var defaultDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				Name = RandomName.Make(),
				TimeZoneCode = "UTC"
			};
			var anotherDsModel = new DataSourceModel
			{
				Id = dataSourceId + 1,
				Name = RandomName.Make(),
				TimeZoneCode = timezoneName
			};

			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> {defaultDsModel, anotherDsModel}
			};

			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSources);
			result.StatusCode.Should().Be(HttpStatusCode.Ambiguous);
			result.Content.Should().Contain(defaultDsModel.Name);
			JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnErrorMessageWhenDataSourceNotFound()
		{
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);

			var dataSourceModel = new DataSourceModel
			{
				Id = 3,
				Name = RandomName.Make(),
				TimeZoneCode = "UTC"
			};
			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> {dataSourceModel}
			};

			var result = (NegotiatedContentResult<string>) Target.PersistDataSource(tenantDataSource);
			result.StatusCode.Should().Be(HttpStatusCode.Ambiguous);
			result.Content.Should().Contain(dataSourceModel.Name);
			JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Should().Be.Empty();
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
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 13, "UTC", 15, false));

			var dataSourceModel = new DataSourceModel
			{
				Id = dataSourceId,
				Name = RandomName.Make(),
				TimeZoneCode = "UTC"
			};
			var tenantDataSource = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> {dataSourceModel}
			};

			var result = (OkResult)Target.PersistDataSource(tenantDataSource);
			result.Should().Not.Be(null);
			JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Should().Be.Empty();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldGetJobsForSpecificTenant(bool pmInstalled)
		{
			var jobs = getJobs(testTenantName, pmInstalled);

			foreach (var job in jobs)
			{
				foreach (var step in job.JobSteps)
				{
					step.DependsOnTenant.Should().Be.False();
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldGetJobsForAllTenant(bool pmInstalled)
		{
			var jobs = getJobs(Tenants.NameForOptionAll, pmInstalled);

			foreach (var job in jobs)
			{
				foreach (var step in job.JobSteps)
				{
					var shouldDependsOnTenant = JobCollectionModelProvider.IsJobStepDependsOnTenant(job.JobName, step.Name);
					step.DependsOnTenant.Should().Be(shouldDependsOnTenant);
				}
			}
		}

		[Test]
		public void ShouldGetScheduledJobs()
		{
			var dailyJob = new EtlJobSchedule(1, "My Nightly job", true, 60, "Nightly", 1, "Desc", null, 
				new List<IEtlJobRelativePeriod>
				{
					new EtlJobRelativePeriod(new MinMax<int>(-5, 5), JobCategoryType.AgentStatistics),
					new EtlJobRelativePeriod(new MinMax<int>(-4, 4), JobCategoryType.QueueStatistics)
				}
				, "tenant A");
			var periodicJob = new EtlJobSchedule(2, "My Intraday job", true, 30, 120, 1320, "Intraday", 1, "Run Intraday job",
				null, DateTime.Now, new List<IEtlJobRelativePeriod>(), "tenant A");
			var manualJobNotToBeLoaded = new EtlJobSchedule(3, "Manual ETL", "Schedule", true, 1, "Manually enqueued job",
				DateTime.Now, new List<IEtlJobRelativePeriod>(), "tenant A");
			JobScheduleRepository.SaveSchedule(dailyJob);
			JobScheduleRepository.SaveSchedule(periodicJob);
			JobScheduleRepository.SaveSchedule(manualJobNotToBeLoaded);

			var savedScheduledJobs = (OkNegotiatedContentResult<List<EtlScheduleJobModel>>)Target.ScheduledJobs();

			savedScheduledJobs.Should().Be.OfType<OkNegotiatedContentResult<List<EtlScheduleJobModel>>>();
			savedScheduledJobs.Content.Count.Should().Be(2);

			var savedDailyJob = savedScheduledJobs.Content.First();
			savedDailyJob.ScheduleId.Should().Be(dailyJob.ScheduleId);
			savedDailyJob.ScheduleName.Should().Be(dailyJob.ScheduleName);
			savedDailyJob.Description.Should().Be(dailyJob.Description);
			savedDailyJob.JobName.Should().Be(dailyJob.JobName);
			savedDailyJob.Enabled.Should().Be(dailyJob.Enabled);
			savedDailyJob.Tenant.Should().Be(dailyJob.TenantName);
			savedDailyJob.LogDataSourceId.Should().Be(dailyJob.DataSourceId);
			savedDailyJob.DailyFrequencyStart.Should().Be(DateTime.MinValue.AddMinutes(dailyJob.OccursOnceAt));
			savedDailyJob.DailyFrequencyEnd.Should().Be(DateTime.MinValue);
			savedDailyJob.DailyFrequencyMinute.Should().Be(string.Empty);
			savedDailyJob.RelativePeriods.Length.Should().Be(2);
			savedDailyJob.RelativePeriods.First().JobCategoryName.Should().Be(dailyJob.RelativePeriodCollection.First().JobCategory.ToString());
			savedDailyJob.RelativePeriods.First().Start.Should().Be(dailyJob.RelativePeriodCollection.First().RelativePeriod.Minimum);
			savedDailyJob.RelativePeriods.First().End.Should().Be(dailyJob.RelativePeriodCollection.First().RelativePeriod.Maximum);
			savedDailyJob.RelativePeriods.Last().JobCategoryName.Should().Be(dailyJob.RelativePeriodCollection.Last().JobCategory.ToString());
			savedDailyJob.RelativePeriods.Last().Start.Should().Be(dailyJob.RelativePeriodCollection.Last().RelativePeriod.Minimum);
			savedDailyJob.RelativePeriods.Last().End.Should().Be(dailyJob.RelativePeriodCollection.Last().RelativePeriod.Maximum);


			var savedPeriodicJob = savedScheduledJobs.Content.Last();
			savedPeriodicJob.ScheduleId.Should().Be(periodicJob.ScheduleId);
			savedPeriodicJob.ScheduleName.Should().Be(periodicJob.ScheduleName);
			savedPeriodicJob.Description.Should().Be(periodicJob.Description);
			savedPeriodicJob.JobName.Should().Be(periodicJob.JobName);
			savedPeriodicJob.Enabled.Should().Be(periodicJob.Enabled);
			savedPeriodicJob.Tenant.Should().Be(periodicJob.TenantName);
			savedPeriodicJob.DailyFrequencyStart.Should().Be(DateTime.MinValue.AddMinutes(periodicJob.OccursEveryMinuteStartingAt));
			savedPeriodicJob.DailyFrequencyEnd.Should().Be(DateTime.MinValue.AddMinutes(periodicJob.OccursEveryMinuteEndingAt));
			savedPeriodicJob.DailyFrequencyMinute.Should().Be(periodicJob.OccursEveryMinute.ToString());
			savedPeriodicJob.RelativePeriods.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddPeriodicScheduledJob()
		{
			var scheduleModel = new EtlScheduleJobModel
			{
				ScheduleName = "My Schedule data Job",
				Description = "desc",
				JobName = "Schedule",
				Enabled = true,
				DailyFrequencyMinute = "15",
				DailyFrequencyStart = new DateTime(2018, 4, 4, 6, 0, 0),
				DailyFrequencyEnd = new DateTime(2018, 4, 4, 16, 0, 0),
				Tenant = "My tenant",
				LogDataSourceId = 1,
				RelativePeriods = new[]
				{
					new JobPeriodRelative
					{
						JobCategoryName = JobCategoryType.Schedule.ToString(),
						Start = -1,
						End = 1
					}
				}
			};

			var result = Target.ScheduleJob(scheduleModel);
			result.Should().Be.OfType<OkResult>();

			var savedSchedule = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			var savedSchedulePeriod = JobScheduleRepository.GetSchedulePeriods(savedSchedule.ScheduleId).Single();

			savedSchedule.ScheduleId.Should().Be.GreaterThan(0);
			savedSchedule.ScheduleName.Should().Be(scheduleModel.ScheduleName);
			savedSchedule.Description.Should().Be(scheduleModel.Description);
			savedSchedule.JobName.Should().Be(scheduleModel.JobName);
			savedSchedule.Enabled.Should().Be(scheduleModel.Enabled);
			savedSchedule.OccursEveryMinute.Should().Be(scheduleModel.DailyFrequencyMinute);
			savedSchedule.OccursEveryMinuteStartingAt.Should().Be(scheduleModel.DailyFrequencyStart.TimeOfDay.TotalMinutes);
			savedSchedule.OccursEveryMinuteEndingAt.Should().Be(scheduleModel.DailyFrequencyEnd.TimeOfDay.TotalMinutes);
			savedSchedule.TenantName.Should().Be(scheduleModel.Tenant);
			savedSchedule.ScheduleType.Should().Be(JobScheduleType.Periodic);
			savedSchedule.DataSourceId.Should().Be(scheduleModel.LogDataSourceId);

			savedSchedulePeriod.JobCategoryName.Should().Be(scheduleModel.RelativePeriods.Single().JobCategoryName);
			savedSchedulePeriod.RelativePeriod.Minimum.Should().Be(scheduleModel.RelativePeriods.Single().Start);
			savedSchedulePeriod.RelativePeriod.Maximum.Should().Be(scheduleModel.RelativePeriods.Single().End);
		}


		[Test]
		public void ShouldAddDailyScheduledJob()
		{
			var scheduleModel = new EtlScheduleJobModel
			{
				ScheduleName = "My Schedule data Job",
				Description = "desc",
				JobName = "Schedule",
				Enabled = true,
				DailyFrequencyStart = new DateTime(2018, 4, 4, 6, 0, 0),
				Tenant = "My tenant",
				LogDataSourceId = 1,
				RelativePeriods = new []
				{
					new JobPeriodRelative
					{
						JobCategoryName = JobCategoryType.Schedule.ToString(),
						Start = -1,
						End = 1
					}
				}
			};

			var result = Target.ScheduleJob(scheduleModel);
			result.Should().Be.OfType<OkResult>();

			var savedSchedule = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			var savedSchedulePeriod = JobScheduleRepository.GetSchedulePeriods(savedSchedule.ScheduleId).Single();

			savedSchedule.ScheduleId.Should().Be.GreaterThan(0);
			savedSchedule.ScheduleName.Should().Be(scheduleModel.ScheduleName);
			savedSchedule.Description.Should().Be(scheduleModel.Description);
			savedSchedule.JobName.Should().Be(scheduleModel.JobName);
			savedSchedule.Enabled.Should().Be(scheduleModel.Enabled);
			savedSchedule.OccursOnceAt.Should().Be(scheduleModel.DailyFrequencyStart.TimeOfDay.TotalMinutes);
			savedSchedule.TenantName.Should().Be(scheduleModel.Tenant);
			savedSchedule.ScheduleType.Should().Be(JobScheduleType.OccursDaily);
			savedSchedule.DataSourceId.Should().Be(scheduleModel.LogDataSourceId);

			savedSchedulePeriod.JobCategoryName.Should().Be(scheduleModel.RelativePeriods.Single().JobCategoryName);
			savedSchedulePeriod.RelativePeriod.Minimum.Should().Be(scheduleModel.RelativePeriods.Single().Start);
			savedSchedulePeriod.RelativePeriod.Maximum.Should().Be(scheduleModel.RelativePeriods.Single().End);
		}

		[Test]
		public void ShouldSaveIntradayJobWithPeriodsForAgentAndQueueStatistics()
		{
			var scheduleModel = new EtlScheduleJobModel
			{
				JobName = "Intraday"
			};

			var result = Target.ScheduleJob(scheduleModel);
			result.Should().Be.OfType<OkResult>();

			var savedSchedule = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			var savedPeriods = JobScheduleRepository.GetSchedulePeriods(savedSchedule.ScheduleId);

			savedPeriods.Count.Should().Be(2);
			savedPeriods
				.Any(x => x.JobCategory.ToString() == JobCategoryType.AgentStatistics.ToString())
				.Should().Be.True();
			savedPeriods
				.Any(x => x.JobCategory.ToString() == JobCategoryType.QueueStatistics.ToString())
				.Should().Be.True();
			savedPeriods.All(x => x.RelativePeriod.Minimum == 0)
				.Should().Be.True();
			savedPeriods.All(x => x.RelativePeriod.Maximum == 0)
				.Should().Be.True();
		}

		[Test]
		public void ShouldEditDailyScheduledJob()
		{
			var existingSchedule = new EtlJobSchedule(
				3,
				"My Schedule data Job",
				true,
				360,
				"Schedule",
				1,
				"Desc",
				null,
				new List<IEtlJobRelativePeriod>{new EtlJobRelativePeriod(new MinMax<int>(-1, 1), JobCategoryType.Schedule)}, 
				"My tenant");
			JobScheduleRepository.SaveSchedule(existingSchedule);
			JobScheduleRepository.SaveSchedulePeriods(existingSchedule);

			var editedScheduleModel = new EtlScheduleJobModel
			{
				ScheduleId = 3,
				ScheduleName = "name",
				Description = "d",
				JobName = "Forecast",
				Enabled = false,
				DailyFrequencyStart = new DateTime(2018, 4, 4, 10, 0, 0),
				Tenant = "your tenant",
				LogDataSourceId = 2,
				RelativePeriods = new[]
				{
					new JobPeriodRelative
					{
						JobCategoryName = JobCategoryType.Forecast.ToString(),
						Start = -2,
						End = 2
					}
				}
			};

			var result = Target.EditScheduleJob(editedScheduleModel);
			result.Should().Be.OfType<OkResult>();

			var savedSchedule = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			var savedSchedulePeriod = JobScheduleRepository.GetSchedulePeriods(savedSchedule.ScheduleId).Single();

			savedSchedule.ScheduleId.Should().Be(editedScheduleModel.ScheduleId);
			savedSchedule.ScheduleName.Should().Be(editedScheduleModel.ScheduleName);
			savedSchedule.Description.Should().Be(editedScheduleModel.Description);
			savedSchedule.JobName.Should().Be(editedScheduleModel.JobName);
			savedSchedule.Enabled.Should().Be(editedScheduleModel.Enabled);
			savedSchedule.OccursOnceAt.Should().Be(editedScheduleModel.DailyFrequencyStart.TimeOfDay.TotalMinutes);
			savedSchedule.TenantName.Should().Be(editedScheduleModel.Tenant);
			savedSchedule.ScheduleType.Should().Be(JobScheduleType.OccursDaily);
			savedSchedule.DataSourceId.Should().Be(editedScheduleModel.LogDataSourceId);

			savedSchedulePeriod.JobCategoryName.Should().Be(editedScheduleModel.RelativePeriods.Single().JobCategoryName);
			savedSchedulePeriod.RelativePeriod.Minimum.Should().Be(editedScheduleModel.RelativePeriods.Single().Start);
			savedSchedulePeriod.RelativePeriod.Maximum.Should().Be(editedScheduleModel.RelativePeriods.Single().End);
		}

		[Test]
		public void ShouldDisableScheduledJob()
		{
			var existingSchedule = new EtlJobSchedule(
				3,
				"My Schedule data Job",
				true,
				360,
				"My job",
				1,
				"Desc",
				null,
				null,
				"My tenant");
			JobScheduleRepository.SaveSchedule(existingSchedule);

			var result = Target.ToggleScheduleJob(existingSchedule.ScheduleId);

			result.Should().Be.OfType<OkResult>();

			var savedSchedule = JobScheduleRepository.GetSchedules(null, DateTime.MinValue).Single();
			savedSchedule.Enabled.Should().Be.False();
		}

		[Test]
		public void ShouldDeleteScheduledJob()
		{
			var existingSchedule = new EtlJobSchedule(
				3,
				"My Schedule data Job",
				true,
				360,
				"My job",
				1,
				"Desc",
				null,
				null,
				"My tenant");
			JobScheduleRepository.SaveSchedule(existingSchedule);

			var result = Target.DeleteScheduleJob(existingSchedule.ScheduleId);

			result.Should().Be.OfType<OkResult>();
			JobScheduleRepository.GetSchedules(null, DateTime.MinValue)
				.Any()
				.Should().Be.False();
		}

		[Test]
		public void ShouldGetBusinessUnitsForOneTenant()
		{
			var bu = new BusinessUnitItem
			{
				Id = Guid.NewGuid(),
				Name = "bu A"
			};
			JobHistoryRepository.AddBusinessUnits(new List<BusinessUnitItem>{bu}, connectionString);
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var result = (OkNegotiatedContentResult<IList<BusinessUnitItem>>)Target.BusinessUnits(testTenantName);

			result.Content.Single().Id.Should().Be(bu.Id);
			result.Content.Single().Name.Should().Be(bu.Name);
		}

		[Test]
		public void ShouldGetOnlyAllOptionForBusinessUnitsWhenAllTenants()
		{
			var result = (OkNegotiatedContentResult<IList<BusinessUnitItem>>)Target.BusinessUnits("<All>");

			result.Content.Single().Id.Should().Be(new Guid("00000000-0000-0000-0000-000000000002"));
			result.Content.Single().Name.Should().Be("<All>");
		}

		[Test]
		public void ShouldGetJobHistoryForOneBusinessUnit()
		{
			var buName = Guid.NewGuid();
			var bu = new BusinessUnitItem
			{
				Id = buName,
				Name = buName.ToString()
			};
			JobHistoryRepository.AddJobHistory(new JobHistoryViewModel()
			{
				BusinessUnitName = bu.Name,
				StartTime = new DateTime(2018,04,11),
			});
			var result = (OkNegotiatedContentResult<IList<JobHistoryViewModel>>)Target.GetJobHistory(new JobHistoryCriteria()
			{
				BusinessUnitId = bu.Id,
				StartDate = new DateTime(2018, 04, 11),
				EndDate = new DateTime(2018, 04, 11),
				ShowOnlyErrors = false,
				TenantName = "tenant"
			});
			result.Content.Count.Should().Be(1);
			result.Content.First().BusinessUnitName.Should().Be(bu.Name);
		}

		[Test]
		public void ShouldGetJobHistoryForAllBusinessUnitsForAGivenTenant()
		{
			var bu1 = Guid.NewGuid();
			var bu2 = Guid.NewGuid();
			var bunit1 = new BusinessUnitItem
			{
				Id = bu1,
				Name = bu1.ToString()
			};
			var bunit2 = new BusinessUnitItem
			{
				Id = bu2,
				Name = bu2.ToString()
			};
			JobHistoryRepository.AddBusinessUnits(new List<BusinessUnitItem> { bunit1, bunit2 }, connectionString);
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			JobHistoryRepository.AddJobHistory(new JobHistoryViewModel()
			{
				BusinessUnitName = bunit1.Name,
				StartTime = new DateTime(2018, 04, 11),
			});
			JobHistoryRepository.AddJobHistory(new JobHistoryViewModel()
			{
				BusinessUnitName = bunit2.Name,
				StartTime = new DateTime(2018, 04, 11),
			});
			var result = (OkNegotiatedContentResult<IList<JobHistoryViewModel>>)Target.GetJobHistory(new JobHistoryCriteria()
			{
				BusinessUnitId = new Guid("00000000-0000-0000-0000-000000000002"),
				StartDate = new DateTime(2018, 04, 11),
				EndDate = new DateTime(2018, 04, 11),
				ShowOnlyErrors = false,
				TenantName = testTenantName
			});
			result.Content.Count.Should().Be(2);
			result.Content.First().BusinessUnitName.Should().Be(bunit1.Name);
			result.Content.Last().BusinessUnitName.Should().Be(bunit2.Name);
		}

		[Test]
		public void ShouldGetJobHistoryForAllTenants()
		{
			var bu1 = Guid.NewGuid();
			var bu2 = Guid.NewGuid();
			var bunit1 = new BusinessUnitItem
			{
				Id = bu1,
				Name = bu1.ToString()
			};
			var bunit2 = new BusinessUnitItem
			{
				Id = bu2,
				Name = bu2.ToString()
			};
			JobHistoryRepository.AddBusinessUnits(new List<BusinessUnitItem> { bunit1, bunit2 }, connectionString);
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);
			JobHistoryRepository.AddJobHistory(new JobHistoryViewModel()
			{
				BusinessUnitName = bunit1.Name,
				StartTime = new DateTime(2018, 04, 11),
			});
			JobHistoryRepository.AddJobHistory(new JobHistoryViewModel()
			{
				BusinessUnitName = bunit2.Name,
				StartTime = new DateTime(2018, 04, 11),
			});
			var result = (OkNegotiatedContentResult<IList<JobHistoryViewModel>>)Target.GetJobHistory(new JobHistoryCriteria()
			{
				BusinessUnitId = new Guid("00000000-0000-0000-0000-000000000002"),
				StartDate = new DateTime(2018, 04, 11),
				EndDate = new DateTime(2018, 04, 11),
				ShowOnlyErrors = false,
				TenantName = "<All>"
			});
			result.Content.Count.Should().Be(2);
			result.Content.First().BusinessUnitName.Should().Be(bunit1.Name);
			result.Content.Last().BusinessUnitName.Should().Be(bunit2.Name);
		}

		private IEnumerable<JobCollectionModel> getJobs(string tenantName, bool pmInstalled)
		{
			ConfigReader.FakeConnectionString("Hangfire", connectionString);


			ConfigurationHandler.AddBaseConfiguration(connectionString, new BaseConfiguration(1053, 15, timezoneName, false));
			AllTenants.HasWithAnalyticsConnectionString(testTenantName, connectionString);

			var localToday = new DateTime(2017, 12, 11);
			var utcToday = TimeZoneHelper.ConvertToUtc(localToday, TimeZoneInfo.FindSystemTimeZoneById(timezoneName));
			Now.Is(utcToday);
			PmInfoProvider.SetPmInstalled(pmInstalled);

			var result = Target.Jobs(tenantName);
			result.Should().Be.OfType<OkNegotiatedContentResult<IList<JobCollectionModel>>>();

			var jobCollection = result as OkNegotiatedContentResult<IList<JobCollectionModel>>;
			Assert.NotNull(jobCollection);

			var jobs = jobCollection.Content;

			var expectedJobCount = basicJobCount + 1 + (pmInstalled ? 1 : 0);
			jobs.Count.Should().Be(expectedJobCount);

			jobs.Any(j => j.JobName == reloadDatamartJobName).Should().Be.True();
			jobs.Any(j => j.JobName == processCubeJobName).Should().Be.EqualTo(pmInstalled);

			return jobs;
		}

		[Test]
		public void ShouldFailToEnqueStatisticsJobsPossiblyDueToTimeoutOnQueueStats()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));
			var period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), (new DateOnly(Now.UtcDateTime())));
			GeneralInfrastructure.HasFactQueuePeriod(dataSourceId, period);
			GeneralInfrastructure.HasFactAgentPeriod(dataSourceId, period);
			GeneralInfrastructure.GetFactQueuePeriodThrowException = true;

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				Name = "myDs",
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSources);
			result.StatusCode.Should().Be(HttpStatusCode.Ambiguous);
			result.Content.Should().Contain(myDsModel.Name);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.Now);
			scheduledJobs.Count.Should().Be(1);
			scheduledJobs.Single().JobName.Should().Be("Initial");
		}

		[Test]
		public void ShouldFailToEnqueStatisticsJobsPossiblyDueToTimeoutOnAgentStats()
		{
			const int dataSourceId = 3;
			var masterTenant = new Tenant(testTenantName);
			masterTenant.DataSourceConfiguration.SetAnalyticsConnectionString($"Initial Catalog={RandomName.Make()}");
			AllTenants.HasWithAnalyticsConnectionString(masterTenant.Name, masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			BaseConfigurationRepository.SaveBaseConfiguration(masterTenant.DataSourceConfiguration.AnalyticsConnectionString,
				new BaseConfiguration(1053, 15, "UTC", false));
			ConfigReader.FakeConnectionString("Hangfire", masterTenant.DataSourceConfiguration.AnalyticsConnectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(dataSourceId, "myDs", 12, timezoneName, 15, false));
			var period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), (new DateOnly(Now.UtcDateTime())));
			GeneralInfrastructure.HasFactQueuePeriod(dataSourceId, period);
			GeneralInfrastructure.HasFactAgentPeriod(dataSourceId, period);
			GeneralInfrastructure.GetFactAgentPeriodThrowException = true;

			var myDsModel = new DataSourceModel
			{
				Id = dataSourceId,
				Name = "myDs",
				TimeZoneCode = "UTC"
			};
			var tenantDataSources = new TenantDataSourceModel
			{
				TenantName = testTenantName,
				DataSources = new List<DataSourceModel> { myDsModel }
			};

			var result = (NegotiatedContentResult<string>)Target.PersistDataSource(tenantDataSources);
			result.StatusCode.Should().Be(HttpStatusCode.Ambiguous);
			result.Content.Should().Contain(myDsModel.Name);

			var scheduledJobs = JobScheduleRepository.GetSchedules(null, DateTime.Now);
			scheduledJobs.Count.Should().Be(1);
			scheduledJobs.Single().JobName.Should().Be("Initial");
		}
	}
}

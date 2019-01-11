using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web.Http;
using log4net;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Configuration;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Toggle;

using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	/// <summary>
	/// Changes to this file may affect third party connections, i.e. Twillio, TalkDesk etc.
	/// Please contact CloudOps when changes are required and made. 
	/// </summary>
	[TenantTokenAuthentication]
	public class EtlController : ApiController
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(EtlController));
		private readonly IToggleManager _toggleManager;
		private readonly JobCollectionModelProvider _jobCollectionModelProvider;
		private readonly TenantLogDataSourcesProvider _tenantLogDataSourcesProvider;
		private readonly EtlJobScheduler _etlJobScheduler;
		private readonly IConfigReader _configReader;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IConfigurationHandler _configurationHandler;
		private readonly IGeneralFunctions _generalFunctions;
		private readonly INow _now;
		private readonly IJobHistoryRepository _jobHistoryRepository;
		private readonly BaseConfigurationValidator _baseConfigurationValidator;
		private const string allBusinessUnitId = "00000000-0000-0000-0000-000000000002";


		public EtlController(IToggleManager toggleManager,
			JobCollectionModelProvider jobCollectionModelProvider,
			TenantLogDataSourcesProvider tenantLogDataSourcesProvider,
			EtlJobScheduler etlJobScheduler,
			IConfigReader configReader,
			IBaseConfigurationRepository baseConfigurationRepository,
			ILoadAllTenants loadAllTenants,
			IConfigurationHandler configurationHandler,
			IGeneralFunctions generalFunctions, 
			INow now,
			IJobHistoryRepository jobHistoryRepository, BaseConfigurationValidator baseConfigurationValidator)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
			_tenantLogDataSourcesProvider = tenantLogDataSourcesProvider;
			_etlJobScheduler = etlJobScheduler;
			_configReader = configReader;
			_baseConfigurationRepository = baseConfigurationRepository;
			_loadAllTenants = loadAllTenants;
			_configurationHandler = configurationHandler;
			_generalFunctions = generalFunctions;
			_now = now;
			_jobHistoryRepository = jobHistoryRepository;
			_baseConfigurationValidator = baseConfigurationValidator;
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/Jobs")]
		public virtual IHttpActionResult Jobs([FromBody] string tenantName)
		{
			try
			{
				return Ok(_jobCollectionModelProvider.Create(tenantName));
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/TenantValidLogDataSources")]
		public virtual IHttpActionResult TenantValidLogDataSources([FromBody] string tenantName)
		{
			try
			{
				return Ok(_tenantLogDataSourcesProvider.Load(tenantName, false, true));
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/TenantAllLogDataSources")]
		public virtual IHttpActionResult TenantAllLogDataSources([FromBody] string tenantName)
		{
			try
			{
				return Ok(_tenantLogDataSourcesProvider.Load(tenantName, true, true));
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/TenantLogDataSources")]
		public virtual IHttpActionResult TenantLogDataSources([FromBody] string tenantName)
		{
			try
			{
				var tenant = _loadAllTenants.Tenants().Single(x => x.Name == tenantName);
				var analyticsConnectionString = tenant.DataSourceConfiguration.AnalyticsConnectionString;
				_generalFunctions.SetConnectionString(analyticsConnectionString);
				_generalFunctions.LoadNewDataSources();
				return Ok(_tenantLogDataSourcesProvider.Load(tenantName, true, false));
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/EnqueueJob")]
		public virtual IHttpActionResult EnqueueJob(EtlJobEnqueModel jobEnqueModel)
		{
			try
			{
				_etlJobScheduler.ScheduleManualJob(jobEnqueModel);
				return Ok();
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Etl/IsBaseConfigurationAvailable")]
		public virtual IHttpActionResult IsBaseConfigurationAvailable()
		{
			var connectionString = _configReader.ConnectionString("Hangfire");
			var baseConfiguration = _baseConfigurationRepository.LoadBaseConfiguration(connectionString);
			var isValid = _baseConfigurationValidator.IsConfigurationValid(baseConfiguration);

			var tenantName = getMasterTenantName();

			return Ok(new TenantConfigurationModel
			{
				TenantName = tenantName,
				IsBaseConfigured = isValid
			});
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/SaveConfigurationForTenant")]
		public virtual IHttpActionResult SaveConfigurationForTenant(TenantConfigurationModel tenantConfigurationModel)
		{
			var tenant = _loadAllTenants.Tenants().SingleOrDefault(x => x.Name == tenantConfigurationModel.TenantName);
			if (tenant == null)
			{
				return Content(HttpStatusCode.NotFound, $"Tenant with name {tenantConfigurationModel.TenantName} does not exists");
			}

			var connectionString = tenant.DataSourceConfiguration.AnalyticsConnectionString;
			try
			{
				_baseConfigurationRepository.SaveBaseConfiguration(connectionString, tenantConfigurationModel.BaseConfig);
				_etlJobScheduler.ScheduleManualJob(createJobToEnqueue(tenantConfigurationModel.TenantName, "Initial",
					JobCategoryType.Initial, 1, _now.UtcDateTime().AddDays(-1), _now.UtcDateTime().AddDays(1)).First());
			}
			catch (Exception ex)
			{
				logger.Error(
					$"Error occurred on save changes for Base Configuration for tenant \"{tenantConfigurationModel.TenantName}\"", ex);
				return Content(HttpStatusCode.InternalServerError, "Error occurred on save base configuration.");
			}

			return Ok();
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Etl/GetTenants")]
		public virtual IHttpActionResult GetTenants()
		{
			var tenants = new List<TenantConfigurationModel>();
			foreach (var tenant in _loadAllTenants.Tenants())
			{
				var analyticsConnectionString =
					new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AnalyticsConnectionString).ToString();

				var baseConfig = _baseConfigurationRepository.LoadBaseConfiguration(analyticsConnectionString);
				var isValid = _baseConfigurationValidator.IsConfigurationValid(baseConfig);

				tenants.Add(new TenantConfigurationModel
				{
					TenantName = tenant.Name,
					BaseConfig = (BaseConfiguration) baseConfig,
					IsBaseConfigured = isValid
				});
			}

			return Ok(tenants);
		}

		[HttpGet, Route("Etl/GetConfigurationModel")]
		public virtual IHttpActionResult GetConfigurationModel()
		{
			var configurationModel = new StartupConfigurationModel(_configurationHandler);
			return Ok(new TenantConfigurationOption
			{
				CultureList = configurationModel.CultureList.ToList(),
				IntervalLengthList = configurationModel.IntervalLengthList.ToList(),
				TimeZoneList = configurationModel.TimeZoneList.ToList()
			});
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/PersistDataSource")]
		public virtual IHttpActionResult PersistDataSource(TenantDataSourceModel tenantDataSourceModel)
		{
			var tenantName = tenantDataSourceModel.TenantName;
			var dataSourceNotPersisted = new List<string>();
			var initialJobEnqueued = false;
			var etlJobsToEnque = new List<EtlJobEnqueModel>();

			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Persist changes for data source {string.Join(", ", tenantDataSourceModel.DataSources.Select(x => x.Id))} "
					+ "of tenant \"{tenantDataSourceModel.TenantName}\"");
			}

			foreach (var dataSourceModel in tenantDataSourceModel.DataSources)
			{
				var dataSourceId = dataSourceModel.Id;

				// To exclude default data source
				if (dataSourceId <= 1)
				{
					dataSourceNotPersisted.Add(dataSourceModel.Name);
					logger.Info($"Datasource with id {dataSourceId} for Tenant \"{tenantName}\" is not allowed to modify");
					continue;
				}

				var dataSource = _tenantLogDataSourcesProvider.Load(tenantName, true, false).SingleOrDefault(x => x.Id == dataSourceId);
				if (dataSource == null)
				{
					dataSourceNotPersisted.Add(dataSourceModel.Name);
					logger.Info($"Datasource with id {dataSourceId} for Tenant \"{tenantName}\" not found");
					continue;
				}

				if (dataSource.TimeZoneCode == dataSourceModel.TimeZoneCode)
				{
					logger.Info(
						$"No change was made to datasource \"{dataSource.Name}\" with id {dataSourceId} for Tenant \"{tenantName}\" "
						+ "since timezone is not changed.");
					continue;
				}

				try
				{
					var tenant = _loadAllTenants.Tenants().Single(x => x.Name == tenantName);
					var analyticsConnectionString = tenant.DataSourceConfiguration.AnalyticsConnectionString;
					_generalFunctions.SetConnectionString(analyticsConnectionString);

					var timeZondeId = _generalFunctions.GetTimeZoneDim(dataSourceModel.TimeZoneCode).MartId;
					_generalFunctions.SaveDataSource(dataSourceId, timeZondeId);

					if (!initialJobEnqueued)
					{
						if (logger.IsDebugEnabled)
						{
							logger.Debug("Enqueue ETL job \"Initial\" since timezone chaned.");
						}

						etlJobsToEnque.Add(createJobToEnqueue(tenantName, "Initial", JobCategoryType.Initial, 1,
							_now.UtcDateTime().AddDays(-1), _now.UtcDateTime().AddDays(1)).First());
						initialJobEnqueued = true;
					}

					var queuePeriod = _generalFunctions.GetFactQueueAvailablePeriod(dataSourceId);
					var agentPeriod = _generalFunctions.GetFactAgentAvailablePeriod(dataSourceId);

					if (queuePeriod.EndDate != DateOnly.MinValue)
					{
						if (logger.IsDebugEnabled)
						{
							logger.Debug(
								$"Enqueue ETL job \"Queue Statistics\" from {queuePeriod.StartDate.Date:yyyy-MM-dd} to "
								+ $"{queuePeriod.EndDate.Date:yyyy-MM-dd} for data source with Id=\"{dataSourceId}\" and tenant=\"{tenantName}\".");
						}

						etlJobsToEnque.AddRange(createJobToEnqueue(tenantName, "Queue Statistics", JobCategoryType.QueueStatistics,
							dataSourceId, queuePeriod.StartDate.Date, queuePeriod.EndDate.Date));
					}
					
					if (agentPeriod.EndDate != DateOnly.MinValue)
					{
						if (logger.IsDebugEnabled)
						{
							logger.Debug(
								$"Enqueue ETL job \"Agent Statistics\" from {agentPeriod.StartDate.Date:yyyy-MM-dd} to "
								+ $"{agentPeriod.EndDate.Date:yyyy-MM-dd} for data source with Id=\"{dataSourceId}\" and tenant=\"{tenantName}\".");
						}

						etlJobsToEnque.AddRange(createJobToEnqueue(tenantName, "Agent Statistics", JobCategoryType.AgentStatistics,
							dataSourceId, agentPeriod.StartDate.Date, agentPeriod.EndDate.Date));
					}
				}
				catch (Exception ex)
				{
					dataSourceNotPersisted.Add(dataSourceModel.Name);
					logger.Error($"Error occurred on save changes for Datasource with id {dataSourceId} for Tenant \"{tenantName}\"",
						ex);
				}
			}

			foreach (var job in etlJobsToEnque)
			{
				_etlJobScheduler.ScheduleManualJob(job);
			}

			return dataSourceNotPersisted.Any()
				? (IHttpActionResult) Content(HttpStatusCode.Ambiguous,
					$"Failed to save {dataSourceNotPersisted.Count} of {tenantDataSourceModel.DataSources.Count} data source(s): "
					+ $"{string.Join(", ", dataSourceNotPersisted)}.")
				: Ok();
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Etl/ScheduledJobs")]
		public virtual IHttpActionResult ScheduledJobs()
		{
			var scheduledJobs = _etlJobScheduler.LoadScheduledJobs();
			var jobs = scheduledJobs
				.Where(x => x.ScheduleType != JobScheduleType.Manual)
				.Select(job =>
				{
					var isScheduledDaily = job.ScheduleType == JobScheduleType.OccursDaily;
					var dailyFrequencyStart =
						DateTime.MinValue.AddMinutes(isScheduledDaily ? job.OccursOnceAt : job.OccursEveryMinuteStartingAt);
					var dailyFrequencyEnd = DateTime.MinValue.AddMinutes(isScheduledDaily ? 0 : job.OccursEveryMinuteEndingAt);
					var dailyFrequencyMinute = isScheduledDaily ? string.Empty : job.OccursEveryMinute.ToString();

					return new EtlScheduleJobModel
					{
						ScheduleId = job.ScheduleId,
						ScheduleName = job.ScheduleName,
						Description = job.Description,
						JobName = job.JobName,
						Enabled = job.Enabled,
						Tenant = job.TenantName,
						LogDataSourceId = job.DataSourceId,
						DailyFrequencyStart = dailyFrequencyStart,
						DailyFrequencyEnd = dailyFrequencyEnd,
						DailyFrequencyMinute = dailyFrequencyMinute,
						RelativePeriods = job.RelativePeriodCollection
							.Select(x => new JobPeriodRelative
							{
								JobCategoryName = x.JobCategory.ToString(),
								Start = x.RelativePeriod.Minimum,
								End = x.RelativePeriod.Maximum
							})
							.ToArray()
					};
				})
				.ToList();

			return Ok(jobs);
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/ScheduleJob")]
		public virtual IHttpActionResult ScheduleJob(EtlScheduleJobModel scheduleModel)
		{
			scheduleModel.ScheduleId = -1;
			saveScheduleJob(scheduleModel);

			return Ok();
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/EditScheduleJob")]
		public virtual IHttpActionResult EditScheduleJob(EtlScheduleJobModel scheduleModel)
		{
			saveScheduleJob(scheduleModel);
			return Ok();
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/ToggleScheduleJob")]
		public virtual IHttpActionResult ToggleScheduleJob([FromBody]  int scheduleId)
		{
			_etlJobScheduler.ToggleScheduleJobEnabledState(scheduleId);
			return Ok();
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/DeleteScheduleJob")]
		public virtual IHttpActionResult DeleteScheduleJob([FromBody] int scheduleId)
		{
			_etlJobScheduler.DeleteScheduleJob(scheduleId);
			return Ok();
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/BusinessUnits")]
		public virtual IHttpActionResult BusinessUnits([FromBody] string tenantName)
		{
			if (Tenants.IsAllTenants(tenantName))
			{
				IList<BusinessUnitItem> allBuList = new List<BusinessUnitItem>
				{
					new BusinessUnitItem
					{
						Id = new Guid(allBusinessUnitId),
						Name = Tenants.NameForOptionAll
					}
				};
				return Ok(allBuList);
			}

			var tenant = _loadAllTenants.Tenants().Single(x => x.Name == tenantName);
			return Ok(_jobHistoryRepository.GetBusinessUnitsIncludingAll(tenant.DataSourceConfiguration.AnalyticsConnectionString));
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/GetJobHistory")]
		public virtual IHttpActionResult GetJobHistory(JobHistoryCriteria jobHistoryCriteria)
		{
			var connectionString = _configReader.ConnectionString("Hangfire");
			var businessUnitIds =  getBusinessUnitIds(jobHistoryCriteria.TenantName, jobHistoryCriteria.BusinessUnitId);
			if(businessUnitIds.Any())
				return Ok(_jobHistoryRepository.GetEtlJobHistory(jobHistoryCriteria.StartDate, jobHistoryCriteria.EndDate,
				businessUnitIds, jobHistoryCriteria.ShowOnlyErrors, connectionString));
			return Ok();
		}

		[HttpGet, Route("Etl/GetjobRunning")]
		public virtual IHttpActionResult GetjobRunning()
		{
			var connectionString = _configReader.ConnectionString("Hangfire");
			var runningStatus = new RunningStatusRepository(connectionString);
			return Ok(runningStatus.GetRunningJob());
		}

		private void saveScheduleJob(EtlScheduleJobModel scheduleModel)
		{
			if (string.IsNullOrEmpty(scheduleModel.DailyFrequencyMinute))
			{
				_etlJobScheduler.ScheduleDailyJob(scheduleModel);
			}
			else
			{
				_etlJobScheduler.SchedulePeriodicJob(scheduleModel);
			}
		}

		private static IEnumerable<EtlJobEnqueModel> createJobToEnqueue(string tenantName, string jobName,
			JobCategoryType jobCategoryName, int datasourceId, DateTime startDate, DateTime endDate)
		{
			const int periodLengthInDay = 30;

			var jobs = new List<EtlJobEnqueModel>();
			for (var start = startDate; start <= endDate; start = start.AddDays(periodLengthInDay))
			{
				var nextStart = start.AddDays(periodLengthInDay);
				var jobPeriodDate = new JobPeriodDate
				{
					Start = start,
					End = nextStart > endDate ? endDate : nextStart,
					JobCategoryName = jobCategoryName.ToString()
				};
				var jobEnqueueModel = new EtlJobEnqueModel
				{
					JobName = jobName,
					JobPeriods = new List<JobPeriodDate> {jobPeriodDate},
					LogDataSourceId = datasourceId,
					TenantName = tenantName
				};
				jobs.Add(jobEnqueueModel);
			}

			return jobs;
		}

		private string getMasterTenantName()
		{
			var masterTenantInitialCatalog =
				new SqlConnectionStringBuilder(_configReader.ConnectionString("Tenancy")).InitialCatalog;
			var master = _loadAllTenants.Tenants().SingleOrDefault(x =>
			{
				var tenantInitialCatalog = new SqlConnectionStringBuilder(x.DataSourceConfiguration.ApplicationConnectionString)
					.InitialCatalog;
				return tenantInitialCatalog.Equals(masterTenantInitialCatalog);
			});

			return master?.Name;
		}

		private List<Guid> getBusinessUnitIds(string tenantName, Guid businessUnitId)
		{
			if (businessUnitId != new Guid(allBusinessUnitId) || Tenants.IsAllTenants(tenantName))
				return new List<Guid> {businessUnitId};
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name == tenantName);
			return _jobHistoryRepository.GetBusinessUnitsIncludingAll(tenant.DataSourceConfiguration.AnalyticsConnectionString)
				.Where(x => x.Id != new Guid(allBusinessUnitId))
				.Select(x => x.Id).ToList();
		}
	}

	public class JobHistoryCriteria
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool ShowOnlyErrors { get; set; }
		public string TenantName { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using log4net;
using Teleopti.Analytics.Etl.Common.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
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

		public EtlController(IToggleManager toggleManager, 
			JobCollectionModelProvider jobCollectionModelProvider,
			TenantLogDataSourcesProvider tenantLogDataSourcesProvider,
			EtlJobScheduler etlJobScheduler,
			IConfigReader configReader,
			IBaseConfigurationRepository baseConfigurationRepository,
			ILoadAllTenants loadAllTenants,
			IConfigurationHandler configurationHandler,
			IGeneralFunctions generalFunctions)
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
		}

		[HttpGet, Route("Etl/ShouldEtlToolBeVisible")]
		public IHttpActionResult ShouldEtlToolBeVisible()
		{
			return Json(_toggleManager.IsEnabled(Toggles.ETL_Show_Web_Based_ETL_Tool_46880));
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
				return Ok(_tenantLogDataSourcesProvider.Load(tenantName));
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
				_generalFunctions.LoadNewDataSources();
				return Ok(_tenantLogDataSourcesProvider.Load(tenantName, true));
			}
			catch (ArgumentException e)
			{
				return Content(HttpStatusCode.NotFound, e.Message);
			}
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/EnqueueJob")]
		public virtual IHttpActionResult EnqueueJob(JobEnqueModel jobEnqueModel)
		{
			try
			{
				_etlJobScheduler.ScheduleJob(jobEnqueModel);
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
			_configurationHandler.SetConnectionString(connectionString);
			var tenantName = getMasterTenantName();

			return Ok(new TenantConfigurationModel
			{
				TenantName = tenantName,
				IsBaseConfigured = _configurationHandler.IsConfigurationValid
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
				enqueueInitialJob(tenantConfigurationModel.TenantName);
			}
			catch (Exception ex)
			{
				logger.Error($"Error occurred on save changes for Base Configuration for tenant \"{tenantConfigurationModel.TenantName}\"", ex);
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
				
				_configurationHandler.SetConnectionString(analyticsConnectionString);
				var baseConfig = _configurationHandler.BaseConfiguration;
				tenants.Add(new TenantConfigurationModel()
				{
					TenantName = tenant.Name,
					BaseConfig = (BaseConfiguration)baseConfig,
					IsBaseConfigured = _configurationHandler.IsConfigurationValid
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
			var dataSourceId = tenantDataSourceModel.DataSource.Id;

			// To exclude default data source
			if (dataSourceId <= 1)
			{
				return Content(HttpStatusCode.Forbidden,
					$"Datasource with id {dataSourceId} for Tenant \"{tenantName}\" is not allowed to modify");
			}

			var dataSource = _tenantLogDataSourcesProvider.Load(tenantName, true).SingleOrDefault(x => x.Id == dataSourceId);
			if (dataSource == null)
			{
				return Content(HttpStatusCode.NotFound, $"Datasource with id {dataSourceId} for Tenant \"{tenantName}\" not found");
			}

			if (dataSource.TimeZoneId == tenantDataSourceModel.DataSource.TimeZoneId)
			{
				return Content(HttpStatusCode.NotModified,
					$"No change was made to datasource \"{dataSource.Name}\" with id {dataSourceId} for Tenant \"{tenantName}\" since timezone is not changed.");
			}

			try
			{
				_generalFunctions.SaveDataSource(dataSourceId, tenantDataSourceModel.DataSource.TimeZoneId);

				enqueueInitialJob(tenantName);
			}
			catch (Exception ex)
			{
				logger.Error($"Error occurred on save changes for Datasource with id {dataSourceId} for Tenant \"{tenantName}\"", ex);
				return Content(HttpStatusCode.InternalServerError, "Error occurred on save data source changes.");
			}

			return Ok();
		}

		private void enqueueInitialJob(string tenantName)
		{
			var utcToday = DateTime.UtcNow.Date;
			var jobEnqueModel = new JobEnqueModel
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
				LogDataSourceId = 1,
				TenantName = tenantName
			};
			_etlJobScheduler.ScheduleJob(jobEnqueModel);
		}

		private string getMasterTenantName()
		{
			var appConnectionString = new SqlConnectionStringBuilder(_configReader.ConnectionString("Tenancy")).InitialCatalog;
			var master = _loadAllTenants.Tenants().SingleOrDefault(x =>
				new SqlConnectionStringBuilder(x.DataSourceConfiguration.ApplicationConnectionString).InitialCatalog.Equals(appConnectionString));

			return master?.Name;
		}
	}
}


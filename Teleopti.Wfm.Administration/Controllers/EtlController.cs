using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
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
		private readonly IToggleManager _toggleManager;
		private readonly JobCollectionModelProvider _jobCollectionModelProvider;
		private readonly TenantLogDataSourcesProvider _tenantLogDataSourcesProvider;
		private readonly EtlJobScheduler _etlJobScheduler;
		private readonly IConfigReader _configReader;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IConfigurationHandler _configurationHandler;

		public EtlController(IToggleManager toggleManager, 
			JobCollectionModelProvider jobCollectionModelProvider,
			TenantLogDataSourcesProvider tenantLogDataSourcesProvider,
			EtlJobScheduler etlJobScheduler,
			IConfigReader configReader,
			IBaseConfigurationRepository baseConfigurationRepository, ILoadAllTenants loadAllTenants, IConfigurationHandler configurationHandler)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
			_tenantLogDataSourcesProvider = tenantLogDataSourcesProvider;
			_etlJobScheduler = etlJobScheduler;
			_configReader = configReader;
			_baseConfigurationRepository = baseConfigurationRepository;
			_loadAllTenants = loadAllTenants;
			_configurationHandler = configurationHandler;
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
		[HttpPost, Route("Etl/TenantLogDataSources")]
		public virtual IHttpActionResult TenantLogDataSources([FromBody] string tenantName)
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

		[HttpGet, Route("Etl/IsBaseConfigurationAvailable")]
		public virtual IHttpActionResult IsBaseConfigurationAvailable()
		{
			var connectionString = _configReader.ConnectionString("Hangfire");
			var baseConfig = _baseConfigurationRepository.LoadBaseConfiguration(connectionString);
			var isConfig = baseConfig.IntervalLength.HasValue;
			return Ok(new TenantConfigurationModel()
			{
				ConnectionString = connectionString,
				IsBaseConfigured = isConfig
			});
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/SaveConfigurationForTenant")]
		public virtual IHttpActionResult SaveConfigurationForTenant(TenantConfigurationModel tenantConfigurationModel)
		{
			_baseConfigurationRepository.SaveBaseConfiguration(tenantConfigurationModel.ConnectionString, tenantConfigurationModel.BaseConfig);
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
					ConnectionString = analyticsConnectionString,
					BaseConfig = baseConfig,
					IsBaseConfigured = _configurationHandler.IsConfigurationValid
				});
			}

			return Ok(tenants);
		}
	}
}


using System;
using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
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

		public EtlController(IToggleManager toggleManager, 
			JobCollectionModelProvider jobCollectionModelProvider,
			TenantLogDataSourcesProvider tenantLogDataSourcesProvider,
			EtlJobScheduler etlJobScheduler)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
			_tenantLogDataSourcesProvider = tenantLogDataSourcesProvider;
			_etlJobScheduler = etlJobScheduler;
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
	}
}


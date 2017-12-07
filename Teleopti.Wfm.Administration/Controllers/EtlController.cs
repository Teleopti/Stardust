using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class EtlController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly JobCollectionModelProvider _jobCollectionModelProvider;
		private readonly TenantLogDataSourcesProvider _tenantLogDataSourcesProvider;

		public EtlController(IToggleManager toggleManager, JobCollectionModelProvider jobCollectionModelProvider, TenantLogDataSourcesProvider tenantLogDataSourcesProvider)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
			_tenantLogDataSourcesProvider = tenantLogDataSourcesProvider;
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
			return Json(_jobCollectionModelProvider.Create(tenantName));
		}

		[TenantUnitOfWork]
		[HttpPost, Route("Etl/TenantLogDataSources")]
		public virtual IHttpActionResult TenantLogDataSources([FromBody] string tenantName)
		{
			return Json(_tenantLogDataSourcesProvider.Load(tenantName));
		}
	}
}

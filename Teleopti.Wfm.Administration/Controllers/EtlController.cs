using System.Linq;
using System.Web.Http;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class EtlController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly EtlToolJobCollectionModelProvider _jobCollectionModelProvider;
		private readonly ILoadAllTenants _loadAllTenants;

		public EtlController(IToggleManager toggleManager, EtlToolJobCollectionModelProvider jobCollectionModelProvider, ILoadAllTenants loadAllTenants)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
			_loadAllTenants = loadAllTenants;
		}

		[HttpGet, Route("Etl/ShouldEtlToolBeVisible")]
		public IHttpActionResult ShouldEtlToolBeVisible()
		{
			return Json(_toggleManager.IsEnabled(Toggles.ETL_Show_Web_Based_ETL_Tool_46880));
		}

		[HttpGet, Route("Etl/Jobs")]
		public IHttpActionResult Jobs()
		{
			return Json(_jobCollectionModelProvider.Create());
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("Etl/GetSelectedLogDataSources")]
		public virtual IHttpActionResult GetSelectedLogDataSources([FromBody]string tenantName)
		{
			var analyticsConnection = _loadAllTenants
				.Tenants()
				.FirstOrDefault(x => x.Name == tenantName)
				?.DataSourceConfiguration.AnalyticsConnectionString;
			var generalFunc = new GeneralFunctions(analyticsConnection, new BaseConfigurationRepository());
			return Json(generalFunc.DataSourceValidListIncludedOptionAll);
		}
	}
}

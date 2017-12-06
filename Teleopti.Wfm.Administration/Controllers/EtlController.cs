using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
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

		public EtlController(IToggleManager toggleManager, EtlToolJobCollectionModelProvider jobCollectionModelProvider)
		{
			_toggleManager = toggleManager;
			_jobCollectionModelProvider = jobCollectionModelProvider;
		}

		[HttpGet, Route("Etl/ShouldEtlToolBeVisible")]
		public IHttpActionResult ShouldEtlToolBeVisible()
		{
			return Json(_toggleManager.IsEnabled(Toggles.ETL_Show_Web_Based_ETL_Tool_46880));
		}

		[HttpPost, Route("Etl/Jobs")]
		public IHttpActionResult Jobs([FromBody] string tenantName)
		{
			return Json(_jobCollectionModelProvider.Create(tenantName));
		}
	}
}

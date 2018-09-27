using System.Web.Http;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class RtaPermissionsController : ApiController
	{
		[HttpGet, Route("api/RtaPermissions/Load")]
		public virtual IHttpActionResult Load()
			=> Ok(new RtaPermissions());
	}

	public class RtaPermissions
	{
		public bool HasHistoricalOverviewPermission = true;
	}
}
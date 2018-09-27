using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class RtaPermissionsController : ApiController
	{		
		private readonly ICurrentAuthorization _authorization;

		public RtaPermissionsController(ICurrentAuthorization authorization)
		{
			_authorization = authorization;
		}

		[UnitOfWork, HttpGet, Route("api/RtaPermissions/Load")]
		public virtual IHttpActionResult Load()
		{
			var permissions = new RtaPermissions
			{
				HasHistoricalOverviewPermission = _authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.HistoricalOverview)
			};

			return Ok(permissions);
		}

	}

	public class RtaPermissions
	{
		public bool HasHistoricalOverviewPermission;
	}
}
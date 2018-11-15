using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class PermissionsController : ApiController
	{
		private readonly PermissionsViewModelBuilder _builder;

		public PermissionsController(PermissionsViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpGet, Route("api/Adherence/Permissions")]
		public virtual IHttpActionResult Load()
		{
			return Ok(_builder.Build());
		}
	}

	public class PermissionsViewModelBuilder
	{
		private readonly ICurrentAuthorization _authorization;

		public PermissionsViewModelBuilder(ICurrentAuthorization authorization)
		{
			_authorization = authorization;
		}

		public PermissionsViewModel Build()
		{
			return new PermissionsViewModel
			{
				HasHistoricalOverviewPermission = _authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.HistoricalOverview)
			};
		}
	}

	public class PermissionsViewModel
	{
		public bool HasHistoricalOverviewPermission;
	}
}